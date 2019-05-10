using System;
using System.Collections.Generic;
using System.Text;

namespace Busy
{
    //refacto - review equalities
    public class MessageTypeId : IEquatable<MessageTypeId>
    {

        private readonly MessageTypeDescriptor _descriptor;

        public MessageTypeId(Type messageType)
        {
            _descriptor = MessageUtil.GetMessageTypeDescriptor(messageType);
            FullName = _descriptor.FullName;
            QualifiedName = GetFullnameWithNoAssemblyOrVersion(messageType);
        }

        public MessageTypeId(string fullName)
        {
            _descriptor = MessageUtil.GetMessageTypeDescriptor(fullName);
            FullName = QualifiedName = fullName;
        }

        public MessageTypeId()
        {
        }

        public string FullName { get; set; }

        public string QualifiedName { get; set; }

        public Type GetMessageType() => _descriptor?.MessageType;

        public bool IsInfrastructure() => _descriptor?.IsInfrastructure ?? false;

        public bool IsPersistent() => _descriptor?.IsPersistent ?? true;

        public override string ToString()
        {
            var lastDotIndex = QualifiedName.LastIndexOf('.');
            return lastDotIndex != -1 ? QualifiedName.Substring(lastDotIndex + 1) : QualifiedName;
        }

        public bool Equals(MessageTypeId other) => FullName == other.FullName;
        public override bool Equals(object obj) => obj is MessageTypeId messageTypeId && Equals(messageTypeId);

        public override int GetHashCode() => FullName.GetHashCode();

        public static bool operator ==(MessageTypeId left, MessageTypeId right) => left.Equals(right);
        public static bool operator !=(MessageTypeId left, MessageTypeId right) => !left.Equals(right);


        private string GetFullnameWithNoAssemblyOrVersion(Type messageType)
        {
            if (!messageType.IsGenericType)
                return messageType.FullName;

            var genericTypeDefinition = messageType.GetGenericTypeDefinition();
            var builder = new StringBuilder();
            if (messageType.IsNested)
                builder.AppendFormat("{0}+", messageType.DeclaringType.FullName);
            else
                builder.AppendFormat("{0}.", genericTypeDefinition.Namespace);

            var backQuoteIndex = genericTypeDefinition.Name.IndexOf('`');
            builder.Append(genericTypeDefinition.Name.Substring(0, backQuoteIndex));
            builder.Append("<");
            foreach (var genericArgument in messageType.GetGenericArguments())
            {
                if (genericArgument.IsGenericType)
                    throw new InvalidOperationException("Nested generics are not supported");
                builder.AppendFormat("{0}.{1}, ", genericArgument.Namespace, genericArgument.Name);
            }

            builder.Length -= 2;
            builder.Append(">");
            return builder.ToString();
        }
    }
}
