using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using PoMo.Common.DataObjects;
using PoMo.Common.ServiceModel;
using PoMo.Common.ServiceModel.Contracts;

namespace PoMo.Server
{
    [ServiceBehavior(Namespace = ServicesNamespace.Value, InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, MaxItemsInObjectGraph = 0x400000)]
    public sealed class DataService : IServerContract
    {
        private readonly ReaderWriterLockSlim _clientLock = new ReaderWriterLockSlim();
        private readonly Dictionary<string, Client> _clients = new Dictionary<string, Client>();
        private readonly IFirmData _firmData;
        private readonly Timer _timer;

        public DataService(IFirmData firmData)
        {
            this._firmData = firmData;
            this._timer = new Timer(this.Timer_Elapsed, null, 60000, 60000);
        }

        PortfolioModel[] IServerContract.GetPortfolios()
        {
            return this._firmData.Portfolios.Select(datum => new PortfolioModel { Id = datum.PortfolioId, Name = datum.PortfolioName }).ToArray();
        }

        void IServerContract.Heartbeat()
        {
            Debug.WriteLine(string.Concat("Heartbeat received from ", OperationContext.Current.SessionId, "."));
        }

        void IServerContract.RegisterClient()
        {
            if (OperationContext.Current.SessionId == null)
            {
                return;
            }
            string sessionId = OperationContext.Current.SessionId;
            try
            {
                this._clientLock.EnterWriteLock();
                ICallbackContract callbackContract = OperationContext.Current.GetCallbackChannel<ICallbackContract>();
                IClientChannel channel = (IClientChannel)callbackContract;
                channel.Closed += this.Channel_ClosedOrFaulted;
                channel.Faulted += this.Channel_ClosedOrFaulted;
                this._clients.Add(sessionId, new Client(this, sessionId, callbackContract));
            }
            finally
            {
                this._clientLock.ExitWriteLock();
            }
            Debug.WriteLine(string.Concat("Registered client ", sessionId, "."));
        }

        DataTable IServerContract.SubscribeToFirmSummary()
        {
            Client client = this.GetClient();
            if (client == null)
            {
                return null;
            }
            int count;
            if (this._firmData.Subscribe(client.SessionId, out count))
            {
                client.AddSubscription(this._firmData);
                if (count == 1)
                {
                    this._firmData.SummaryChanged += this.FirmData_SummaryChanged;
                }
            }
            return this._firmData.GetSummary();
        }

        DataTable IServerContract.SubscribeToPortfolio(string portfolioId)
        {
            Client client = this.GetClient();
            IPortfolioData portfolioData;
            if (client == null || !this._firmData.TryGetPortfolio(portfolioId, out portfolioData))
            {
                return null;
            }
            int count;
            if (portfolioData.Subscribe(client.SessionId, out count))
            {
                client.AddSubscription(portfolioData);
                if (count == 1)
                {
                    portfolioData.DataChanged += this.PortfolioData_DataChanged;
                }
            }
            return portfolioData.GetData();
        }

        void IServerContract.UnsubscribeFromFirmSummary()
        {
            Client client = this.GetClient();
            if (client == null)
            {
                return;
            }
            int count;
            if (!this._firmData.Unsubscribe(client.SessionId, out count))
            {
                return;
            }
            client.RemoveSubscription(this._firmData);
            if (count == 0)
            {
                this._firmData.SummaryChanged -= this.FirmData_SummaryChanged;
            }
        }

        void IServerContract.UnsubscribeFromPortfolio(string portfolioId)
        {
            Client client = this.GetClient();
            IPortfolioData portfolioData;
            int count;
            if (client == null || !this._firmData.TryGetPortfolio(portfolioId, out portfolioData) || !portfolioData.Unsubscribe(client.SessionId, out count))
            {
                return;
            }
            client.RemoveSubscription(portfolioData);
            if (count == 0)
            {
                portfolioData.DataChanged -= this.PortfolioData_DataChanged;
            }
        }

        private void Channel_ClosedOrFaulted(object sender, EventArgs e)
        {
            IClientChannel channel = (IClientChannel)sender;
            try
            {
                this._clientLock.EnterUpgradeableReadLock();
                Client client;
                if (!this._clients.TryGetValue(channel.SessionId, out client))
                {
                    channel.Closed -= this.Channel_ClosedOrFaulted;
                    channel.Faulted -= this.Channel_ClosedOrFaulted;
                    return;
                }
                try
                {
                    this._clientLock.EnterWriteLock();
                    this.RemoveClient(client);
                }
                finally
                {
                    this._clientLock.ExitWriteLock();
                }
            }
            finally
            {
                this._clientLock.ExitUpgradeableReadLock();
            }
        }

        private void FirmData_SummaryChanged(object sender, DataEventArgs<RowChangeBase[]> e)
        {
            this.PerformClientAction(this.GetClients(this._firmData.GetSubscribers()), channel => channel.OnFirmSummaryChanged(e.Data));
        }

        private Client GetClient()
        {
            if (OperationContext.Current.SessionId == null)
            {
                return null;
            }
            try
            {
                this._clientLock.EnterReadLock();
                Client client;
                return this._clients.TryGetValue(OperationContext.Current.SessionId, out client) ? client : null;
            }
            finally
            {
                this._clientLock.ExitReadLock();
            }
        }

        private IReadOnlyCollection<Client> GetClients(IEnumerable<string> subscriberIds)
        {
            try
            {
                this._clientLock.EnterReadLock();
                List<Client> clients = new List<Client>();
                foreach (string subscriberId in subscriberIds)
                {
                    Client client;
                    if (this._clients.TryGetValue(subscriberId, out client))
                    {
                        clients.Add(client);
                    }
                }
                return clients;
            }
            finally
            {
                this._clientLock.ExitReadLock();
            }
        }

        private void PerformClientAction(Action<ICallbackContract> action)
        {
            Client[] clients;
            try
            {
                this._clientLock.EnterReadLock();
                clients = this._clients.Values.ToArray();
            }
            finally
            {
                this._clientLock.ExitReadLock();
            }
            this.PerformClientAction(clients, action);
        }

        private void PerformClientAction(IEnumerable<Client> clients, Action<ICallbackContract> action)
        {
            List<Client> failedClients = new List<Client>();
            Parallel.ForEach(
                clients,
                () => new List<Client>(),
                (client, loopState, index, list) =>
                {
                    try
                    {
                        action(client.CallbackContract);
                    }
                    catch (Exception)
                    {
                        list.Add(client);
                    }
                    return list;
                },
                list =>
                {
                    if (list.Count == 0)
                    {
                        return;
                    }
                    lock (failedClients)
                    {
                        failedClients.AddRange(list);
                    }
                }
            );
            if (failedClients.Count == 0)
            {
                return;
            }
            try
            {
                this._clientLock.EnterWriteLock();
                failedClients.ForEach(this.RemoveClient);
            }
            finally
            {
                this._clientLock.ExitWriteLock();
            }
        }

        private void PortfolioData_DataChanged(object sender, DataEventArgs<RowChangeBase[]> e)
        {
            IPortfolioData portfolioData = (IPortfolioData)sender;
            this.PerformClientAction(this.GetClients(portfolioData.GetSubscribers()), channel => channel.OnPortfolioChanged(portfolioData.PortfolioId, e.Data));
        }

        private void RemoveClient(Client client)
        {
            // This method can only be invoked while in the write lock.
            using (client)
            {
                IClientChannel channel = (IClientChannel)client.CallbackContract;
                channel.Closed -= this.Channel_ClosedOrFaulted;
                channel.Faulted -= this.Channel_ClosedOrFaulted;
                this._clients.Remove(client.SessionId);
            }
        }

        private void Timer_Elapsed(object argument)
        {
            try
            {
                this._timer.Change(Timeout.Infinite, Timeout.Infinite);
                this.PerformClientAction(callback => callback.Heartbeat());
            }
            finally
            {
                this._timer.Change(60000, 60000);
            }
        }

        private void UnsubscribeClient(string sessionId, ISubscribable subscribable)
        {
            int count;
            if (!subscribable.Unsubscribe(sessionId, out count) || count != 0)
            {
                return;
            }
            if (object.ReferenceEquals(subscribable, this._firmData))
            {
                this._firmData.SummaryChanged -= this.FirmData_SummaryChanged;
            }
            else
            {
                ((IPortfolioData)subscribable).DataChanged -= this.PortfolioData_DataChanged;
            }
        }

        private sealed class Client : IDisposable
        {
            private readonly DataService _dataService;
            private readonly HashSet<ISubscribable> _subscriptions;

            public Client(DataService dataService, string sessionId, ICallbackContract callbackContract)
            {
                this._dataService = dataService;
                this.SessionId = sessionId;
                this.CallbackContract = callbackContract;
                this._subscriptions = new HashSet<ISubscribable>();
            }

            public ICallbackContract CallbackContract
            {
                get;
            }

            public string SessionId
            {
                get;
            }

            public void AddSubscription(ISubscribable subscribable)
            {
                lock (this._subscriptions)
                {
                    this._subscriptions.Add(subscribable);
                }
            }

            public void Dispose()
            {
                lock (this._subscriptions)
                {
                    if (this._subscriptions.Count != 0)
                    {
                        ISubscribable[] subscriptions = this._subscriptions.ToArray();
                        this._subscriptions.Clear();
                        Array.ForEach(subscriptions, subscription => this._dataService.UnsubscribeClient(this.SessionId, subscription));
                    }
                }
                ServiceModelMethods.TryClose(this.CallbackContract);
            }

            public void RemoveSubscription(ISubscribable subscribable)
            {
                lock (this._subscriptions)
                {
                    this._subscriptions.Remove(subscribable);
                }
            }
        }
    }
}