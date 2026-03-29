using System.Runtime.CompilerServices;
using Math;
using Reproduction;
using Unity.Mathematics;

namespace Diet
{
    public static class SynthesizingUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Synthesizing Create(float minStrength, float maxStrength, ref Random random)
        {
            return new Synthesizing {
                Strength = random.NextFloat(minStrength, maxStrength)
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Synthesizing Mutate(in Synthesizing source, float deviation, float minStrength,
            float maxStrength, ref Random random)
        {
            Synthesizing result = source;
            result.Strength =
                MutationUtils.MutateFloat(result.Strength, minStrength, maxStrength, deviation, ref random);
            return result;
        }
    }
}