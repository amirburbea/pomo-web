using Microsoft.AspNet.SignalR;
using Owin;

namespace PoMo.Server.Web.SignalR
{
    public sealed class OwinStartup : IOwinStartup
    {
        private readonly IDependencyResolver _dependencyResolver;

        public OwinStartup(IDependencyResolver dependencyResolver)
        {
            this._dependencyResolver = dependencyResolver;
        }

        void IOwinStartup.OnStartup(IAppBuilder app)
        {
            GlobalHost.DependencyResolver = this._dependencyResolver;
            // If not on the same port/web server serving HTML you would need to enable CORS support and possibly JSONP.  
            // SignalR has settings to enable them. This app however, does not need these.
            app.MapSignalR();
        }
    }
}