using Microsoft.Extensions.Logging;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
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

            Scan(scanner =>
            {
                scanner.AssembliesAndExecutablesFromApplicationBaseDirectory();
                scanner.WithDefaultConventions();
                scanner.LookForRegistries();
                scanner.AddAllTypesOf(typeof(IMessageHandler<>));
                scanner.Exclude((type) =>
                {
                    return type.GetInterfaces().Any(@interface => typeof(IPeerDirectory) == @interface);
                });
         
            });

        }

    }
}
