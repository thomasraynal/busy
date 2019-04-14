using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetMQTests
{
    public class PeerDirectory
    {
        private List<Peer> _peers = new List<Peer>();

        public PeerDirectory(string inboundSocker)
        {
            Task.Run(() =>
            {
                using (var receiver = new PullSocket(inboundSocker))
                {
                    while (true)
                    {
                        var workload = receiver.ReceiveFrameString();

                      //  Console.WriteLine($"Peer {Id} - received workload {workload}");
                    }
                }
            });


        }

        public IEnumerable<Peer> GetPeers()
        {
            return _peers;
        }

        public void Register(Peer peer)
        {
            _peers.Add(peer);
        }
    }

    public class Peer
    {
        public Peer(string inboundEndpoint, PeerDirectory peerDirectory)
        {
            Id = Guid.NewGuid();
            InboundSocketEndpoint = inboundEndpoint;

            _peerDirectory = peerDirectory;

            _rand = new Random();
        }

        public Guid Id { get; set; }
        public String InboundSocketEndpoint { get; private set; }


        private PeerDirectory _peerDirectory;
        private Random _rand;


        public void Start()
        {
            Console.WriteLine($"Start peer {Id}");

            Task.Run(() =>
            {
                using (var receiver = new PullSocket(InboundSocketEndpoint))
                {
                    while (true)
                    {
                        var workload = receiver.ReceiveFrameString();

                        Console.WriteLine($"Peer {Id} - received workload {workload}");
                    }
                }
            });


            foreach(var peer in _peerDirectory.GetPeers().Where(p=> p.Id != Id))
            {
                Task.Run(async () =>
                {
                    using (var sender = new PushSocket($"{peer.InboundSocketEndpoint}"))
                    {

                        while (true)
                        {

                            await Task.Delay(_rand.Next(1000, 3000));

                            Console.WriteLine($"Peer {Id} - send some info to peer {peer.Id}");

                            sender.SendFrame($"Some info from peer {Id} to peer {peer.Id} - {_rand.NextDouble()}");

                        }

                    }

                });
            }

        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var peerDirectorySocket = "tcp://localhost:7979";

            var socket1 = "tcp://localhost:8080";
            var socket2 = "tcp://localhost:8181";
            var socket3 = "tcp://localhost:8282";

            var peerDirectory = new PeerDirectory(peerDirectorySocket);

            var peer1 = new Peer(socket1, peerDirectory);
            var peer2 = new Peer(socket2, peerDirectory);
            var peer3 = new Peer(socket3, peerDirectory);
            
            peerDirectory.Register(peer1);
            peerDirectory.Register(peer2);
            peerDirectory.Register(peer3);


            var peers = new[] { peer1, peer2, peer3 };

            foreach (var peer in peers)
            {
                peer.Start();
            }




            Console.Read();





            //        Task.Run(async() =>
            //        {
            //            using (var sender = new PushSocket("@tcp://*:5557"))
            //            {


            //                //the first message it "0" and signals start of batch
            //                //see the Sink.csproj Program.cs file for where this is used
            //                //Console.WriteLine("Sending start of batch to Sink");
            //                //sink.SendFrame("0");

            //                await Task.Delay(1000);

            //                Console.WriteLine("Sending tasks to workers");

            //                //initialise random number generator
            //                Random rand = new Random(0);

            //                //expected costs in Ms
            //                int totalMs = 0;

            //                //send 100 tasks (workload for tasks, is just some random sleep time that
            //                //the workers can perform, in real life each work would do more than sleep
            //                for (int taskNumber = 0; taskNumber < 100; taskNumber++)
            //                {
            //                    //Random workload from 1 to 100 msec
            //                    int workload = rand.Next(0, 100);
            //                    totalMs += workload;
            //                    Console.WriteLine("Workload : {0}", workload);
            //                    sender.SendFrame(workload.ToString());
            //                }
            //                Console.WriteLine("Total expected cost : {0} msec", totalMs);
            //                Console.WriteLine("Press Enter to quit");
            //                Console.ReadLine();
            //            }

            //        });


            //        Task.Run(() =>
            //        {


            //        });

            //        Task.Run(() =>
            //        {

            //        using (var receiver = new PullSocket("@tcp://localhost:5558"))
            //        {
            //            //wait for start of batch (see Ventilator.csproj Program.cs)
            //            //var startOfBatchTrigger = receiver.ReceiveFrameString();
            //            //Console.WriteLine("Seen start of batch");

            //            //Start our clock now
            //            var watch = Stopwatch.StartNew();

            //                for (int taskNumber = 0; taskNumber < 100; taskNumber++)
            //                {
            //                    var workerDoneTrigger = receiver.ReceiveFrameString();
            //                    if (taskNumber % 10 == 0)
            //                    {
            //                        Console.Write(":");
            //                    }
            //                    else
            //                    {
            //                        Console.Write(".");
            //                    }
            //                }
            //        watch.Stop();
            //        //Calculate and report duration of batch
            //        Console.WriteLine();
            //        Console.WriteLine("Total elapsed time {0} msec", watch.ElapsedMilliseconds);
            //        Console.ReadLine();
            //    }


            //});



















            Task.Run(() =>
            {
                using (var responseSocket = new ResponseSocket("@tcp://*:5555"))
                {
                    while (true)
                    {
                        var message = responseSocket.ReceiveFrameString();
                        Console.WriteLine(message);
                        responseSocket.SendFrame("World");

                    }

                  
      
                };

            });

            Task.Run(() =>
            {

                    using (var requestSocket = new RequestSocket("tcp://localhost:5555"))
                {
          
                    requestSocket.SendFrame("Hello");
                    var message = requestSocket.ReceiveFrameString();
        
                    Console.WriteLine(message);
                }

            });


            Task.Run(async() =>
            {

                await Task.Delay(1000);

                using (var requestSocket = new RequestSocket("tcp://localhost:5555"))
                {

                    requestSocket.SendFrame("Hello");
                    var message = requestSocket.ReceiveFrameString();

                    Console.WriteLine(message);
                }

            });

            Console.Read();
        }
    }
}
