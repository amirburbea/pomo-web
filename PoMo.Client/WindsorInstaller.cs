using System.ServiceModel.Channels;
using System.Windows;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using PoMo.Common.ServiceModel;

namespace PoMo.Client
{
    public sealed class WindsorInstaller : IWindsorInstaller
    {
        void IWindsorInstaller.Install(IWindsorContainer container, IConfigurationStore store)
        {
            container
                .Register(Component.For<Binding>().UsingFactoryMethod(BindingFactory.CreateBinding))
                .Register(Component.For<Application>().ImplementedBy<App>().OnCreate(app => ((App)app).InitializeComponent()));
        }
    }
}