using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Busy
{
    public interface IBus : IDisposable
    {
        PeerId PeerId { get; }
        bool IsRunning { get; }
        String DirectoryEndpoint { get; }

        void Configure(PeerId peerId);

        void Publish(IEvent message);
        Task<CommandResult> Send(ICommand message);
        Task<CommandResult> Send(ICommand message, Peer peer);
        Task<IDisposable> SubscribeAsync(SubscriptionRequest request);

        void Start();
        void Stop();

    }
}
