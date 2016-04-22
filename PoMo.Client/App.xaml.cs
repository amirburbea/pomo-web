using System.Windows;
using PoMo.Client.Shell;
using PoMo.Common.Windsor;

namespace PoMo.Client
{
    partial class App
    {
        private readonly IFactory<ShellView> _mainWindowFactory;

        public App(IFactory<ShellView> mainWindowFactory)
        {
            this._mainWindowFactory = mainWindowFactory;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            this._mainWindowFactory.Release((ShellView)this.MainWindow);
            base.OnExit(e);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            (this.MainWindow = this._mainWindowFactory.Resolve()).Show();
            base.OnStartup(e);
        }
    }
}