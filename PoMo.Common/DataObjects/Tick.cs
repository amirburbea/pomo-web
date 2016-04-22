namespace PoMo.Common.DataObjects
{
    public sealed class Tick
    {
        public Tick()
        {
        }

        public Tick(string column, object value)
        {
            this.Column = column;
            this.Value = value;
        }

        public string Column
        {
            get;
            set;
        }

        public object Value
        {
            get;
            set;
        }
    }
}