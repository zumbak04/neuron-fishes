using System.Runtime.CompilerServices;
using Reproduction;
using Unity.Mathematics;

namespace Lifetime
{
    public static class LastingUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Lasting Create(float minLifetime, float maxLifetime, ref Random random)
        {
            return new Lasting {
                Lifetime = random.NextFloat(minLifetime, maxLifetime)
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Lasting Mutate(in Lasting source, float deviation, float minLifetime, float maxLifetime,
            ref Random random)
        {
            Lasting result = source;
            result.Lifetime =
                MutationUtils.MutateFloat(result.Lifetime, minLifetime, maxLifetime, deviation, ref random);
            return result;
        }
    }
}