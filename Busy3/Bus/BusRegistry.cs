using Microsoft.Extensions.Logging;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Text;

namespace Busy
{
    public class BusRegistry : Registry
    {
        public BusRegistry()
        {
            For<IMessageDispatcher>().Use<MessageDispatcher>().Singleton();
            For<IMessageHandlerInvokerCache>().Use<MessageHandlerInvokerCache>().Singleton();
            For<ITransport>().Use<Transport>().Singleton();
            For<IBus>().Use<Bus>().Singleton();
            For<IPeerDirectory>().Use<PeerDirectoryClient>();

            Scan(scanner =>
            {
                scanner.AssembliesAndExecutablesFromApplicationBaseDirectory();
                scanner.WithDefaultConventions();
                scanner.LookForRegistries();
                scanner.ConnectImplementationsToTypesClosing(typeof(IMessageHandler<>));
            });

        }

    }
}
