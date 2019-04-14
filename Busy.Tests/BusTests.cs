using Busy.Infrastructure;
using NetMQ;
using NetMQ.Sockets;
using NUnit.Framework;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Busy.Tests
{
    [TestFixture]
    public class BusTests
    {

        [Test]
        public void TestSocket()
        {

        }


        [Test]
        public void TestCreateBus()
        {
            var container = new Container();

            var transport = new ZeroMQTransport("tcp://*:129");
    
            var busConfiguration = new BusConfiguration();
            var peerRepository = new MemoryPeerRepository();
            var directoryCOnfiguration = new DirectoryConfiguration();
            var peerDirectory = new PeerDirectoryServer(directoryCOnfiguration, peerRepository);
            var serializer = new JsonSerializer();
            var pipeSources = new List<IPipeSource>().ToArray();
            var pipeManager = new PipeManager(pipeSources);
            var dispatchQueueFactory = new DispatchQueueFactory(pipeManager, busConfiguration);

            var messageInvokers = new IMessageHandlerInvokerLoader[] {
                new SyncMessageHandlerInvokerLoader(container),
                new AsyncMessageHandlerInvokerLoader(container) };

            var dispatcher = new MessageDispatcher(messageInvokers, dispatchQueueFactory);
            var bindingKeyPredicateBuilder = new BindingKeyPredicateBuilder();

            var peerId = new PeerId("Abc.Testing." + Guid.NewGuid());

            var bus = new Bus(transport, peerDirectory, serializer, dispatcher, bindingKeyPredicateBuilder, busConfiguration);
            bus.Configure(peerId, "test");
            bus.Start();

        }

    }
}
