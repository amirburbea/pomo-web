using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using PoMo.Server.Web;
using PoMo.Server.Web.StaticFiles;

namespace PoMo.Server.Properties
{
    public sealed class WindsorInstaller : IWindsorInstaller
    {
        void IWindsorInstaller.Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<IWebSettings, IStaticFileSettings>().Instance(Settings.Default));
        }
    }
}