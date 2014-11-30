using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using System.Reflection;

namespace UniMoveStation.Messages
{
    /// <summary>
    /// Helper functions for message serialization and deserialization.
    /// </summary>
    public static class Util
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

    /// <summary>
    /// A message containing a single string.
    /// </summary>
    [Serializable]
    public class StringMessage
    {
        /// <summary>
        /// The string.
        /// </summary>
        public string Message { get; set; }
    }

    /// <summary>
    /// A message with more information.
    /// </summary>
    [Serializable]
    public class ComplexMessage
    {
        /// <summary>
        /// The user-defined string.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The time this message was created.
        /// </summary>
        public DateTimeOffset Time { get; set; }

        /// <summary>
        /// The unique identifier for this message.
        /// </summary>
        public Guid UniqueID { get; set; }
    }

    [Serializable]
    public class Float3
    {
        public float x, y, z;

        public static implicit operator Float3(Vector3 vector3)
        {
            return new Float3(vector3);
        }

        public static implicit operator Vector3(Float3 float3)
        {
            return new Vector3(float3.x, float3.y, float3.z);
        }

        public static implicit operator String(Float3 float3)
        {
            return float3.ToString();
        }

        public override String ToString()
        {
            return String.Format("{{ {0:F}, {1:F}, {2:F} }}", x, y, z);
        }

        public Float3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Float3(Vector3 vector3)
        {
            this.x = vector3.x;
            this.y = vector3.y;
            this.z = vector3.z;
        }
        
    } //Float3

    [Serializable]
    public class PositionMessage
    {
        public Float3 Message { get; set; }

        public long StartTick { get; set; }

        public int TrackerIndex { get; set; }

        public int MoveIndex { get; set; }
    }
}  //namespace
