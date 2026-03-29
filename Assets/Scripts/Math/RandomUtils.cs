using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace Math
{
    public static class RandomUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int NextIntSign(ref Random random)
        {
            return random.NextBool() ? 1 : -1;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float NextFloatSign(ref Random random)
        {
            return random.NextBool() ? 1f : -1f;
        }
    }
}