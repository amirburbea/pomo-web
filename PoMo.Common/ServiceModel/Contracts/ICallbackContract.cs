using System.ServiceModel;
using PoMo.Common.DataObjects;

namespace PoMo.Common.ServiceModel.Contracts
{
    [ServiceContract(Namespace = Namespace.Value), ServiceKnownType(typeof(RowAdded)), ServiceKnownType(typeof(RowColumnsChanged)), ServiceKnownType(typeof(RowRemoved))]
    public interface ICallbackContract : IHeartbeatContract
    {
        [OperationContract(IsOneWay = false)]
        void OnPortfolioChanged(string portfolioId, RowChangeBase[] changes);

        [OperationContract(IsOneWay = false)]
        void OnFirmSummaryChanged(RowChangeBase[] changes);
    }
}