using System.Runtime.CompilerServices;
using Reproduction;
using Unity.Mathematics;

namespace Sight
{
    public static class SeeingUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Seeing Create(float minRange, float maxRange, ref Random random)
        {
            return new Seeing {
                Range = random.NextFloat(minRange, maxRange)
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Seeing Mutate(in Seeing source, float deviation, float minSeeingRange, float maxSeeingRange,
            ref Random random)
        {
            Seeing result = source;
            result.Range =
                MutationUtils.MutateFloat(result.Range, minSeeingRange, maxSeeingRange, deviation, ref random);
            return result;
        }
    }
}