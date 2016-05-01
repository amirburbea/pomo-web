using Owin;

namespace PoMo.Server.Web
{
    internal interface IOwinStartup
    {
        void OnStartup(IAppBuilder app);
    }
}