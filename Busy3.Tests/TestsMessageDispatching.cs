using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Busy;
using NUnit.Framework;
using StructureMap;

namespace Busy.Tests
{
    public static class TestsMessageDispatchingContext2
    {
        private static volatile int _counter;

        public static int Get()
        {
            return _counter;
        }

        public static void Increment()
        {
            Interlocked.Increment(ref _counter);
        }

        public static void Reset()
        {
            Interlocked.Exchange(ref _counter, 0);
        }
    }
    public static class TestsMessageDispatchingContext
    {
        private static volatile int _counter;

        public static int Get()
        {
            return _counter;
        }

        public static void Increment()
        {
            Interlocked.Increment(ref _counter);
        }

        public static void Reset()
        {
            Interlocked.Exchange(ref _counter, 0);
        }
    }

    [TestFixture]
    public class TestsMessageDispatching
    {

        [SetUp]
        public void SetUp()
        {
            TestsMessageDispatchingContext.Reset();
            TestsMessageDispatchingContext2.Reset();
        }


        [Test]
        public async Task TestMessageQueue()
        {
         
            var logger = new MockLogger();

            var container = new Container(configuration => configuration.AddRegistry<BusRegistry>());

            var messages = Enumerable.Range(0, 50)
                .Select(_ => new TestMessage())
                .Select(msg => new MessageDispatch(msg));

            var queue = new DispatchQueue(logger, 10, "TestQueue");
            var cache = container.GetInstance<IMessageHandlerInvokerCache>();

            Assert.AreEqual(0, TestsMessageDispatchingContext.Get());

            queue.Start();

            foreach (var dispatch in messages)
            {
                var type = typeof(IMessageHandler<>).MakeGenericType(dispatch.Message.GetType());
                var invoker = new MessageHandlerInvoker(cache, MessageHandlerInvokerMode.Synchronous, type);

                queue.RunOrEnqueue(dispatch, invoker);
            }

            await Task.Delay(1000);

            Assert.AreEqual(100, TestsMessageDispatchingContext.Get());
        }

        [Test]
        public async Task TestMessageDispatcher()
        {

            var logger = new MockLogger();

            var container = new Container(configuration => configuration.AddRegistry<BusRegistry>());

            var messages1 = Enumerable.Range(0, 50)
                .Select(_ => new TestMessage())
                .Select(msg => new MessageDispatch(msg))
                .ToList();

            var messages2 = Enumerable.Range(0, 100)
                .Select(_ => new TestMessage2())
                .Select(msg => new MessageDispatch(msg))
                .ToList();


            Assert.AreEqual(0, TestsMessageDispatchingContext.Get());
            Assert.AreEqual(0, TestsMessageDispatchingContext2.Get());

            var messageDispatcher = new MessageDispatcher(logger, container);

            messageDispatcher.Dispatch(messages1.Concat(messages2).ToList());

            await Task.Delay(1000);

            Assert.AreEqual(100, TestsMessageDispatchingContext.Get());

            Assert.AreEqual(100, TestsMessageDispatchingContext2.Get());
        }
    }
}
