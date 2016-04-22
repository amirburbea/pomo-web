using System;
using System.Data;
using System.ServiceModel;
using PoMo.Common.DataObjects;

namespace PoMo.Common.ServiceModel.Contracts
{
    [ServiceContract(CallbackContract = typeof(IClientContract), Namespace = Namespace.Value, SessionMode = SessionMode.Allowed)]
    public interface IServiceContract
    {
        [OperationContract(AsyncPattern = true, IsOneWay = false)]
        IAsyncResult BeginGetData(string portfolioId, AsyncCallback callback, object state);

        DataSet EndGetData(IAsyncResult result);

        [OperationContract(AsyncPattern = true, IsOneWay = false)]
        IAsyncResult BeginUnsubscribe(string portfolioId, AsyncCallback callback, object state);

        void EndUnsubscribe(IAsyncResult result);

        [OperationContract(AsyncPattern = true, IsOneWay = false)]
        IAsyncResult BeginGetPortfolios(AsyncCallback callback, object state);

        PortfolioModel[] EndGetPortfolios(IAsyncResult result);
    }
}