using System;
using System.Collections.Generic;
using System.Text;

namespace Busy
{
    public readonly struct MessageTypeId : IEquatable<MessageTypeId>
    {

        private readonly MessageTypeDescriptor _descriptor;


        public string FullName => _descriptor?.FullName;

        public Type GetMessageType() => _descriptor?.MessageType;

        public bool IsInfrastructure() => _descriptor?.IsInfrastructure ?? false;

        public bool IsPersistent() => _descriptor?.IsPersistent ?? true;

        public override string ToString()
        {
            var lastDotIndex = FullName.LastIndexOf('.');
            return lastDotIndex != -1 ? FullName.Substring(lastDotIndex + 1) : FullName;
        }

        public bool Equals(MessageTypeId other) => _descriptor == other._descriptor;
        public override bool Equals(object obj) => obj is MessageTypeId messageTypeId && Equals(messageTypeId);

        public override int GetHashCode() => _descriptor?.GetHashCode() ?? 0;

        public static bool operator ==(MessageTypeId left, MessageTypeId right) => left.Equals(right);
        public static bool operator !=(MessageTypeId left, MessageTypeId right) => !left.Equals(right);

    }
}
