using Unity.Mathematics;
using UnityEngine;

namespace Math
{
    public static class MathUtils
    {
        public static float2 Clamp(float2 value, float lenght)
        {
            float lenghtSq = math.lengthsq(lenght);
            float inversedLenght = math.rsqrt(lenghtSq);
            float factor = math.min(1, inversedLenght);
            return value * factor;
        }

        public static Vector3 Clamp(Vector3 value, Vector3 minValue, Vector3 maxValue)
        {
            return new Vector3(Mathf.Clamp(value.x, minValue.x, maxValue.x),
                Mathf.Clamp(value.y, minValue.y, maxValue.y), Mathf.Clamp(value.z, minValue.z, maxValue.z));
        }
        
        public static Vector3 Clamp(Vector3 value, Vector2 minValue, Vector2 maxValue)
        {
            return new Vector3(Mathf.Clamp(value.x, minValue.x, maxValue.x), Mathf.Clamp(value.y, minValue.y, maxValue.y), value.z);
        }

        public static Vector3 Clamp(Vector3 value, float3 minValue, float3 maxValue)
        {
            return new Vector3(Mathf.Clamp(value.x, minValue.x, maxValue.x),
                Mathf.Clamp(value.y, minValue.y, maxValue.y), Mathf.Clamp(value.z, minValue.z, maxValue.z));
        }

        public static Vector3 Clamp(Vector3 value, float2 minValue, float2 maxValue)
        {
            return new Vector3(Mathf.Clamp(value.x, minValue.x, maxValue.x), Mathf.Clamp(value.y, minValue.y, maxValue.y), value.z);
        }
    }
}