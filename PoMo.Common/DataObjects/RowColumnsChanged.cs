using System;
using System.Collections.ObjectModel;

namespace PoMo.Common.DataObjects
{
    [Serializable]
    public sealed class RowColumnsChanged : RowChangeBase
    {
        public override RowChangeType ChangeType => RowChangeType.ColumnsChanged;

        public Collection<ColumnChange> ColumnChanges
        {
            get;
        } = new Collection<ColumnChange>();
    }
}