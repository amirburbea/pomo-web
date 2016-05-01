namespace PoMo.Client.Properties
{
    partial class Settings : IWebSettings
    {
        string IWebSettings.Host => this.WebHost;

        int IWebSettings.Port => this.WebPort;
    }
}
