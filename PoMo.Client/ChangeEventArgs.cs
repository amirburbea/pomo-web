using System;
using System.Collections.Generic;
using PoMo.Common.DataObjects;

namespace PoMo.Client
{
    public class ChangeEventArgs : EventArgs
    {
        public ChangeEventArgs(IReadOnlyCollection<RowChangeBase> rowChanges)
        {
            this.RowChanges = rowChanges;
        }

        public IReadOnlyCollection<RowChangeBase> RowChanges
        {
            get;
        }
    }
}