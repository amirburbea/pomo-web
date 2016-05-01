using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;

namespace PoMo.Server.Web.StaticFiles
{
    public sealed class OwinStartup : IOwinStartup
    {
        private readonly IFileSystem _fileSystem;

        public OwinStartup(IStaticFileSettings settings)
        {
            this._fileSystem = new PhysicalFileSystem(settings.WebRoot);
        }

        void IOwinStartup.OnStartup(IAppBuilder app)
        {
            app.UseFileServer(new FileServerOptions
            {
                FileSystem = this._fileSystem,
                StaticFileOptions =
                {
                    FileSystem = this._fileSystem,
                    ServeUnknownFileTypes = true,
                },
                DefaultFilesOptions =
                {
                    DefaultFileNames =
                    {
                        "index.html"
                    }
                }
            });
        }
    }
}