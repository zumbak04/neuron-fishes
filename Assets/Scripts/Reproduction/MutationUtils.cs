using System.Runtime.CompilerServices;
using Math;
using Unity.Mathematics;

namespace Reproduction
{
    public static class MutationUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float MutateFloat(float value, float min, float max, float deviation, ref Random random)
        {
            float delta = (max - min) * deviation * RandomUtils.NextFloatSign(ref random);
            return math.clamp(value + delta, min, max);
        }
    }
}