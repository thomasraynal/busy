using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Busy
{
    public interface IMessageSerializer
    {
        Stream Serialize(IMessage message);
        IMessage Deserialize(MessageTypeId messageTypeId, Stream stream);
    }
}
