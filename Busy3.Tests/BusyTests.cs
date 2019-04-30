using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Busy;
using Busy.Handler;
using NUnit.Framework;
using StructureMap;

namespace Busy.Tests
{
    public static class TestContext2
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
    public static class TestContext
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
    public class BusyTests
    {
        [Test]
        public async Task TestBus()
        {

        }

        [Test]
        public async Task TestMessageQueue()
        {
            TestContext.Reset();

            var logger = new MockLogger();

            var container = new Container(configuration => configuration.AddRegistry<AppRegistry>());

            var messages = Enumerable.Range(0, 50)
                .Select(_ => new TestMessage())
                .Select(msg => new MessageDispatch(msg));

            var queue = new DispatchQueue(logger, 10, "TestQueue");

            Assert.AreEqual(0, TestContext.Get());

            queue.Start();

            foreach (var dispatch in messages)
            {
                var type = typeof(IMessageHandler<>).MakeGenericType(dispatch.Message.GetType());
                var invoker = new MessageHandlerInvoker(container, MessageHandlerInvokerMode.Synchronous, type);

                queue.RunOrEnqueue(dispatch, invoker);
            }

            await Task.Delay(1000);

            Assert.AreEqual(100, TestContext.Get());
        }

        [Test]
        public async Task TestMessageDispatcher()
        {
            TestContext.Reset();
            TestContext2.Reset();

            var logger = new MockLogger();

            var container = new Container(configuration => configuration.AddRegistry<AppRegistry>());

            var messages1 = Enumerable.Range(0, 50)
                .Select(_ => new TestMessage())
                .Select(msg => new MessageDispatch(msg))
                .ToList();

            var messages2 = Enumerable.Range(0, 100)
                .Select(_ => new TestMessage2())
                .Select(msg => new MessageDispatch(msg))
                .ToList();


            Assert.AreEqual(0, TestContext.Get());
            Assert.AreEqual(0, TestContext2.Get());

            var messageDispatcher = new MessageDispatcher(logger, container);

            messageDispatcher.Dispatch(messages1.Concat(messages2).ToList());

            await Task.Delay(1000);

            Assert.AreEqual(100, TestContext.Get());

            Assert.AreEqual(100, TestContext2.Get());
        }
    }
}
