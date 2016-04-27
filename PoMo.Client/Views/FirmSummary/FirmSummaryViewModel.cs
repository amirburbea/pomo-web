using System;
using PoMo.Common;

namespace PoMo.Client.Views.FirmSummary
{
    public sealed class FirmSummaryViewModel : NotifierBase, IDisposable
    {
        private readonly IConnectionManager _connectionManager;

        public FirmSummaryViewModel(IConnectionManager connectionManager)
        {
            this._connectionManager = connectionManager;
        }

        public void Dispose()
        {
        }
    }
}