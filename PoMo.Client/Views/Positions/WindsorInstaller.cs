using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace PoMo.Client.Views.Positions
{
    public sealed class WindsorInstaller : IWindsorInstaller
    {
        void IWindsorInstaller.Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<PositionsView>().LifestyleTransient());
        }
    }
}