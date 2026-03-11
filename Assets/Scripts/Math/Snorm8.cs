using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace Math
{
    public struct Snorm8
    {
        private const float PRECISION = 127f;
        private const float INV_PRECISION = 1 / 127f;

        private readonly sbyte _data;

        public Snorm8(float value)
        {
            _data = (sbyte)(math.clamp(value, -1f, 1f) * PRECISION);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator float(Snorm8 s)
        {
            return s._data * INV_PRECISION;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Snorm8(float s)
        {
            return new Snorm8(s);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float operator *(Snorm8 s, float f)
        {
            return s._data * f * INV_PRECISION;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float operator *(float f, Snorm8 s)
        {
            return s._data * f * INV_PRECISION;
        }
    }
}