using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace PoMo.Server.Web.SignalR
{
    public sealed class WindsorInstaller : IWindsorInstaller
    {
        void IWindsorInstaller.Install(IWindsorContainer container, IConfigurationStore store)
        {
            container
                // SignalR doesn't have a callback to its dependency resolver for releasing hubs.
                // The idea here is that hub resources are only singletons so it's fine to release immediately.
                .Register(Classes.FromThisAssembly().BasedOn(typeof(IHub)).LifestyleTransient().Configure(registration => registration.OnCreate(container.Release)))
                .Register(Component.For<IDataHubController>().ImplementedBy<DataHubController>())
                .Register(Component.For<IDependencyResolver>().Instance(new SignalRDependencyResolver(container)));
        }

        private sealed class SignalRDependencyResolver : DefaultDependencyResolver
        {
            private readonly IWindsorContainer _container;

            public SignalRDependencyResolver(IWindsorContainer container)
            {
                this._container = container;
            }

            public override object GetService(Type serviceType)
            {
                return !this._container.Kernel.HasComponent(serviceType) ? base.GetService(serviceType) : this._container.Kernel.Resolve(serviceType);
            }
        }
    }
}