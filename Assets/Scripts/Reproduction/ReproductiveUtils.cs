using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace Reproduction
{
    public static class ReproductiveUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Reproductive Create(float minMutationChance, float maxMutationChance, float minMutationDeviation,
            float maxMutationDeviation, ref Random random)
        {
            return new Reproductive {
                MutationChance = random.NextFloat(minMutationChance, maxMutationChance),
                MutationDeviation = random.NextFloat(minMutationDeviation, maxMutationDeviation)
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Reproductive Mutate(in Reproductive source, float minMutationChance, float maxMutationChance,
            float minMutationDeviation, float maxMutationDeviation, ref Random random)
        {
            Reproductive result = source;
            result.MutationChance = MutationUtils.MutateFloat(result.MutationChance, minMutationChance,
                maxMutationChance, result.MutationDeviation, ref random);
            result.MutationDeviation = MutationUtils.MutateFloat(result.MutationDeviation, minMutationDeviation, maxMutationDeviation,
                result.MutationDeviation, ref random);
            return result;
        }
    }
}