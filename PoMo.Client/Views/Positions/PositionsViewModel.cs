using System;
using PoMo.Client.Views.Shell;
using PoMo.Common.DataObjects;

namespace PoMo.Client.Views.Positions
{
    public sealed class PositionsViewModel : NotifierBase, ISubscriber, IDisposable
    {
        private readonly ShellViewModel _shellViewModel;

        public PositionsViewModel(ShellViewModel shellViewModel, PortfolioModel parameter)
        {
            this._shellViewModel = shellViewModel;
            this.Portfolio = parameter;
        }

        public PortfolioModel Portfolio
        {
            get;
        }

        void ISubscriber.Subscribe()
        {
            
        }

        void ISubscriber.Unsubscribe()
        {
        }

        public void Dispose()
        {
            
        }
    }
}