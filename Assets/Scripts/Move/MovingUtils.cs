using System.Runtime.CompilerServices;
using Reproduction;
using Unity.Mathematics;

namespace Move
{
    public static class MovingUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Moving Create(float minAcceleration, float maxAcceleration, ref Random random)
        {
            return new Moving {
                Acceleration = random.NextFloat(minAcceleration, maxAcceleration)
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Moving Mutate(in Moving source, float deviation, float minAcceleration, float maxAcceleration,
            ref Random random)
        {
            Moving result = source;
            result.Acceleration = MutationUtils.MutateFloat(result.Acceleration, minAcceleration, maxAcceleration, deviation, ref random);
            return result;
        }
    }
}