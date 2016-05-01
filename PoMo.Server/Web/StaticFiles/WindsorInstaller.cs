using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using PoMo.Server.Properties;

namespace PoMo.Server.Web.StaticFiles
{
    public sealed class WindsorInstaller : IWindsorInstaller
    {
        void IWindsorInstaller.Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<IStaticFileSettings>().Instance(Settings.Default).Named(nameof(IStaticFileSettings)));
        }
    }
}