using Unity.Mathematics;

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
    }
}