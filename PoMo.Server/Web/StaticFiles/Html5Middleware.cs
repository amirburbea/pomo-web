using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Owin.StaticFiles;

namespace PoMo.Server.Web.StaticFiles
{
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
            if (!environment.TryGetValue(OwinKeys.ResponseStatusCode, out value) || (HttpStatusCode)(int)value != HttpStatusCode.NotFound ||
                !environment.TryGetValue(OwinKeys.RequestPath, out value) || ((string)value).EndsWith(".map", StringComparison.Ordinal))
            {
                // If the response is not a 404, or it is a 404 but for a .map file on a minimized js file.
                return;
            }
            environment[OwinKeys.RequestPath] = "/index.html";
            await this._staticFileMiddleware.Invoke(environment);
        }

        private static class OwinKeys
        {
            public const string RequestPath = "owin.RequestPath";

            public const string ResponseStatusCode = "owin.ResponseStatusCode";
        }
    }
}