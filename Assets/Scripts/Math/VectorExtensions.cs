using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace Math
{
    public static class VectorExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 ToInt2(this Vector2Int value)
        {
            return new int2(value.x, value.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 ToFloat2(this Vector2 value)
        {
            return new float2(value.x, value.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 ToFloat2(this Vector3 value)
        {
            return new float2(value.x, value.y);
        }
    }
}