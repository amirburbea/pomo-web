using System.Web.Http;
using System.Web.Http.Dispatcher;
using Newtonsoft.Json.Serialization;
using Owin;

namespace PoMo.Server.Web.WebApi
{
    public sealed class OwinStartup : IOwinStartup
    {
        private readonly HttpConfiguration _httpConfiguration;

        public OwinStartup(IHttpControllerActivator controllerActivator, IContractResolver contractResolver)
        {
            this._httpConfiguration = new HttpConfiguration
            {
                Formatters =
                {
                    JsonFormatter =
                    {
                        SerializerSettings =
                        {
                            ContractResolver = contractResolver
                        }
                    }
                }
            };
            this._httpConfiguration.Services.Replace(typeof(IHttpControllerActivator), controllerActivator);
            this._httpConfiguration.Formatters.Remove(this._httpConfiguration.Formatters.XmlFormatter);
            this._httpConfiguration.Routes.MapHttpRoute("api", "api/{controller}/{id}", new { id = RouteParameter.Optional });
        }

        void IOwinStartup.OnStartup(IAppBuilder app)
        {
            app.UseWebApi(this._httpConfiguration);
        }
    }
}