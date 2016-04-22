using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using PoMo.ServiceModel.Contracts;

namespace PoMo.Client.ServiceClient
{
    public class ChannelFactoryFactory
    {
        public object CreateChannelFactory(Binding binding, IClientContract callback, string uri)
        {
            ChannelFactory<IServiceContract> factory = new DuplexChannelFactory<IServiceContract>(callback, binding, new EndpointAddress(uri));
            IServiceCotfactory.CreateChannel();
            return factory;
        }
    }
}
