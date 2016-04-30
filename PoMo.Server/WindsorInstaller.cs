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
                .Register(Component.For<IApplication>().ImplementedBy<Application>())
                .Register(Component.For<DataContext, IDataContext>().ImplementedBy<DataContext>())
                .Register(Component.For<IFirmData>().ImplementedBy<FirmData>())
                .Register(Component.For<IServerContract>().ImplementedBy<DataService>())
                .Register(Component.For<ITradeFactory>().ImplementedBy<TradeFactory>())
                .Register(Component.For<IMarketDataProvider>().ImplementedBy<MarketDataProvider>())
                .Register(Component.For<Binding>().UsingFactoryMethod(BindingFactory.CreateBinding));
        }
    }
}