using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Busy.Tests
{
    public class JsonMessageSerializer : IMessageSerializer
    {
        public T Deserialize<T>(byte[] stream)
        {
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(stream));
        }

        public object Deserialize(byte[] stream, Type type)
        {
            return JsonConvert.DeserializeObject(Encoding.UTF8.GetString(stream), type);
        }

        public byte[] Serialize(object message)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
        }
    }
}
