using System;
using System.Collections.Generic;
using System.Text;

namespace Busy
{
    public class MessageTypeDescriptor
    {
        public MessageTypeDescriptor(string fullName, Type messageType, bool isPersistent, bool isInfrastructure)
        {
            FullName = fullName;
            MessageType = messageType;
            IsPersistent = isPersistent;
            IsInfrastructure = isInfrastructure;
        }

        public string FullName { get; }
        public Type MessageType { get; }
        public bool IsPersistent { get; }
        public bool IsInfrastructure { get; }

    }
}
