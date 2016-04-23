using PoMo.Client.Controls;

namespace PoMo.Client.Shell
{
    public sealed class ShellViewModel : NotifierBase
    {
        private bool _isLocked;
        private ConnectionStatus _connectionStatus;

        public ShellViewModel(ITabTearOffHandler tabTearOffHandler)
        {
            this.TabTearOffHandler = tabTearOffHandler;
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

        public ITabTearOffHandler TabTearOffHandler
        {
            get;
        }
    }
}