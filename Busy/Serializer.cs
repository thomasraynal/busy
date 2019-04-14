using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Busy
{
    internal static class Serializer
    {
        private static readonly ConcurrentDictionary<Type, bool> _hasParameterLessConstructorByType = new ConcurrentDictionary<Type, bool>();

        public static MemoryStream Serialize(object message)
        {
            var stream = new MemoryStream();
            Serialize(stream, message);
            return stream;
        }

        public static void Serialize(Stream stream, object message)
        {
            try
            {
              //  RuntimeTypeModel.Default.Serialize(stream, message);
            }
            catch (Exception ex)
            {
              //  throw new ProtocolBufferSerializationException(message, ex);
            }
        }

        public static object Deserialize(Type messageType, Stream stream)
        {
            //if (messageType == null)
            //    return null;

            //stream.Position = 0; // Reset position

            //object obj = null;
            //if (!HasParameterLessConstructor(messageType) && messageType != typeof(string))
            //    obj = FormatterServices.GetUninitializedObject(messageType);

            //return RuntimeTypeModel.Default.Deserialize(stream, obj, messageType);

            return null;
        }

        public static bool TryClone<T>(T message, out T clone)
            where T : class
        {
            //var messageType = message?.GetType();
            //if (messageType != null && RuntimeTypeModel.Default.CanSerialize(messageType))
            //{
            //    // Cannot use the DeepClone method as it doesn't handle classes without a parameterless constructor

            //    using (var ms = new MemoryStream())
            //    {
            //        Serialize(ms, message);
            //        ms.Position = 0;
            //        clone = (T)Deserialize(messageType, ms);
            //    }

            //    return true;
            //}

            //clone = null;
            //return false;

            clone = null;
            return false;
        }

        private static bool HasParameterLessConstructor(Type messageType)
            => _hasParameterLessConstructorByType.GetOrAdd(messageType, type => ComputeHasParameterLessConstructor(type));

        private static bool ComputeHasParameterLessConstructor(Type type)
            => type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null) != null;
    }
}
