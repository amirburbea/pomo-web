using PoMo.Client.Controls;

namespace PoMo.Client.Shell
{
    public sealed class ShellViewModel : NotifierBase
    {
        private bool _isLocked;

        private string _status = "Connecting";

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

        public string Status
        {
            get
            {
                return this._status;
            }
            private set
            {
                this.SetValue(ref this._status, value);
            }
        }

        public ITabTearOffHandler TabTearOffHandler
        {
            get;
        }
    }
}