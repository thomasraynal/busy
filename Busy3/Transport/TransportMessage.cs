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

        public byte[] Content { get; set; }

        public TransportMessage(MessageTypeId messageTypeId, byte[] content, Peer sender)
            : this(messageTypeId, content, sender.Id, sender.EndPoint)
        {
        }

        public TransportMessage(MessageTypeId messageTypeId, byte[] content, PeerId senderId, string senderEndPoint)
            : this(messageTypeId, content, senderId)
        {
        }

        public TransportMessage(MessageTypeId messageTypeId, byte[] content, PeerId senderId)
        {
            Id = Guid.NewGuid();
            Sender = senderId;
            MessageTypeId = messageTypeId;
            Sender = senderId;
            Content = content;
        }

        public TransportMessage()
        {
        }
    }
}
