using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Threading;
using PoMo.Common.DataObjects;

namespace PoMo.Client.Views.Positions
{
    public sealed class PositionsViewModel : SubscriberViewModelBase
    {
        public PositionsViewModel(PortfolioModel parameter, Dispatcher dispatcher, IConnectionManager connectionManager)
            : base(dispatcher, connectionManager)
        {
            this.Portfolio = parameter;
            this.ConnectionManager.PortfolioChanged += this.ConnectionManager_PortfolioChanged;
        }

        public PortfolioModel Portfolio
        {
            get;
        }

        public override void Dispose()
        {
            this.ConnectionManager.PortfolioChanged -= this.ConnectionManager_PortfolioChanged;
            base.Dispose();
        }

        protected override Task<DataTable> SubscribeAsync()
        {
            return this.ConnectionManager.SubscribeToPortfolioAsync(this.Portfolio.Id, this.CreateBusyScope());
        }

        protected override Task UnsubscribeAsync()
        {
            return this.ConnectionManager.UnsubscribeFromPortfolioAsync(this.Portfolio.Id, this.CreateBusyScope());
        }

        private void ConnectionManager_PortfolioChanged(object sender, PortfolioChangeEventArgs e)
        {
            if (this.IsActive && e.PortfolioId == this.Portfolio.Id)
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action<IReadOnlyCollection<RowChangeBase>>(this.ProcessChanges), e.RowChanges);
            }
        }
    }
}