using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

namespace UniMoveStation.Common.Utils
{
    /// <summary>
    /// Helper functions for message serialization and deserialization.
    /// </summary>
    public class SerializationHelper
    {
        /// <summary>
        /// Serializes an object to a binary representation, returned as a byte array.
        /// </summary>
        /// <param name="Object">The object to serialize.</param>
        public static byte[] Serialize(object Object)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;
                bf.Serialize(stream, Object);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Deserializes an object from a binary representation.
        /// </summary>
        /// <param name="binaryObject">The byte array to deserialize.</param>
        public static object Deserialize(byte[] binaryObject)
        {
            using (MemoryStream stream = new MemoryStream(binaryObject))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;
                bf.Binder = new AllowAllAssemblyVersionsDeserializationBinder();
                return bf.Deserialize(stream);
            }
        }

        sealed class AllowAllAssemblyVersionsDeserializationBinder : System.Runtime.Serialization.SerializationBinder
        {
            public override Type BindToType(string assemblyName, string typeName)
            {
                Type typeToDeserialize = null;

                String currentAssembly = Assembly.GetExecutingAssembly().FullName;

                // In this case we are always using the current assembly
                assemblyName = currentAssembly;

                // Get the type using the typeName and assemblyName
                typeToDeserialize = Type.GetType(String.Format("{0}, {1}",
                    typeName, assemblyName));

                return typeToDeserialize;
            }
        }
    }
}
