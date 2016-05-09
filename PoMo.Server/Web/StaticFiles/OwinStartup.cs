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
            app.UseDefaultFiles(new DefaultFilesOptions { DefaultFileNames = { "index.html" }, FileSystem = this._options.FileSystem });
            app.Use<Html5Middleware>(this._options);
        }
    }
}