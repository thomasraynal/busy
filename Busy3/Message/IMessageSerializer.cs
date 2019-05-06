using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Busy
{
    public interface IMessageSerializer
    {
        byte[] Serialize(object message);
        T Deserialize<T>(byte[] stream);
        object Deserialize(byte[] stream, Type type);
    }
}
