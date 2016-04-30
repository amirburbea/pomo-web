using System.ServiceModel;
using PoMo.Common.DataObjects;

namespace PoMo.Common.ServiceModel.Contracts
{
    [ServiceContract(Namespace = ServicesNamespace.Value), ServiceKnownType(typeof(RowAdded)), ServiceKnownType(typeof(RowColumnsChanged)), ServiceKnownType(typeof(RowRemoved))]
    public interface ICallbackContract
    {
        [OperationContract(IsOneWay = false)]
        void Heartbeat();

        [OperationContract(IsOneWay = false)]
        void OnFirmSummaryChanged(RowChangeBase[] changes);

        [OperationContract(IsOneWay = false)]
        void OnPortfolioChanged(string portfolioId, RowChangeBase[] changes);
    }
}