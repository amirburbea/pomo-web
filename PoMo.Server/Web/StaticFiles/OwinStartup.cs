using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;

namespace PoMo.Server.Web.StaticFiles
{
    public sealed class OwinStartup : IOwinStartup
    {
        private readonly StaticFileOptions _options;

        public OwinStartup(IStaticFileSettings settings)
        {
            this._options = new StaticFileOptions
            {
                FileSystem = new PhysicalFileSystem(settings.WebRoot)
            };
        }

        void IOwinStartup.OnStartup(IAppBuilder app)
        {
            //app.Use<StaticFileMiddleware>(this._options);
            app.UseDefaultFiles(new DefaultFilesOptions { DefaultFileNames = { "index.html" }, FileSystem = this._options.FileSystem });
            app.Use<Html5Middleware>(this._options);
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public sealed class Html5Middleware
        {
            private readonly StaticFileMiddleware _staticFileMiddleware;

            public Html5Middleware(Func<IDictionary<string, object>, Task> next, StaticFileOptions options)
            {
                this._staticFileMiddleware = new StaticFileMiddleware(next, options);
            }

            public async Task Invoke(IDictionary<string, object> environment)
            {
                // Attempt to serve it as a static file.
                await this._staticFileMiddleware.Invoke(environment);
                object value;
                if (!environment.TryGetValue("owin.ResponseStatusCode", out value) || 
                    (HttpStatusCode)(int)value != HttpStatusCode.NotFound ||
                    !environment.TryGetValue("owin.RequestPath", out value) || 
                    ((string)value).EndsWith(".map", StringComparison.Ordinal))
                {
                    // If the response is not a 404, or it is a 404 but for a .map file on a minimized js file.
                    return;
                }
                environment["owin.RequestPath"] = "/index.html";
                await this._staticFileMiddleware.Invoke(environment);
            }
        }
    }
}