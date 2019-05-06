using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Busy.Tests
{
    public class Message
    {
        public string Subject { get; set; }
        public string Endpoint { get; set; }
    }

    public static class TestsNetMQContext
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

    public class NetMQPeerDirectory
    {
        public const string Ack = "Ack";
        public const string Event = "Event";
        public const string Command = "Command";
        public const string RegisterCommand = "Register";
        public const string Separator = "-";

        private List<string> _peers = new List<string>();
        private object _locker = new Object();

        public NetMQPeerDirectory(string inboundSocker, CancellationToken token)
        {

            Task.Run(() =>
            {
                using (var receiver = new PullSocket(inboundSocker))
                {
                    while (true)
                    {
                        var command = receiver.ReceiveFrameBytes();

                        var msg = JsonConvert.DeserializeObject<Message>(Encoding.UTF32.GetString(command));

                        if (msg.Subject == RegisterCommand)
                        {
                            TestsNetMQContext.Increment();

                            var peer = msg.Endpoint;

                            lock (_locker)
                            {
                                _peers.Add(peer);

                                using (var sender = new PushSocket(peer))
                                {
                                    var message = new Message()
                                    {
                                        Subject = NetMQPeerDirectory.Ack
                                    };

                                    var ack = Encoding.UTF32.GetBytes(JsonConvert.SerializeObject(message));

                                    sender.SendFrame(ack);
                                }
                            }
                        }
                    }
                }
            }, token);
        }
    }

    public class NetMQPeer
    {
       
        private Random _rand;

        public NetMQPeer(string endpoint, string directoryEndpoint, CancellationToken token)
        {

            Endpoint = endpoint;
            DirectoryEndpoint = directoryEndpoint;

            _token = token;
            _rand = new Random();

        }

        public String Endpoint { get; private set; }
        public String DirectoryEndpoint { get; private set; }

        private CancellationToken _token;

        public void Start()
        {

            Task.Run(() =>
            {
                using (var receiver = new PullSocket(Endpoint))
                {
                    while (true)
                    {
                        var command = receiver.ReceiveFrameBytes();

                        var msg = JsonConvert.DeserializeObject<Message>(Encoding.UTF32.GetString(command));

                        switch (msg.Subject)
                        {
                            case NetMQPeerDirectory.Ack:
                                TestsNetMQContext.Increment();
                                break;

                            case NetMQPeerDirectory.Command:
                                TestsNetMQContext.Increment();
                                break;

                            case NetMQPeerDirectory.Event:
                                TestsNetMQContext.Increment();
                                break;
                        }
                    }
                }
            }, _token);


            Task.Run(async() =>
            {
                using (var sender = new PushSocket($"{DirectoryEndpoint}"))
                {
                    var message = new Message()
                    {
                        Endpoint = Endpoint,
                        Subject = NetMQPeerDirectory.RegisterCommand
                    };

                    var msg = Encoding.UTF32.GetBytes(JsonConvert.SerializeObject(message));

                    sender.SendFrame(msg);

                    await Task.Delay(100);
                }
            });
        }
    }

    [TestFixture]
    public class TestsNetMQ
    {
        [SetUp]
        public void SetUp()
        {
            TestsNetMQContext.Reset();
        }

        [Test]
        public async Task TestPushPullSocket()
        {
            var directory = "tcp://localhost:7979";

            var cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;

            var peer1Endpoint = "tcp://localhost:8181";
            var peer2Endpoint = "tcp://localhost:8080";

            Assert.AreEqual(0, TestsNetMQContext.Get());

            var peerDirectory = new NetMQPeerDirectory(directory, token);

            var peer1 = new NetMQPeer(peer1Endpoint, directory, token);
            var peer2 = new NetMQPeer(peer2Endpoint, directory, token);

            peer1.Start();
            peer2.Start();

            await Task.Delay(500);

            Assert.AreEqual(4, TestsNetMQContext.Get());

            cancellationTokenSource.Cancel();

        }
    }
}
