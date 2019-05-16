using Busy;
using Busy3.Demo.Shared;
using System;
using System.Threading.Tasks;

namespace Busy3.Peer1
{

    class Program
    {
        private static IBus _bus;

        static void Main(string[] args)
        {

            _bus = BusFactory.Create("peer1", Constants.EndpointPeer1, Constants.EndpointDirectory);
            _bus.Start();

            var subscription1 = Subscription.Matching<DatabaseStatus>(ev=> ev.DatacenterName == Constants.London);
            var subscription2 = Subscription.Any<DoSomething>();

            _bus.Subscribe(new SubscriptionRequest(subscription1)).Wait();
            _bus.Subscribe(new SubscriptionRequest(subscription2)).Wait();

            Task.Delay(100).Wait();


            while (true)
            {
                _bus.Publish(DatabaseStatus.Create(Constants.Paris));
                Task.Delay(1000);
            }

        }
    }
}
