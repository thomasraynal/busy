using Busy.Handler;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Text;

namespace Busy.Tests
{
    public class AppRegistry : Registry
    {
        public AppRegistry()
        {
            Scan(scanner =>
            {
                scanner.AssembliesAndExecutablesFromApplicationBaseDirectory();
                scanner.WithDefaultConventions();
                scanner.ConnectImplementationsToTypesClosing(typeof(IMessageHandler<>));
            });

        }

    }
}
