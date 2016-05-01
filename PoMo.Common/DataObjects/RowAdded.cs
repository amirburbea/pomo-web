namespace PoMo.Common.DataObjects
{
    public sealed class RowAdded : RowChangeBase
    {
        public override RowChangeType ChangeType => RowChangeType.Added;

        public object[] Data
        {
            get;
            set;
        }
    }
}