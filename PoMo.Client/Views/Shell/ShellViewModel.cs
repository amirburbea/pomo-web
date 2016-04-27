using System;
using System.Collections.ObjectModel;
using PoMo.Common;
using PoMo.Common.DataObjects;

namespace PoMo.Client.Views.Shell
{
    public sealed class ShellViewModel : NotifierBase
    {
        private readonly IConnectionManager _connectionManager;
        private readonly ObservableCollection<PortfolioModel> _portfolios;
        private ConnectionStatus _connectionStatus;
        private bool _isTabControlLocked;

        public ShellViewModel(IConnectionManager connectionManager)
        {
            this._connectionManager = connectionManager;
            this._connectionManager.ConnectionStatusChanged += this.ConnectionManager_ConnectionStatusChanged;
            this.Portfolios = new ReadOnlyObservableCollection<PortfolioModel>(this._portfolios = new ObservableCollection<PortfolioModel>());
        }

        public ConnectionStatus ConnectionStatus
        {
            get
            {
                return this._connectionStatus;
            }
            private set
            {
                this.SetValue(ref this._connectionStatus, value);
            }
        }

        public bool IsTabControlLocked
        {
            get
            {
                return this._isTabControlLocked;
            }
            set
            {
                this.SetValue(ref this._isTabControlLocked, value);
            }
        }

        public ReadOnlyObservableCollection<PortfolioModel> Portfolios
        {
            get;
        }

        private void ConnectionManager_ConnectionStatusChanged(object sender, EventArgs e)
        {
            this.ConnectionStatus = ((IConnectionManager)sender).ConnectionStatus;
        }
    }
}