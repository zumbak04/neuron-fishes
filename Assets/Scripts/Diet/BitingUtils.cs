using System.Runtime.CompilerServices;
using Reproduction;
using Unity.Mathematics;

namespace Diet
{
    public static class BitingUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Biting Mutate(in Biting source, float deviation, float minStrength, float maxStrength, ref Random random)
        {
            Biting result = source;
            result.Strength = MutationUtils.MutateFloat(result.Strength, minStrength, maxStrength, deviation, ref random);
            return result;
        }
    }
}