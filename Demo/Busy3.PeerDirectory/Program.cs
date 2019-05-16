using Busy;
using Busy3.Demo.Shared;
using System;
using System.Threading.Tasks;

namespace Busy3.PeerDirectory
{
    class Program
    {
        private static IBus _bus;

        static void Main(string[] args)
        {
            _bus = BusFactory.Create("directory", Constants.EndpointDirectory, Constants.EndpointDirectory);

            _bus.Start();

            Console.Read();

            while (true)
            {
                _bus.Send(new DoSomething());
                Task.Delay(1500);
            }
        }
    }
}
