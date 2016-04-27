using System.ServiceModel;

namespace PoMo.Common.ServiceModel.Contracts
{
    [ServiceContract(Namespace = Namespace.Value)]
    public interface IHeartbeatContract
    {
        [OperationContract(IsOneWay = false)]
        void Heartbeat();
    }
}