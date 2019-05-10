using Busy.Tests.Infrastructure;
using NUnit.Framework;
using StructureMap;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Busy.Tests.Storage
{
    [TestFixture]
    public class TestsDirectoryServer
    {
        private Container _container;
        private IBus _bus;
        private IPeerDirectory _directoryServer;
        private IPeerRepository _directoryRepository;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _container = new Container(configuration => configuration.AddRegistry<BusRegistry>());

            _bus = BusFactory.Create("TestE2E", "tcp://localhost:8080", "tcp://localhost:8080", _container);

            _directoryRepository = _container.GetInstance<IPeerRepository>();
        }

        [SetUp]
        public void SetUp()
        {
            GlobalTestContext.Reset();
        }

        [Test]
        public async Task ShouldRegisterAndUnregisterPeer()
        {
            var subscription1 = Subscription.Matching<DatabaseStatus>(status => status.DatacenterName == "Paris" && status.Status == "Ko");
            var subscription2 = Subscription.Any<DoSomething>();

            var bytes = _container.GetInstance<IMessageSerializer>().Serialize(subscription1);
            var json = Encoding.UTF8.GetString(bytes);

            var obj = _container.GetInstance<IMessageSerializer>().Deserialize(bytes, typeof(Subscription));

            var command = new DoSomething();

            Assert.AreEqual(0, GlobalTestContext.Get());

            _bus.Start();

            await _bus.Subscribe(new SubscriptionRequest(subscription1));

            var peers = _directoryRepository.GetPeers(true);
            Assert.AreEqual(1, peers.Count());
            Assert.AreEqual(5, peers.First().Subscriptions.Count());

            await _bus.Subscribe(new SubscriptionRequest(subscription2));

            peers = _directoryRepository.GetPeers(true);
            Assert.AreEqual(1, peers.Count());
            Assert.AreEqual(6, peers.First().Subscriptions.Count());

            await _bus.Send(command);

            await Task.Delay(50);

            Assert.AreEqual(1, GlobalTestContext.Get());


        }
    }
}
