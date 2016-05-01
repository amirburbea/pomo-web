using System.Collections.ObjectModel;

namespace PoMo.Common.DataObjects
{
    public sealed class RowColumnsChanged : RowChangeBase
    {
        public override RowChangeType ChangeType => RowChangeType.ColumnsChanged;

        public Collection<ColumnChange> ColumnChanges
        {
            get;
        } = new Collection<ColumnChange>();
    }
}