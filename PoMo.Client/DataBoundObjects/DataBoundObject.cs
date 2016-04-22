using System;
using System.ComponentModel;

namespace PoMo.Client.DataBoundObjects
{
    [TypeDescriptionProvider(typeof(DataBoundTypeDescriptionProvider))]
    public sealed class DataBoundObject : INotifyPropertyChanged
    {
        private readonly DataBoundObjectCollection _collection;
        private readonly object[] _values;

        internal DataBoundObject(DataBoundObjectCollection collection, object[] values)
        {
            this._collection = collection;
            this._values = values;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public object this[int ordinal] => this._values[ordinal];

        public object this[string columnName] => this._collection.Properties.Find(columnName, false)?.GetValue(this);

        internal bool OnPropertyChanged(int ordinal, object value)
        {
            if (object.Equals(this._values[ordinal], value))
            {
                return false;
            }
            this._values[ordinal] = value;
            this.OnPropertyChanged(this._collection.Properties[ordinal].Name);
            return true;
        }

        private void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private sealed class DataBoundTypeDescriptionProvider : TypeDescriptionProvider
        {
            public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
            {
                return ((DataBoundObject)instance)._collection.CustomTypeDescriptor;
            }
        }
    }
}