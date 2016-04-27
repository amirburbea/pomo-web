namespace PoMo.Common.DataObjects
{
    public sealed class RowAdded : RowChangeBase
    {
        public RowAdded()
            : base(RowChangeType.Added)
        {
        }

        public object[] Data
        {
            get;
            set;
        }
    }
}