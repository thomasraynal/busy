using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Busy
{
    public class TransportMessage
    {
        public MessageId Id { get; set; }

        public MessageTypeId MessageTypeId { get; set; }

        private byte[] ContentBytes
        {
            get => GetContentBytes();
            set => Content = new MemoryStream(value, 0, value.Length, false, true);
        }

        [JsonIgnore]
        public Stream Content { get; set; }

        public OriginatorInfo Originator { get; set; }

        public string Environment { get; set; }

        public bool? WasPersisted { get; set; }

        public List<PeerId> PersistentPeerIds { get; set; }

        [JsonIgnore]
        public bool IsPersistTransportMessage => PersistentPeerIds != null && PersistentPeerIds.Count != 0;

        public TransportMessage(MessageTypeId messageTypeId, Stream content, Peer sender)
            : this(messageTypeId, content, sender.Id, sender.EndPoint)
        {
        }

        public TransportMessage(MessageTypeId messageTypeId, Stream content, PeerId senderId, string senderEndPoint)
            : this(messageTypeId, content, CreateOriginator(senderId, senderEndPoint))
        {
        }

        public TransportMessage(MessageTypeId messageTypeId, Stream content, OriginatorInfo originator)
        {
            Id = MessageId.NextId();
            MessageTypeId = messageTypeId;
            Content = content;
            Originator = originator;
        }

        internal TransportMessage()
        {
        }

        private static OriginatorInfo CreateOriginator(PeerId peerId, string peerEndPoint)
        {
            return new OriginatorInfo(peerId, peerEndPoint, MessageContext.CurrentMachineName, MessageContext.GetInitiatorUserName());
        }

        public byte[] GetContentBytes()
        {
            if (Content == null)
                return Array.Empty<byte>();

            var position = Content.Position;
            var buffer = new byte[Content.Length];
            Content.Position = 0;
            Content.Read(buffer, 0, buffer.Length);
            Content.Position = position;

            return buffer;
        }

        internal TransportMessage ToPersistTransportMessage(List<PeerId> peerIds) => CloneWithPeerIds(peerIds);
        internal TransportMessage UnpackPersistTransportMessage() => CloneWithPeerIds(null);

        private TransportMessage CloneWithPeerIds(List<PeerId> peerIds) => new TransportMessage
        {
            Id = Id,
            MessageTypeId = MessageTypeId,
            Content = Content,
            Originator = Originator,
            Environment = Environment,
            WasPersisted = WasPersisted,
            PersistentPeerIds = peerIds,
        };

        public override string ToString()
        {
            return $"{MessageTypeId.FullName} - {Originator.SenderId} - {Id} ";
        }
    }
}
