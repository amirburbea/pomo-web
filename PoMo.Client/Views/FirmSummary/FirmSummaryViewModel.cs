using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Threading;
using PoMo.Common.DataObjects;

namespace PoMo.Client.Views.FirmSummary
{
    public sealed class FirmSummaryViewModel : SubscriberViewModelBase
    {
        public FirmSummaryViewModel(Dispatcher dispatcher, IConnectionManager connectionManager)
            : base(dispatcher, connectionManager)
        {
            this.ConnectionManager.FirmSummaryChanged += this.ConnectionManager_FirmSummaryChanged;
        }

        private void ConnectionManager_FirmSummaryChanged(object sender, ChangeEventArgs e)
        {
            if (this.IsActive)
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action<IReadOnlyCollection<RowChangeBase>>(this.ProcessChanges), e.RowChanges);
            }
        }

        protected override Task<DataTable> SubscribeAsync()
        {
            return this.ConnectionManager.SubscribeToFirmSummaryAsync(this.CreateBusyScope());
        }

        protected override Task UnsubscribeAsync()
        {
            return this.ConnectionManager.UnsubscribeFromFirmSummaryAsync(this.CreateBusyScope());
        }

        public override void Dispose()
        {
            this.ConnectionManager.FirmSummaryChanged -= this.ConnectionManager_FirmSummaryChanged;
            base.Dispose();
        }
    }
}