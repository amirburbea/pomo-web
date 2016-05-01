using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using PoMo.Data;

namespace PoMo.Server
{
    public sealed class WindsorInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container
                .Register(Component.For<IApplication>().ImplementedBy<Application>())
                .Register(Component.For<ITradeRepository, IDataContext>().ImplementedBy<DataContext>())
                .Register(Component.For<IFirmData>().ImplementedBy<FirmData>())
                .Register(Component.For<ITradeFactory>().ImplementedBy<TradeFactory>())
                .Register(Component.For<IMarketDataProvider>().ImplementedBy<MarketDataProvider>());
        }
    }
}