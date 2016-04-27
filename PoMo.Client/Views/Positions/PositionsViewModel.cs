using System;
using PoMo.Client.Views.Shell;
using PoMo.Common;
using PoMo.Common.DataObjects;

namespace PoMo.Client.Views.Positions
{
    public sealed class PositionsViewModel : NotifierBase, IDisposable
    {
        private readonly IConnectionManager _connectionManager;

        public PositionsViewModel(IConnectionManager connectionManager, PortfolioModel parameter)
        {
            this._connectionManager = connectionManager;
            this.Portfolio = parameter;
        }

        public PortfolioModel Portfolio
        {
            get;
        }

        public void Dispose()
        {
        }
    }
}