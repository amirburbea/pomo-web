using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using PoMo.Common.DataObjects;
using PoMo.Common.ServiceModel;
using PoMo.Common.ServiceModel.Contracts;
using PoMo.Data;

namespace PoMo.Server
{
    [ServiceBehavior(Namespace = Namespace.Value, InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, MaxItemsInObjectGraph = 0x400000)]
    public sealed class PortfolioService : IServerContract
    {
        private readonly ReaderWriterLockSlim _clientLock = new ReaderWriterLockSlim();
        private readonly Dictionary<string, Client> _clients = new Dictionary<string, Client>();
        private readonly IDataContext _dataContext;
        private readonly Timer _timer;

        public PortfolioService(IDataContext dataContext)
        {
            this._dataContext = dataContext;
            object o = new PortfolioData(dataContext);
            this._timer = new Timer(this.Timer_Elapsed, null, 60000, 60000);
        }

        void IHeartbeatContract.Heartbeat()
        {
            Console.WriteLine(string.Concat("Heartbeat received from ", OperationContext.Current.SessionId, "."));
        }

        PortfolioModel[] IServerContract.GetPortfolios()
        {
            return this._dataContext.Portfolios
                .AsEnumerable()
                .Select(portfolio => new PortfolioModel { Id = portfolio.Id, Name = portfolio.Name })
                .ToArray();
        }

        void IServerContract.RegisterClient()
        {
            if (OperationContext.Current.SessionId == null)
            {
                return;
            }
            Console.WriteLine(string.Concat("Registered client ", OperationContext.Current.SessionId, "."));
            try
            {
                this._clientLock.EnterWriteLock();
                this._clients.Add(OperationContext.Current.SessionId, new Client(OperationContext.Current.SessionId, OperationContext.Current.GetCallbackChannel<ICallbackContract>()));
            }
            finally
            {
                this._clientLock.ExitWriteLock();
            }
        }

        DataTable IServerContract.SubscribeToFirmSummary()
        {
            throw new NotImplementedException();
        }

        DataTable IServerContract.SubscribeToPortfolio(string portfolioId)
        {
            throw new NotImplementedException();
        }

        void IServerContract.UnsubscribeFromFirmSummary()
        {
            throw new NotImplementedException();
        }

        void IServerContract.UnsubscribeFromPortfolio(string porfolioId)
        {
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
                foreach (Client failedClient in failedClients)
                {
                    using (failedClient)
                    {
                        this._clients.Remove(failedClient.SessionId);
                    }
                }
            }
            finally
            {
                this._clientLock.ExitWriteLock();
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

        private sealed class Client : IDisposable
        {
            public Client(string sessionId, ICallbackContract callbackContract)
            {
                this.SessionId = sessionId;
                this.CallbackContract = callbackContract;
            }

            public ICallbackContract CallbackContract
            {
                get;
            }

            public string SessionId
            {
                get;
            }

            public void Dispose()
            {
                ServiceModelMethods.TryDispose(this.CallbackContract);
            }
        }
    }
}