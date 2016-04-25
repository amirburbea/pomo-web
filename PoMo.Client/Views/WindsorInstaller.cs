using System;
using System.Windows;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using PoMo.Client.Views.Shell;

namespace PoMo.Client.Views
{
    public sealed class WindsorInstaller : IWindsorInstaller
    {
        void IWindsorInstaller.Install(IWindsorContainer container, IConfigurationStore store)
        {
            container
                .Register(Classes.FromThisAssembly().BasedOn<Window>().LifestyleTransient())
                .Register(Classes.FromThisAssembly().Where(type => type != typeof(ShellViewModel) && type.Name.EndsWith("ViewModel", StringComparison.Ordinal)).LifestyleTransient());
        }
    }
}