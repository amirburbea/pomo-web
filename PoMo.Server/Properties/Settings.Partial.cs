using PoMo.Server.Web;
using PoMo.Server.Web.StaticFiles;

namespace PoMo.Server.Properties
{
    partial class Settings : IWebSettings, IStaticFileSettings
    {
        int IWebSettings.Port => this.WebPort;

        bool IWebSettings.SkipAdministratorCheck => this.WebSkipAdministratorCheck;
    }
}
