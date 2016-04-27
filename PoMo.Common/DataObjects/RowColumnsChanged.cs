using System.Collections.Generic;

namespace PoMo.Common.DataObjects
{
    public sealed class RowColumnsChanged : RowChangeBase
    {
        public RowColumnsChanged()
            : base(RowChangeType.ColumnsChanged)
        {
            this.ColumnChanges = new List<ColumnChange>();
        }

        public IList<ColumnChange> ColumnChanges
        {
            get;
        }
    }
}