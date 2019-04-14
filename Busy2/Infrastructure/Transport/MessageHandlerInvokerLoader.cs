using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Busy.Infrastructure
{
    public abstract class MessageHandlerInvokerLoader : IMessageHandlerInvokerLoader
    {
        private readonly Type _genericHandlerType;
        private readonly Type _handlerType;

        protected MessageHandlerInvokerLoader(IContainer container, Type genericHandlerType)
        {
            _handlerType = genericHandlerType.GetInterfaces().Single();
            _genericHandlerType = genericHandlerType;

            Container = container;
        }

        protected IContainer Container { get; private set; }

        public IEnumerable<IMessageHandlerInvoker> LoadMessageHandlerInvokers(params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {

                foreach (var handlerType in assembly.GetTypes())
                {
                    if (!handlerType.IsClass || handlerType.IsAbstract || !handlerType.IsVisible || !_handlerType.IsAssignableFrom(handlerType))
                        continue;

                    var subscriptionMode = MessageHandlerInvoker.GetExplicitSubscriptionMode(handlerType);
                    var interfaces = handlerType.GetInterfaces();

                    var handleInterfaces = interfaces.Where(IsMessageHandlerInterface);
                    foreach (var handleInterface in handleInterfaces)
                    {
                        var messageType = handleInterface.GetGenericArguments()[0];

                        var shouldBeSubscribedOnStartup = MessageHandlerInvoker.MessageShouldBeSubscribedOnStartup(messageType, subscriptionMode);
                        var invoker = BuildMessageHandlerInvoker(handlerType, messageType, shouldBeSubscribedOnStartup);
                        yield return invoker;
                    }
                }

            }
        }

        protected abstract IMessageHandlerInvoker BuildMessageHandlerInvoker(Type handlerType, Type messageType, bool shouldBeSubscribedOnStartup);

   
        private bool IsMessageHandlerInterface(Type interfaceType)
        {
            return interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == _genericHandlerType && !interfaceType.GetGenericArguments()[0].IsGenericParameter;
        }
    }
}
