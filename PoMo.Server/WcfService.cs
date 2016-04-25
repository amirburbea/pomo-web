using System;
using System.Collections.Concurrent;
using System.Data;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using PoMo.Common.DataObjects;
using PoMo.Common.ServiceModel;
using PoMo.Common.ServiceModel.Contracts;
using PoMo.Common.System;

namespace PoMo.Server
{
    [ServiceBehavior(Namespace = Namespace.Value, InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, MaxItemsInObjectGraph = 0x400000)]
    public sealed class WcfService : IServerContract
    {
        private readonly ConcurrentDictionary<string, Client> _clients = new ConcurrentDictionary<string, Client>();

        IAsyncResult IServerContract.BeginRegisterClient(AsyncCallback callback, object state)
        {
            return Task.Factory.StartNew(
                arg => this.RegisterClient((OperationContext)arg),
                OperationContext.Current,
                CancellationToken.None,
                TaskCreationOptions.None,
                TaskScheduler.Default
            ).ToApm(callback, state);
        }

        void IServerContract.EndRegisterClient(IAsyncResult result)
        {
            TaskApm.EndInvoke(result);
        }

        private void RegisterClient(OperationContext context)
        {
            if (context.SessionId == null)
            {
                return;
            }
            ICallbackContract callback = context.GetCallbackChannel<ICallbackContract>();
            this._clients.TryAdd(context.SessionId, new Client(context.SessionId, callback));
            ICommunicationObject channel = (ICommunicationObject)callback;
            channel.Closed += this.ClientChannel_ClosedOrFaulted;
            channel.Faulted += this.ClientChannel_ClosedOrFaulted;
        }

        private void ClientChannel_ClosedOrFaulted(object sender, EventArgs e)
        {
        }

        IAsyncResult IServerContract.BeginGetData(string portfolioId, AsyncCallback callback, object state)
        {
            return Task.Factory.StartNew(
                arg => this.GetData(portfolioId, ((OperationContext)arg).SessionId),
                OperationContext.Current,
                CancellationToken.None,
                TaskCreationOptions.None,
                TaskScheduler.Default
            ).ToApm(callback, state);
        }

        IAsyncResult IServerContract.BeginGetPortfolios(AsyncCallback callback, object state)
        {
            return Task.Factory.StartNew(
                arg => this.GetPortfolios(((OperationContext)arg).SessionId),
                OperationContext.Current,
                CancellationToken.None,
                TaskCreationOptions.None,
                TaskScheduler.Default
            ).ToApm(callback, state);
        }

        IAsyncResult IServerContract.BeginUnsubscribe(string portfolioId, AsyncCallback callback, object state)
        {
            return Task.Factory.StartNew(
                arg => this.Unsubscribe(portfolioId, ((OperationContext)arg).SessionId),
                OperationContext.Current,
                CancellationToken.None,
                TaskCreationOptions.None,
                TaskScheduler.Default
            ).ToApm(callback, state);
        }

        DataSet IServerContract.EndGetData(IAsyncResult result)
        {
            return TaskApm.EndInvoke<DataSet>(result);
        }

        PortfolioModel[] IServerContract.EndGetPortfolios(IAsyncResult result)
        {
            return TaskApm.EndInvoke<PortfolioModel[]>(result);
        }

        void IServerContract.EndUnsubscribe(IAsyncResult result)
        {
            TaskApm.EndInvoke(result);
        }

        private DataSet GetData(string portfolioId, string sessionId)
        {
            return null;
        }

        private PortfolioModel[] GetPortfolios(string sessionId)
        {
            return null;
        }

        private void Unsubscribe(string porfolioId, string sessionId)
        {
        }

        private class Client
        {
            public string SessionId
            {
                get;
            }
            public ICallbackContract Callback
            {
                get;
            }

            public Client(string sessionId, ICallbackContract callback)
            {
                this.SessionId = sessionId;
                this.Callback = callback;
            }
        }
    }

    
}