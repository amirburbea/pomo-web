using System;

namespace PoMo.Common.DataObjects
{
    [Serializable]
    public sealed class RowRemoved : RowChangeBase
    {
        public override RowChangeType ChangeType => RowChangeType.Removed;
    }
}