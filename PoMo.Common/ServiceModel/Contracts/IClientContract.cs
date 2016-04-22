using System;
using System.ServiceModel;
using PoMo.Common.DataObjects;

namespace PoMo.Common.ServiceModel.Contracts
{
    public interface IClientContract
    {
        [OperationContract(AsyncPattern = true, IsOneWay = false)]
        IAsyncResult BeginReceiveTicks(string portfolioId, TickData[] data, AsyncCallback callback, object state);

        void EndReceiveTicks(IAsyncResult result);
    }
}