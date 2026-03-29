using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace Math
{
    public struct Snorm8
    {
        private const float FLOAT_SCALE = 127f;
        private const float FLOAT_INV_SCALE = 1 / 127f;
        
        private const float FLOAT_MAX_VALUE = 1f;
        private const float FLOAT_MIN_VALUE = -1f;
        
        private const int INT_MAX_VALUE = 127;
        private const int INT_MIN_VALUE = -127;

        private readonly sbyte _data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Snorm8(float value)
        {
            _data = (sbyte)(math.clamp(value, FLOAT_MIN_VALUE, FLOAT_MAX_VALUE) * FLOAT_SCALE);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Snorm8(int value)
        {
            _data = (sbyte)math.clamp(value, INT_MIN_VALUE, INT_MAX_VALUE);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator float(Snorm8 value)
        {
            return value._data * FLOAT_INV_SCALE;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Snorm8(float value)
        {
            return new Snorm8(value);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Snorm8 operator +(Snorm8 a, Snorm8 b)
        {
            int result = a._data + b._data;
            return new Snorm8(result);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Snorm8 operator -(Snorm8 a, Snorm8 b)
        {
            int result = a._data - b._data;
            return new Snorm8(result);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Snorm8 operator -(Snorm8 value)
        {
            return new Snorm8(-value._data);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float operator *(Snorm8 s, float f)
        {
            return s._data * f * FLOAT_INV_SCALE;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float operator *(float f, Snorm8 s)
        {
            return s._data * f * FLOAT_INV_SCALE;
        }
    }
}