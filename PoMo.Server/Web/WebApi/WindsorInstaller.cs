using System;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace PoMo.Server.Web.WebApi
{
    public sealed class WindsorInstaller : IWindsorInstaller
    {
        void IWindsorInstaller.Install(IWindsorContainer container, IConfigurationStore store)
        {
            container
                .Register(Component.For<IHttpControllerActivator>().Instance(new WindsorActivator(container)))
                .Register(Classes.FromThisAssembly().BasedOn<IHttpController>().LifestyleTransient());
        }

        private sealed class WindsorActivator : IHttpControllerActivator
        {
            private readonly IWindsorContainer _container;

            public WindsorActivator(IWindsorContainer container)
            {
                this._container = container;
            }

            IHttpController IHttpControllerActivator.Create(HttpRequestMessage request, HttpControllerDescriptor controllerDescriptor, Type controllerType)
            {
                IHttpController controller = (IHttpController)this._container.Resolve(controllerType);
                request.RegisterForDispose(new ComponentRelease(this._container, controller));
                return controller;
            }

            private sealed class ComponentRelease : IDisposable
            {
                private readonly IWindsorContainer _container;
                private readonly IHttpController _controller;

                public ComponentRelease(IWindsorContainer container, IHttpController controller)
                {
                    this._container = container;
                    this._controller = controller;
                }

                public void Dispose()
                {
                    this._container.Release(this._controller);
                }
            }
        }
    }
}