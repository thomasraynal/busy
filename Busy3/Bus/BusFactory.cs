using StructureMap;
using System;
using System.Collections.Generic;
using System.Text;

namespace Busy
{
    public static class BusFactory
    {
        private static Container _container;

        static BusFactory()
        {
            _container = new Container(configuration => configuration.AddRegistry<BusRegistry>());
        }

        public static IBus Create(string id, string endpoint, string directoryEndpoint, IContainer container = null)
        {
            var current = container == null ? _container : container;
            var bus = current.GetInstance<IBus>();
            bus.Configure(new PeerId(id), endpoint, directoryEndpoint);
            return bus;
        }

    }
}
