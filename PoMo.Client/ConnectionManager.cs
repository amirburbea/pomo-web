using System;
using System.Collections.Generic;
using System.Data;
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

        event EventHandler<ChangeEventArgs> FirmSummaryChanged;

        event EventHandler<PortfolioChangeEventArgs> PortfolioChanged;

        ConnectionStatus ConnectionStatus
        {
            get;
        }

        Task<PortfolioModel[]> GetPortfoliosAsync();

        Task<DataTable> SubscribeToFirmSummaryAsync();

        Task<DataTable> SubscribeToPortfolioAsync(string portfolioId);
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

        public event EventHandler<ChangeEventArgs> FirmSummaryChanged;

        public event EventHandler<PortfolioChangeEventArgs> PortfolioChanged;

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

        Task<PortfolioModel[]> IConnectionManager.GetPortfoliosAsync()
        {
            TaskCompletionSource<PortfolioModel[]> source = new TaskCompletionSource<PortfolioModel[]>();

            return source.Task;
        }

        Task<DataTable> IConnectionManager.SubscribeToFirmSummaryAsync()
        {
            return null;
        }

        Task<DataTable> IConnectionManager.SubscribeToPortfolioAsync(string portfolioId)
        {
            return null;
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

        private void OnFirmSummaryChanged(IReadOnlyCollection<RowChangeBase> changes)
        {
            this.FirmSummaryChanged?.Invoke(this, new ChangeEventArgs(changes));
        }

        private void OnPortfolioChanged(string portfolioId, IReadOnlyCollection<RowChangeBase> changes)
        {
            this.PortfolioChanged?.Invoke(this, new PortfolioChangeEventArgs(portfolioId, changes));
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

            void ICallbackContract.OnFirmSummaryChanged(RowChangeBase[] changes)
            {
                Task.Factory.StartNew(
                    arg => this._connectionManager.OnFirmSummaryChanged((RowChangeBase[])arg),
                    changes,
                    CancellationToken.None,
                    TaskCreationOptions.None,
                    TaskScheduler.Default
                );
            }

            void ICallbackContract.OnPortfolioChanged(string portfolioId, RowChangeBase[] changes)
            {
                Task.Factory.StartNew(
                    () => this._connectionManager.OnPortfolioChanged(portfolioId, changes),
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