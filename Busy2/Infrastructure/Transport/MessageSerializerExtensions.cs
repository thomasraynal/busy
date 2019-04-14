using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Busy.Infrastructure
{
    public static class MessageSerializerExtensions
    {
        public static TransportMessage ToTransportMessage(this IMessageSerializer serializer, IMessage message, PeerId peerId, string peerEndPoint)
        {
            return new TransportMessage(message.TypeId(), serializer.Serialize(message), peerId, peerEndPoint);
        }

        public static IMessage ToMessage(this IMessageSerializer serializer, TransportMessage transportMessage)
        {
            return ToMessage(serializer, transportMessage, transportMessage.MessageTypeId, transportMessage.Content);
        }

        public static IMessage ToMessage(this IMessageSerializer serializer, TransportMessage transportMessage, MessageTypeId messageTypeId, Stream content)
        {
            return serializer.Deserialize(messageTypeId, content);
        }

    }
}
