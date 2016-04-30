using System.Data;
using System.ServiceModel;
using PoMo.Common.DataObjects;

namespace PoMo.Common.ServiceModel.Contracts
{
    [ServiceContract(CallbackContract = typeof(ICallbackContract), Namespace = ServicesNamespace.Value, SessionMode = SessionMode.Allowed)]
    public interface IServerContract
    {
        [OperationContract]
        PortfolioModel[] GetPortfolios();

        [OperationContract(IsOneWay = false)]
        void Heartbeat();

        [OperationContract(IsOneWay = false, IsInitiating = true)]
        void RegisterClient();

        [OperationContract]
        DataTable SubscribeToFirmSummary();

        [OperationContract]
        DataTable SubscribeToPortfolio(string portfolioId);

        [OperationContract(IsOneWay = false)]
        void UnsubscribeFromFirmSummary();

        [OperationContract(IsOneWay = false)]
        void UnsubscribeFromPortfolio(string portfolioId);
    }
}