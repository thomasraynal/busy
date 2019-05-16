using StructureMap;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Busy
{
    public interface IBus : IDisposable
    {
        PeerId PeerId { get; }

        Peer Self { get; }

        IEnumerable<Subscription> AutoSubscribes { get; }

        IContainer Container { get; }

        String DirectoryEndpoint { get; }

        void Configure(PeerId peerId, String endpoint, string directoryEndpoint);

        void Publish(IEvent message);
        Task<ICommandResult> Send(ICommand message);
        Task<ICommandResult> Send(ICommand message, Peer peer);
        Task Subscribe(SubscriptionRequest request);

        IEnumerable<Subscription> GetSubscriptions();

        void Start();
        void Stop();

    }
}
