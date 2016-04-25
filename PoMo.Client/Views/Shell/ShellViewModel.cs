using System.Collections.ObjectModel;
using PoMo.Common.DataObjects;

namespace PoMo.Client.Views.Shell
{
    public sealed class ShellViewModel : NotifierBase
    {
        private readonly ObservableCollection<PortfolioModel> _portfolios;
        private ConnectionStatus _connectionStatus;
        private bool _isLocked;

        public ShellViewModel()
        {
            this.Portfolios = new ReadOnlyObservableCollection<PortfolioModel>(this._portfolios = new ObservableCollection<PortfolioModel>());
            this._portfolios.Add(new PortfolioModel { Id = "ABC", Description = "Port ABC" });
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

        public bool IsLocked
        {
            get
            {
                return this._isLocked;
            }
            set
            {
                this.SetValue(ref this._isLocked, value);
            }
        }

        public ReadOnlyObservableCollection<PortfolioModel> Portfolios
        {
            get;
        }
    }
}