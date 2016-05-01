using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using Microsoft.AspNet.SignalR;
using PoMo.Common.DataObjects;

namespace PoMo.Server.Web.SignalR
{
    public interface IDataHubController
    {
        void OnConnected(string connectionId);

        void OnDisconnected(string connectionId);

        DataTable SubscibeToFirmSummary(string connectionId);

        DataTable SubscribeToPortfolio(string connectionId, string portfolioId);

        void UnsubscribeFromFirmSummary(string connectionId);

        void UnsubscribeFromPortfolio(string connectionId, string portfolioId);
    }

    internal sealed class DataHubController : IDataHubController
    {
        private readonly ReaderWriterLockSlim _clientLock;
        private readonly Dictionary<string, Client> _clients;
        private readonly IFirmData _firmData;
        private readonly IHubContext<IDataClient> _hubContext;

        public DataHubController(IFirmData firmData)
        {
            this._firmData = firmData;
            this._hubContext = GlobalHost.ConnectionManager.GetHubContext<DataHub, IDataClient>();
            this._clients = new Dictionary<string, Client>();
            this._clientLock = new ReaderWriterLockSlim();
        }

        public void OnConnected(string connectionId)
        {
            try
            {
                this._clientLock.EnterWriteLock();
                this._clients[connectionId] = new Client(connectionId);
            }
            finally
            {
                this._clientLock.ExitWriteLock();
            }
        }

        public void OnDisconnected(string connectionId)
        {
            try
            {
                this._clientLock.EnterWriteLock();
                Client client;
                if (!this._clients.TryGetValue(connectionId, out client))
                {
                    return;
                }
                ISubscribable[] subscriptions = client.GetSubscriptions();
                foreach (ISubscribable subscribable in subscriptions)
                {
                    if (object.ReferenceEquals(this._firmData, subscribable))
                    {
                        this.UnsubscribeClientFromFirmSummary(client);
                    }
                    else
                    {
                        this.UnsubscribeClientFromPortfolio(client, (IPortfolioData)subscribable);
                    }
                }
                this._clients.Remove(connectionId);
            }
            finally
            {
                this._clientLock.ExitWriteLock();
            }
        }

        public DataTable SubscibeToFirmSummary(string connectionId)
        {
            Client client = this.GetClient(connectionId);
            if (client == null)
            {
                return null;
            }
            int count;
            if (this._firmData.Subscribe(connectionId, out count))
            {
                client.AddSubscription(this._firmData);
                if (count == 1)
                {
                    this._firmData.SummaryChanged += this.FirmData_SummaryChanged;
                }
            }
            return this._firmData.GetSummary();
        }

        public DataTable SubscribeToPortfolio(string connectionId, string portfolioId)
        {
            Client client = this.GetClient(connectionId);
            IPortfolioData portfolioData;
            if (client == null || !this._firmData.TryGetPortfolio(portfolioId, out portfolioData))
            {
                return null;
            }
            int count;
            if (portfolioData.Subscribe(connectionId, out count))
            {
                client.AddSubscription(portfolioData);
                if (count == 1)
                {
                    portfolioData.DataChanged += this.PortfolioData_DataChanged;
                }
            }
            return portfolioData.GetData();
        }

        public void UnsubscribeFromFirmSummary(string connectionId)
        {
            Client client = this.GetClient(connectionId);
            if (client != null)
            {
                this.UnsubscribeClientFromFirmSummary(client);
            }
        }

        public void UnsubscribeFromPortfolio(string connectionId, string portfolioId)
        {
            Client client = this.GetClient(connectionId);
            IPortfolioData portfolioData;
            if (client != null && this._firmData.TryGetPortfolio(portfolioId, out portfolioData))
            {
                this.UnsubscribeClientFromPortfolio(client, portfolioData);
            }
        }

        private void FirmData_SummaryChanged(object sender, DataEventArgs<RowChangeBase[]> e)
        {
            string[] subscriberIds = this._firmData.GetSubscribers();
            if (subscriberIds.Length != 0)
            {
                this._hubContext.Clients.Clients(subscriberIds).OnFirmSummaryChanged(e.Data);
            }
        }

        private Client GetClient(string connectionId)
        {
            try
            {
                this._clientLock.EnterReadLock();
                Client client;
                return this._clients.TryGetValue(connectionId, out client) ? client : null;
            }
            finally
            {
                this._clientLock.ExitReadLock();
            }
        }

        private void PortfolioData_DataChanged(object sender, DataEventArgs<RowChangeBase[]> e)
        {
            IPortfolioData portfolioData = (IPortfolioData)sender;
            string[] subscriberIds = portfolioData.GetSubscribers();
            if (subscriberIds.Length != 0)
            {
                this._hubContext.Clients.Clients(subscriberIds).OnPortfolioChanged(portfolioData.PortfolioId, e.Data);
            }
        }

        private void UnsubscribeClientFromFirmSummary(Client client)
        {
            int count;
            if (!this._firmData.Unsubscribe(client.ConnectionId, out count))
            {
                return;
            }
            client.RemoveSubscription(this._firmData);
            if (count == 0)
            {
                this._firmData.SummaryChanged -= this.FirmData_SummaryChanged;
            }
        }

        private void UnsubscribeClientFromPortfolio(Client client, IPortfolioData portfolioData)
        {
            int count;
            if (!portfolioData.Unsubscribe(client.ConnectionId, out count))
            {
                return;
            }
            client.RemoveSubscription(portfolioData);
            if (count == 0)
            {
                portfolioData.DataChanged -= this.PortfolioData_DataChanged;
            }
        }

        private sealed class Client
        {
            private readonly Dictionary<string, ISubscribable> _subscriptions = new Dictionary<string, ISubscribable>();

            public Client(string connectionId)
            {
                this.ConnectionId = connectionId;
            }

            public string ConnectionId
            {
                get;
            }

            public void AddSubscription(ISubscribable subscribable)
            {
                lock (this)
                {
                    this._subscriptions[(subscribable as IPortfolioData)?.PortfolioId ?? string.Empty] = subscribable;
                }
            }

            public ISubscribable[] GetSubscriptions()
            {
                lock (this)
                {
                    return this._subscriptions.Values.ToArray();
                }
            }

            public void RemoveSubscription(ISubscribable subscribable)
            {
                lock (this)
                {
                    this._subscriptions.Remove((subscribable as IPortfolioData)?.PortfolioId ?? string.Empty);
                }
            }
        }
    }
}