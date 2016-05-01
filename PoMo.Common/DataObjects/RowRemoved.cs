namespace PoMo.Common.DataObjects
{
    public sealed class RowRemoved : RowChangeBase
    {
        public override RowChangeType ChangeType => RowChangeType.Removed;
    }
}