using System;
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
    public sealed class WcfService : IServiceContract
    {
        IAsyncResult IServiceContract.BeginGetData(string portfolioId, AsyncCallback callback, object state)
        {
            return Task.Factory.StartNew(
                arg =>
                {
                    OperationContext context = (OperationContext)arg;
                    return this.GetData(portfolioId, context.SessionId, context.GetCallbackChannel<IClientContract>());
                },
                OperationContext.Current,
                CancellationToken.None,
                TaskCreationOptions.None,
                TaskScheduler.Default
            ).ToApm(callback, state);
        }

        IAsyncResult IServiceContract.BeginGetPortfolios(AsyncCallback callback, object state)
        {
            return Task.Factory.StartNew(
                arg => this.GetPortfolios(((OperationContext)arg).SessionId),
                OperationContext.Current,
                CancellationToken.None,
                TaskCreationOptions.None,
                TaskScheduler.Default
            ).ToApm(callback, state);
        }

        IAsyncResult IServiceContract.BeginUnsubscribe(string portfolioId, AsyncCallback callback, object state)
        {
            return Task.Factory.StartNew(
                arg => this.Unsubscribe(portfolioId, ((OperationContext)arg).SessionId),
                OperationContext.Current,
                CancellationToken.None,
                TaskCreationOptions.None,
                TaskScheduler.Default
            ).ToApm(callback, state);
        }

        DataSet IServiceContract.EndGetData(IAsyncResult result)
        {
            return TaskApm.EndInvoke<DataSet>(result);
        }

        PortfolioModel[] IServiceContract.EndGetPortfolios(IAsyncResult result)
        {
            return TaskApm.EndInvoke<PortfolioModel[]>(result);
        }

        void IServiceContract.EndUnsubscribe(IAsyncResult result)
        {
            TaskApm.EndInvoke(result);
        }

        private DataSet GetData(string portfolioId, string sessionId, IClientContract callback)
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
    }
}