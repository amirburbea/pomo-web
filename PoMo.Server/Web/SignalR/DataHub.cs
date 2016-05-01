using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace PoMo.Server.Web.SignalR
{
    [HubName("data")]
    public sealed class DataHub : Hub<IDataClient>
    {
        private readonly IDataHubController _hubController;

        public DataHub(IDataHubController hubController)
        {
            this._hubController = hubController;
        }

        public override Task OnConnected()
        {
            this._hubController.OnConnected(this.Context.ConnectionId);
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            this._hubController.OnDisconnected(this.Context.ConnectionId);
            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            this._hubController.OnConnected(this.Context.ConnectionId);
            return base.OnReconnected();
        }

        public Task<DataTable> SubscribeToFirmSummary()
        {
            return Task.Factory.StartNew(
                arg => this._hubController.SubscibeToFirmSummary((string)arg),
                this.Context.ConnectionId,
                CancellationToken.None,
                TaskCreationOptions.None,
                TaskScheduler.Default
            );
        }

        public Task<DataTable> SubscribeToPortfolio(string portfolioId)
        {
            return Task.Factory.StartNew(
                arg => this._hubController.SubscribeToPortfolio((string)arg, portfolioId),
                this.Context.ConnectionId,
                CancellationToken.None,
                TaskCreationOptions.None,
                TaskScheduler.Default
            );
        }

        public Task UnsubscribeFromFirmSummary()
        {
            return Task.Factory.StartNew(
                arg => this._hubController.UnsubscribeFromFirmSummary((string)arg),
                this.Context.ConnectionId,
                CancellationToken.None,
                TaskCreationOptions.None,
                TaskScheduler.Default
            );
        }

        public Task UnsubscribeFromPortfolio(string portfolioId)
        {
            return Task.Factory.StartNew(
                arg => this._hubController.UnsubscribeFromPortfolio((string)arg, portfolioId),
                this.Context.ConnectionId,
                CancellationToken.None,
                TaskCreationOptions.None,
                TaskScheduler.Default
            );
        }
    }
}