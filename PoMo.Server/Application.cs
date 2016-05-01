using PoMo.Server.Web;

namespace PoMo.Server
{
    public interface IApplication
    {
        void Start();

        void Stop();
    }

    public sealed class Application : IApplication
    {
        private readonly IWebManager _webManager;

        public Application(IWebManager webManager)
        {
            this._webManager = webManager;
        }

        void IApplication.Start()
        {
            this._webManager.Start();
        }

        void IApplication.Stop()
        {
            this._webManager.Stop();
        }
    }
}