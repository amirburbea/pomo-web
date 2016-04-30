using System;

namespace PoMo.Server
{
    public static class DataEventArgs
    {
        public static DataEventArgs<TData> Create<TData>(TData data)
        {
            return new DataEventArgs<TData>(data);
        }
    }

    public sealed class DataEventArgs<TData> : EventArgs
    {
        public DataEventArgs(TData data)
        {
            this.Data = data;
        }

        public TData Data
        {
            get;
        }
    }
}