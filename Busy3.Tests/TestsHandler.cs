using NUnit.Framework;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Busy.Tests
{
    public class MessageA : IMessage
    {

    }

    public class MessageB : IMessage
    {

    }

    public class HandlerA : IMessageHandler<MessageA>
    {
        public void Handle(MessageA message)
        {
        }

        public void Handle(IMessage message)
        {
        }
    }

    public class HandlerB : IMessageHandler<MessageB>
    {
        public void Handle(MessageB message)
        {
        }
        public void Handle(IMessage message)
        {
        }
    }

    [Singleton]
    public class SingletonHandler : IMessageHandler<MessageB>, IMessageHandler<MessageA>
    {
        public Guid Id { get; internal set; }

        public void Handle(MessageB message)
        {
        }

        public void Handle(MessageA message)
        {
         
        }

        public void Handle(IMessage message)
        {
        }
    }
    

    [TestFixture]
    public class TestsHandler
    {
        private Container _container;

        [OneTimeSetUp]
        public void Setup()
        {
            _container = new Container(configuration => configuration.AddRegistry<AppRegistry>());
        }

        [Test]
        public void ShouldGetAllExecutingAssemblyHandlers()
        {
            var singleton = _container.GetInstance<SingletonHandler>();
            singleton.Id = Guid.NewGuid();

            var handlerA = new HandlerA();
            var handlerA2 = new HandlerA();

            _container.Configure(configuration =>
            {
                configuration.For<SingletonHandler>().Use(singleton);
                configuration.Forward<SingletonHandler, IMessageHandler>();
            });

            _container.Configure(configuration =>
            {
                configuration.For<IMessageHandler>().Use(handlerA);
                configuration.For<IMessageHandler>().Use(handlerA2);
            });

            var handlers = _container.GetAllInstances<IMessageHandler>();

            Assert.IsTrue(handlers.All((handler) => handlers.Any(h => h.GetType() == handler.GetType())));


        }

    }
}
