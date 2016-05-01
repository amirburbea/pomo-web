using PoMo.Server.Web;

namespace PoMo.Server.Properties
{
    partial class Settings : IWebSettings
    {
        int IWebSettings.Port => this.WebPort;

        bool IWebSettings.SkipAdministratorCheck => this.WebSkipAdministratorCheck;
    }
}
