using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Threading;
using PoMo.Common.DataObjects;

namespace PoMo.Client.Views.Shell
{
    public sealed class ShellViewModel : ViewModelBase
    {
        private readonly ObservableCollection<PortfolioModel> _portfolios;
        private ConnectionStatus _connectionStatus;
        private bool _isTabControlLocked;

        public ShellViewModel(Dispatcher dispatcher, IConnectionManager connectionManager)
            : base(dispatcher, connectionManager)
        {
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

        protected override void OnConnectionStatusChanged()
        {
            switch (this.ConnectionStatus = this.ConnectionManager.ConnectionStatus)
            {
                case ConnectionStatus.Connected:
                    if (this.Portfolios.Count == 0)
                    {
                        this.ConnectionManager.GetPortfoliosAsync(this.CreateBusyScope())
                            .ContinueWith(
                                task => this.Dispatcher.Invoke(
                                    DispatcherPriority.Normal,
                                    new Action<PortfolioModel[]>(models => Array.ForEach(models, this._portfolios.Add)),
                                    task.Result
                                ),
                                TaskContinuationOptions.NotOnFaulted
                            );
                    }
                    break;
            }
        }
    }
}