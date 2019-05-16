using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Busy.Tests
{
    public static class TestTransportContext
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

    public class TestTransportMessage
    {
        public static string BusinessSubject = "BUS";
        public static string AcknowledgeSubject = "ACK";

        public TestTransportMessage()
        {
            Id = Guid.NewGuid();
            Subject = BusinessSubject;
            Content = new string('*', 10000);
        }

        public TestTransportMessage(string subject)
        {
            Id = Guid.NewGuid();
            Subject = subject;
            Content = new string('*', 10000);
        }

        public Guid Id { get; set; }
        public String Subject { get; set; }
        public string Content { get; set; }

    }

    [TestFixture]
    public class TestTransport
    {
        [Test]
        public async Task ShouldCloseGracefully()
        {
            var transportConfiguration = new TransportConfiguration()
            {
                InboundEndPoint = "tcp://localhost:7979",
                PeerId = new PeerId("TestTransport")
            };

            var receiver = new Peer(transportConfiguration.PeerId, transportConfiguration.InboundEndPoint);

            var serializer = new JsonMessageSerializer();
            var logger = new MockLogger();
            var transport = new Transport(serializer, logger);

            transport.Configure(transportConfiguration.PeerId, transportConfiguration.InboundEndPoint);

            transport.Start();

            transport.MessageReceived += (msg) =>
            {
                TestTransportContext.Increment();
            };

            var task = Task.Run(() =>
            {

                for (var i = 0; i < 100; i++)
                {
                    var testTransportMessage = new TestTransportMessage();
                    var message = serializer.Serialize(testTransportMessage);
                    var transportMessage = new TransportMessage(new MessageTypeId(typeof(TestTransportMessage)), message, transportConfiguration.PeerId);
                    transport.Send(transportMessage, new[] { receiver });
                }

            });

            while (!task.IsCompleted)
            {
                await Task.Delay(200);
            }

            Assert.AreEqual(true, task.IsCompletedSuccessfully);
           
            transport.Stop();

            Assert.AreEqual(100, TestTransportContext.Get());
        }


        [Test]
        public async Task ShouldMakeARoundTrip()
        {
            var transportConfiguration = new TransportConfiguration()
            {
                InboundEndPoint = "tcp://localhost:7979",
                PeerId = new PeerId($"{Guid.NewGuid()}")
            };

            var receiver = new Peer(transportConfiguration.PeerId, transportConfiguration.InboundEndPoint);

            var serializer = new JsonMessageSerializer();
            var logger = new MockLogger();

            var transport = new Transport(serializer, logger);
            transport.Configure(transportConfiguration.PeerId, transportConfiguration.InboundEndPoint);

            var testTransportMessage = new TestTransportMessage();
            var message = serializer.Serialize(testTransportMessage);
            var transportMessage = new TransportMessage(new MessageTypeId(typeof(TestTransportMessage)), message, transportConfiguration.PeerId);

            transport.Start();

            transport.MessageReceived += (msg) =>
             {
                 if (TestTransportContext.Get() == 0)
                 {
                     Assert.AreEqual(transportMessage.Id, msg.Id);
                     Assert.AreEqual(transportMessage.Sender, transportConfiguration.PeerId);

                     var testTransportMessage2 = serializer.Deserialize(msg.Content, Type.GetType(msg.MessageTypeId.FullName)) as TestTransportMessage;
                     Assert.AreEqual(testTransportMessage.Id, testTransportMessage2.Id);
                     Assert.AreEqual(testTransportMessage.Subject, testTransportMessage2.Subject);
                     Assert.AreEqual(testTransportMessage.Content, testTransportMessage2.Content);

                     TestTransportContext.Increment();

                     var ackMessage = new TestTransportMessage(TestTransportMessage.AcknowledgeSubject);
                     var ackTransportMessage = new TransportMessage(new MessageTypeId(typeof(TestTransportMessage)), serializer.Serialize(ackMessage), transportConfiguration.PeerId);

                     transport.Send(ackTransportMessage, new[] { new Peer(msg.Sender, transportConfiguration.InboundEndPoint) });
                 }

                 else if (TestTransportContext.Get() == 1)
                 {

                     var testTransportMessage2 = serializer.Deserialize(msg.Content, Type.GetType(msg.MessageTypeId.FullName)) as TestTransportMessage;
                     Assert.AreEqual(TestTransportMessage.AcknowledgeSubject, testTransportMessage2.Subject);
                  
                     TestTransportContext.Increment();
                 }

             };


            transport.Send(transportMessage, new[] { receiver });

            await Task.Delay(1000);

            Assert.AreEqual(2, TestTransportContext.Get());

            transport.Stop();
        }

    }

}
