namespace PoMo.Common.DataObjects
{
    public abstract class RowChangeBase
    {
        public abstract RowChangeType ChangeType
        {
            get;
        }

        public object RowKey
        {
            get;
            set;
        }
    }
}