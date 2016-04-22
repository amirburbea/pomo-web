using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using PoMo.Common.ServiceModel.Contracts;
using PoMo.Common.Windsor;
using PoMo.Server.Properties;

namespace PoMo.Server
{
    public sealed class Application : IWindowsService
    {
        private readonly Binding _binding;
        private readonly IFactory<IServiceContract> _serviceFactory;
        private ServiceHost _host;

        public Application(IFactory<IServiceContract> serviceFactory, Binding binding)
        {
            this._serviceFactory = serviceFactory;
            this._binding = binding;
        }

        void IWindowsService.Start()
        {
            this._host = new ServiceHost(this._serviceFactory.Resolve());
            this._host.AddServiceEndpoint(typeof(IServiceContract), this._binding, Settings.Default.WcfUri);
            this._host.Description.Behaviors.Find<ServiceDebugBehavior>().IncludeExceptionDetailInFaults = true;
            this._host.Open();
        }

        void IWindowsService.Stop()
        {
            this._host.Close();
            this._serviceFactory.Release((IServiceContract)this._host.SingletonInstance);
        }
    }
}