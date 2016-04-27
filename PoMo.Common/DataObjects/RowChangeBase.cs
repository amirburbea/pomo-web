namespace PoMo.Common.DataObjects
{
    public abstract class RowChangeBase
    {
        protected RowChangeBase(RowChangeType changeType)
        {
            this.ChangeType = changeType;
        }

        public RowChangeType ChangeType
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