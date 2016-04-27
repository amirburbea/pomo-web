using System;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Threading;
using System.Threading.Tasks;
using PoMo.Client.Properties;
using PoMo.Common.DataObjects;
using PoMo.Common.ServiceModel;
using PoMo.Common.ServiceModel.Contracts;

namespace PoMo.Client
{
    public interface IConnectionManager
    {
        event EventHandler ConnectionStatusChanged;

        event EventHandler<TicksReceivedEventArgs> TicksReceived;

        ConnectionStatus ConnectionStatus
        {
            get;
        }

        Task<PortfolioModel[]> GetPortfoliosAsync();
    }

    internal sealed class ConnectionManager : IConnectionManager, IDisposable
    {
        private readonly Thread _channelThread;
        private readonly DuplexChannelFactory<IServerContract> _factory;
        private readonly AutoResetEvent _resetEvent;
        private IServerContract _channel;
        private bool _isDisposed;

        public ConnectionManager(Binding binding)
        {
            this._factory = new DuplexChannelFactory<IServerContract>(
                new InstanceContext(new Callback(this)),
                new ServiceEndpoint(
                    ContractDescription.GetContract(typeof(IServerContract)),
                    binding,
                    new EndpointAddress(Settings.Default.WcfUri)
                )
            );
            this._resetEvent = new AutoResetEvent(true);
            this._channelThread = new Thread(this.Run)
            {
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal,
                Name = nameof(ConnectionManager)
            };
            this._channelThread.Start();
        }

        public event EventHandler ConnectionStatusChanged;

        public event EventHandler<TicksReceivedEventArgs> TicksReceived;

        public ConnectionStatus ConnectionStatus
        {
            get
            {
                if (this._channel != null)
                {
                    ICommunicationObject communicationObject = (ICommunicationObject)this._channel;
                    switch (communicationObject.State)
                    {
                        case CommunicationState.Created:
                        case CommunicationState.Opening:
                            return ConnectionStatus.Connecting;
                        case CommunicationState.Opened:
                            return ConnectionStatus.Connected;
                    }
                }
                return ConnectionStatus.Disconnected;
            }
        }

        public void Dispose()
        {
            if (this._isDisposed)
            {
                return;
            }
            this._isDisposed = true;
            this._resetEvent.Set();
            this._channelThread.Join();
            ServiceModelMethods.TryDispose(Interlocked.Exchange(ref this._channel, null));
            ServiceModelMethods.TryDispose(this._factory);
        }

        public Task<PortfolioModel[]> GetPortfoliosAsync()
        {
            TaskCompletionSource<PortfolioModel[]> source = new TaskCompletionSource<PortfolioModel[]>();

            return source.Task;
        }

        private void Channel_ClosedOrFaulted(object sender, EventArgs e)
        {
            ICommunicationObject communicationObject = (ICommunicationObject)sender;
            communicationObject.Closed -= this.Channel_ClosedOrFaulted;
            communicationObject.Faulted -= this.Channel_ClosedOrFaulted;
            this._channel = null;
            this.OnConnectionStatusChanged();
            this._resetEvent.Set();
        }

        private IServerContract InitializeChannel()
        {
            IServerContract channel = this._factory.CreateChannel();
            ICommunicationObject communicationObject = (ICommunicationObject)channel;
            try
            {
                communicationObject.Open();
            }
            catch
            {
                return null;
            }
            communicationObject.Closed += this.Channel_ClosedOrFaulted;
            communicationObject.Faulted += this.Channel_ClosedOrFaulted;
            return channel;
        }

        private void OnConnectionStatusChanged()
        {
            this.ConnectionStatusChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnTicksReceived(string portfolioId, TickData[] data)
        {
            this.TicksReceived?.Invoke(this, new TicksReceivedEventArgs(portfolioId, Array.AsReadOnly(data)));
        }

        private void Run()
        {
            while (true)
            {
                bool waitOne = this._resetEvent.WaitOne(TimeSpan.FromSeconds(30d));
                try
                {
                    if (this._isDisposed)
                    {
                        return;
                    }
                    if (this._channel == null)
                    {
                        if ((this._channel = this.InitializeChannel()) == null)
                        {
                            this.OnConnectionStatusChanged();
                            continue;
                        }
                        this._channel.RegisterClient();
                        this.OnConnectionStatusChanged();
                    }
                    if (!waitOne)
                    {
                        this._channel.Heartbeat();
                    }
                    else
                    {
                    }
                }
                catch (CommunicationException e)
                {
                    Debug.WriteLine(e);
                }
            }
        }

        private sealed class Callback : ICallbackContract
        {
            private readonly ConnectionManager _connectionManager;

            public Callback(ConnectionManager connectionManager)
            {
                this._connectionManager = connectionManager;
            }

            void ICallbackContract.ReceiveTicks(string portfolioId, TickData[] data)
            {
                Task.Factory.StartNew(
                    () => this._connectionManager.OnTicksReceived(portfolioId, data),
                    CancellationToken.None,
                    TaskCreationOptions.None,
                    TaskScheduler.Default
                );
            }

            void IHeartbeatContract.Heartbeat()
            {
                Debug.WriteLine("Heartbeat received from server.");
            }
        }
    }
}