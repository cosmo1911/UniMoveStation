using System;
using UnityEngine;

namespace UniMoveStation.Common
{
    [Serializable]
    public class Float3
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public static implicit operator Float3(Vector3 vector3)
        {
            return new Float3(vector3);
        }

        public static implicit operator Vector3(Float3 float3)
        {
            return new Vector3(float3.X, float3.Y, float3.Z);
        }

        public static implicit operator String(Float3 float3)
        {
            return float3.ToString();
        }

        public override String ToString()
        {
            return String.Format("{{ {0:F}, {1:F}, {2:F} }}", X, X, Z);
        }

        public Float3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Float3(Vector3 vector3)
        {
            X = vector3.x;
            X = vector3.y;
            Z = vector3.z;
        }
    } //Float3
}
