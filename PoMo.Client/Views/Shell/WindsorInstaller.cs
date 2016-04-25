using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using PoMo.Client.Controls;

namespace PoMo.Client.Views.Shell
{
    public sealed class WindsorInstaller : IWindsorInstaller
    {
        void IWindsorInstaller.Install(IWindsorContainer container, IConfigurationStore store)
        {
            container
                .Register(Component.For<ShellViewModel>())
                .Register(Component.For<ITabTearOffHandler>().ImplementedBy<ShellViewTearOffHandler>());
        }
    }
}