using NetMQ;
using NetMQ.Sockets;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Busy.Tests
{

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
                        var command = receiver.ReceiveFrameString();

                        if (command.StartsWith(RegisterCommand))
                        {
                            TestsNetMQContext.Increment();

                            var peer = command.Split(Separator)[1];

                            lock (_locker)
                            {
                                _peers.Add(peer);

                                using (var sender = new PushSocket(peer))
                                {
                                    sender.SendFrame(Ack);
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
                        var command = receiver.ReceiveFrameString();

                        switch (command)
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
                    sender.SendFrame($"{NetMQPeerDirectory.RegisterCommand}{NetMQPeerDirectory.Separator}{Endpoint}");

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
