namespace PoMo.Common.DataObjects
{
    public sealed class RowRemoved : RowChangeBase
    {
        public RowRemoved()
            : base(RowChangeType.Removed)
        {
        }
    }
}