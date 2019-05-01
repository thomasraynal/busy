using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Busy
{
    public class TransportMessage
    {
        public Guid Id { get; set; }

        public PeerId Sender { get; set; }

        public MessageTypeId MessageTypeId { get; set; }

        private byte[] ContentBytes
        {
            get => GetContentBytes();
            set => Content = new MemoryStream(value, 0, value.Length, false, true);
        }

        public Stream Content { get; set; }

        public List<PeerId> PersistentPeerIds { get; set; }

        public TransportMessage(MessageTypeId messageTypeId, Stream content, Peer sender)
            : this(messageTypeId, content, sender.Id, sender.EndPoint)
        {
        }

        public TransportMessage(MessageTypeId messageTypeId, Stream content, PeerId senderId, string senderEndPoint)
            : this(messageTypeId, content, senderId)
        {
        }

        public TransportMessage(MessageTypeId messageTypeId, Stream content, PeerId senderId)
        {
            Id = Guid.NewGuid();
            MessageTypeId = messageTypeId;
            Sender = senderId;
            Content = content;
        }

        internal TransportMessage()
        {
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
    }
}
