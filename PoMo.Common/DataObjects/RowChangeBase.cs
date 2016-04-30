using System;

namespace PoMo.Common.DataObjects
{
    [Serializable]
    public abstract class RowChangeBase
    {
        private object _rowKey;

        public abstract RowChangeType ChangeType
        {
            get;
        }

        public object RowKey
        {
            get
            {
                return this._rowKey;
            }
            set
            {
                this._rowKey = value;
            }
        }
    }
}