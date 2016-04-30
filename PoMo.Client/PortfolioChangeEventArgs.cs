using System.Collections.Generic;
using PoMo.Common.DataObjects;

namespace PoMo.Client
{
    public sealed class PortfolioChangeEventArgs : ChangeEventArgs
    {
        public PortfolioChangeEventArgs(string portfolioId, IReadOnlyCollection<RowChangeBase> rowChanges)
            : base(rowChanges)
        {
            this.PortfolioId = portfolioId;
        }

        public string PortfolioId
        {
            get;
        }
    }
}