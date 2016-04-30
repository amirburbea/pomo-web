using System;

namespace PoMo.Common.DataObjects
{
    [Serializable]
    public sealed class RowAdded : RowChangeBase
    {
        private object[] _data;

        public override RowChangeType ChangeType => RowChangeType.Added;

        public object[] Data
        {
            get
            {
                return this._data;
            }
            set
            {
                this._data = value;
            }
        }
    }
}