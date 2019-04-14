using Busy.Infrastructure;
using NUnit.Framework;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Busy.Tests
{
    [TestFixture]
    public class DispatchQueueTests
    {

        [Test]
        public async Task ShouldTestMessageDispatcher()
        {
            var handler = new DatabaseNodeFailureHandler();

            var container = new Container();
            container.Inject(handler);
            container.Inject<IBus>(new FakeBus());

            var busConfiguration = new BusConfiguration();

            var messageInvokers = new IMessageHandlerInvokerLoader[] {
                new SyncMessageHandlerInvokerLoader(container),
                new AsyncMessageHandlerInvokerLoader(container) };

            var pipeSources = new List<IPipeSource>().ToArray();
            var pipeManager = new PipeManager(pipeSources);

            var dispatchQueueFactory = new DispatchQueueFactory(pipeManager, busConfiguration);

            var dispatcher = new MessageDispatcher(messageInvokers, dispatchQueueFactory);

            dispatcher.LoadMessageHandlerInvokers();
            
            dispatcher.Start();


            var messageKo = new DatabaseStatus()
            {
                DatacenterName = "Paris",
                Status = "Ko"

            };

            var messageContext = new MessageContext();

            var messageDispatch = new MessageDispatch(messageContext, messageKo, (_, __) => { });


            Assert.IsFalse(DatabaseNodeFailureHandler.HasBeenHandled);

          
            dispatcher.Dispatch(messageDispatch);

            await Task.Delay(1000);

            Assert.IsTrue(DatabaseNodeFailureHandler.HasBeenHandled);
        }

        [Test]
        public async Task ShouldTestDispatchQueue()
        {
            var messageKo = new DatabaseStatus()
            {
                DatacenterName = "Paris",
                Status = "Ko"

            };

            var handler = new DatabaseNodeFailureHandler();


            var container = new Container();

            container.Inject(new DatabaseNodeFailureHandler());

            container.Inject<IBus>(new FakeBus());

            var pipeSources = new List<IPipeSource>().ToArray();
            var pipeManager = new PipeManager(pipeSources);
            var queue = new DispatchQueue(pipeManager, 20, "MyQueue");

            queue.Start();

            var messageContext = new MessageContext();

            var invoker = new SyncMessageHandlerInvoker(container, typeof(DatabaseNodeFailureHandler), typeof(DatabaseStatus));

            var messageDispatch = new MessageDispatch(messageContext, messageKo, (_, __) => { });

            queue.Enqueue(messageDispatch, invoker);

            Assert.IsTrue(queue.QueueLength == 1);
            Assert.IsFalse(DatabaseNodeFailureHandler.HasBeenHandled);

            await Task.Delay(1000);

            Assert.IsTrue(queue.QueueLength == 0);

            Assert.IsTrue(DatabaseNodeFailureHandler.HasBeenHandled);
        }
    }
}
