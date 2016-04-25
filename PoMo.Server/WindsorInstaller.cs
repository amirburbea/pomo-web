using System.ServiceModel.Channels;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using PoMo.Common.ServiceModel;
using PoMo.Common.ServiceModel.Contracts;
using PoMo.Data;

namespace PoMo.Server
{
    public sealed class WindsorInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container
                .Register(Component.For<IWindowsService>().ImplementedBy<Application>())
                .Register(Component.For<DataContext>())
                .Register(Component.For<IServerContract>().ImplementedBy<WcfService>())
                .Register(Component.For<Binding>().UsingFactoryMethod(BindingFactory.CreateBinding));
        }
    }
}