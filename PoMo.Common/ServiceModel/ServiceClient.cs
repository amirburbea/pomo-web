using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using System.Threading.Tasks;
using PoMo.Common.DataObjects;
using PoMo.Common.ServiceModel.Contracts;
using PoMo.Common.System;

namespace PoMo.Common.ServiceModel
{
    public interface IServiceChannelManager
    {
        event EventHandler<TicksReceivedEventArgs> TicksReceived;

        IServiceContract Channel
        {
            get;
        }
    }

    public sealed class ServiceChannelManager : IServiceChannelManager, IDisposable
    {
        private readonly DuplexChannelFactory<IServiceContract> _factory;
        private IServiceContract _channel;
        private bool _isDisposed;

        public ServiceChannelManager(Binding binding)
        {
            this._factory = new DuplexChannelFactory<IServiceContract>(new InstanceContext(new Callback(this)), binding);
        }

        public event EventHandler<TicksReceivedEventArgs> TicksReceived;

        public IServiceContract Channel
        {
            get
            {
                if (this._isDisposed)
                {
                    return null;
                }
                lock (this._factory)
                {
                    return this._channel ?? (this._channel = this.CreateChannel());
                }
            }
        }

        public void Dispose()
        {
            if (this._isDisposed)
            {
                return;
            }
            lock (this._factory)
            {
                ServiceChannelManager.TryDisposeCommunicationObject(Interlocked.Exchange(ref this._channel, null));
            }
            ServiceChannelManager.TryDisposeCommunicationObject(this._factory);
            this._isDisposed = true;
        }

        private static void TryDisposeCommunicationObject(object obj)
        {
            ICommunicationObject communicationObject = obj as ICommunicationObject;
            if (communicationObject == null ||
                communicationObject.State == CommunicationState.Faulted ||
                communicationObject.State == CommunicationState.Closed)
            {
                return;
            }
            try
            {
                (obj as IDisposable)?.Dispose();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        private void Channel_ClosedOrFaulted(object sender, EventArgs e)
        {
            ICommunicationObject obj = (ICommunicationObject)sender;
            obj.Closed -= this.Channel_ClosedOrFaulted;
            obj.Faulted -= this.Channel_ClosedOrFaulted;
            lock (this._factory)
            {
                Interlocked.CompareExchange(ref this._channel, null, (IServiceContract)sender);
            }
        }

        private IServiceContract CreateChannel()
        {
            IServiceContract channel = this._factory.CreateChannel();
            ICommunicationObject obj = (ICommunicationObject)channel;
            obj.Closed += this.Channel_ClosedOrFaulted;
            obj.Faulted += this.Channel_ClosedOrFaulted;
            return channel;
        }

        private sealed class Callback : IClientContract
        {
            private readonly ServiceChannelManager _channelManager;

            public Callback(ServiceChannelManager channelManager)
            {
                this._channelManager = channelManager;
            }

            IAsyncResult IClientContract.BeginReceiveTicks(string portfolioId, TickData[] data, AsyncCallback callback, object state)
            {
                return Task.Factory.StartNew(
                    arg => this._channelManager.TicksReceived?.Invoke(this._channelManager, (TicksReceivedEventArgs)arg),
                    new TicksReceivedEventArgs(portfolioId, data),
                    CancellationToken.None,
                    TaskCreationOptions.None,
                    TaskScheduler.Default
                ).ToApm(callback, state);
            }

            void IClientContract.EndReceiveTicks(IAsyncResult result)
            {
                TaskApm.EndInvoke(result);
            }
        }
    }

    public sealed class TicksReceivedEventArgs : EventArgs
    {
        public TicksReceivedEventArgs(string portfolioId, IReadOnlyList<TickData> data)
        {
            this.PortfolioId = portfolioId;
            this.Data = data;
        }

        public IReadOnlyList<TickData> Data
        {
            get;
        }

        public string PortfolioId
        {
            get;
        }
    }
}