using PoMo.Common.DataObjects;

namespace PoMo.Server.Web.SignalR
{
    public interface IDataClient
    {
        void OnFirmSummaryChanged(RowChangeBase[] changes);

        void OnPortfolioChanged(string portfolioId, RowChangeBase[] changes);
    }
}