using Unity.Mathematics;

namespace Math
{
    public struct Snorm8
    {
        private const float PRECISION = 127f;

        private readonly sbyte _data;

        public Snorm8(float value)
        {
            _data = (sbyte)(math.clamp(value, -1f, 1f) * PRECISION);
        }

        public static implicit operator float(Snorm8 s)
        {
            return s._data / PRECISION;
        }

        public static explicit operator Snorm8(float value)
        {
            return new Snorm8(value);
        }
    }
}