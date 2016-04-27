using PoMo.Common;

namespace PoMo.Client.Views
{
    public abstract class SubscriberViewModelBase : NotifierBase
    {
        private bool _isActive;

        public bool IsActive
        {
            get
            {
                return this._isActive;
            }
            internal set
            {
                if (this._isActive == value)
                {
                    return;
                }
                this._isActive = value;
                this.OnPropertyChanged();
                this.OnIsActiveChanged();
            }
        }

        protected abstract void OnIsActiveChanged();


    }
}