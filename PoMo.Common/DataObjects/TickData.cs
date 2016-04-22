using System.Collections.ObjectModel;

namespace PoMo.Common.DataObjects
{
    public sealed class TickData
    {
        public string RowId
        {
            get;
            set;
        }

        public Collection<Tick> Ticks
        {
            get;
        } = new Collection<Tick>();
    }
}