using System;
using System.ServiceModel.Channels;

namespace PoMo.Common.ServiceModel
{
    public static class BindingFactory
    {
        public static Binding CreateBinding()
        {
            return new CustomBinding
            {
                SendTimeout = TimeSpan.FromMinutes(10d),
                Namespace = Namespace.Value,
                Name = nameof(Binding),
                Elements =
                {
                    new BinaryMessageEncodingBindingElement
                    {
                        CompressionFormat = CompressionFormat.GZip,
                        ReaderQuotas =
                        {
                            MaxArrayLength = 0x1000000
                        }
                    },
                    new TcpTransportBindingElement
                    {
                        MaxReceivedMessageSize = 0x1000000,
                        MaxBufferSize = 0x1000000
                    }
                }
            };
        }
    }
}