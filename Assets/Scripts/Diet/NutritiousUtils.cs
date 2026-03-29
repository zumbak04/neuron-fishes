using System.Runtime.CompilerServices;
using Reproduction;
using Unity.Mathematics;

namespace Diet
{
    public static class NutritiousUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Nutritious Create(float minNutrients, float maxNutrients, ref Random random)
        {
            float limit = random.NextFloat(minNutrients, maxNutrients);
            return new Nutritious {
                Current = CurrentFromLimit(limit),
                Limit = limit
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Nutritious Mutate(in Nutritious source, float deviation, float minLimit,
            float maxLimit, ref Random random)
        {
            Nutritious result = source;
            result.Limit = MutationUtils.MutateFloat(result.Limit, minLimit, maxLimit, deviation, ref random);
            result.Current = math.min(result.Current, result.Limit);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float CurrentFromLimit(float limit)
        {
            return limit / 2;
        }
    }
}