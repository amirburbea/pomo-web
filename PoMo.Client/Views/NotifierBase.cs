using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PoMo.Client.Views
{
    public abstract class NotifierBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void SetValue<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return;
            }
            field = value;
            this.OnPropertyChanged(propertyName);
        }
    }
}