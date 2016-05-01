using System;
using System.Net;
using System.Threading;
using Microsoft.Owin.Hosting;

namespace PoMo.Server.Web
{
    public interface IWebManager
    {
        void Start();

        void Stop();
    }

    internal sealed class WebManager : IWebManager, IDisposable
    {
        private readonly IOwinStartup[] _startupHandlers;
        private readonly string _url;
        private IDisposable _webApplication;

        public WebManager(IWebSettings webSettings, IOwinStartup[] startupHandlers)
        {
            // If you whitelist a port using netsh urlacl commands, you can skip the administrator check.  
            // Otherwise unelevated processes can only host on localhost. 
            string host = webSettings.SkipAdministratorCheck || UacProperties.IsProcessElevated ? "+" : "localhost";
            this._url = $"http://{host}:{webSettings.Port}/pomo/";
            this._startupHandlers = startupHandlers;
        }

        public void Dispose()
        {
            this.Stop();
        }

        void IWebManager.Start()
        {
            this._webApplication = WebApp.Start(
                new StartOptions(this._url),
                app =>
                {
                    HttpListener listener = (HttpListener)app.Properties[typeof(HttpListener).FullName];
                    listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
                    foreach (IOwinStartup handler in this._startupHandlers)
                    {
                        handler.OnStartup(app);
                    }
                }
            );
        }

        void IWebManager.Stop()
        {
            this.Stop();
        }

        private void Stop()
        {
            Interlocked.Exchange(ref this._webApplication, null)?.Dispose();
        }
    }
}