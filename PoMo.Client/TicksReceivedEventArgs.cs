using System;
using System.Collections.ObjectModel;
using PoMo.Common.DataObjects;

namespace PoMo.Client
{
    public sealed class TicksReceivedEventArgs : EventArgs
    {
        public TicksReceivedEventArgs(string portfolioId, ReadOnlyCollection<TickData> data)
        {
            this.Data = data;
            this.PortfolioId = portfolioId;
        }

        public ReadOnlyCollection<TickData> Data
        {
            get;
        }

        public string PortfolioId
        {
            get;
        }
    }
}