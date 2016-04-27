using System.Data;
using System.ServiceModel;
using PoMo.Common.DataObjects;

namespace PoMo.Common.ServiceModel.Contracts
{
    [ServiceContract(CallbackContract = typeof(ICallbackContract), Namespace = Namespace.Value, SessionMode = SessionMode.Allowed)]
    public interface IServerContract : IHeartbeatContract
    {
        [OperationContract]
        PortfolioModel[] GetPortfolios();

        [OperationContract(IsOneWay = false, IsInitiating = true)]
        void RegisterClient();

        [OperationContract]
        DataTable SubscribeToPortfolio(string portfolioId);

        [OperationContract]
        DataTable SubscribeToFirmSummary();

        [OperationContract(IsOneWay = false)]
        void UnsubscribeFromPortfolio(string portfolioId);

        [OperationContract(IsOneWay = false)]
        void UnsubscribeFromFirmSummary();
    }
}