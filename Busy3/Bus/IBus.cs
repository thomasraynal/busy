using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Busy
{
    public interface IBus : IDisposable
    {
        PeerId PeerId { get; }
        String DirectoryEndpoint { get; }

        void Configure(PeerId peerId, String endpoint, string directoryEndpoint);

        void Publish(IEvent message);
        Task<ICommandResult> Send(ICommand message);
        Task<ICommandResult> Send(ICommand message, Peer peer);
        Task Subscribe(SubscriptionRequest request);

        void Start();
        void Stop();

    }
}
