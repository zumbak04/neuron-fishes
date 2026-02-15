using Unity.Mathematics;
using UnityEngine;

namespace Math
{
    public static class Vector2IntExtensions
    {
        public static int2 ToInt2(this Vector2Int value) => new(value.x, value.y);
        public static float2 ToFloat2(this Vector2 value) => new(value.x, value.y);
        public static float2 ToFloat2(this Vector3 value) => new(value.x, value.y);
    }
}