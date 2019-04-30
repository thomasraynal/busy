using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Busy.Tests
{
    public class JsonSerializer : IMessageSerializer
    {
        public IMessage Deserialize(MessageTypeId messageTypeId, Stream stream)
        {

            var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);

            return JsonConvert.DeserializeObject(Encoding.UTF8.GetString(memoryStream.ToArray()), messageTypeId.GetMessageType()) as IMessage;
        }

        public Stream Serialize(IMessage message)
        {
            var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
            return new MemoryStream(bytes);
        }
    }
}
