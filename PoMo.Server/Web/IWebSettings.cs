namespace PoMo.Server.Web
{
    internal interface IWebSettings
    {
        int Port
        {
            get;
        }

        bool SkipAdministratorCheck
        {
            get;
        }
    }
}