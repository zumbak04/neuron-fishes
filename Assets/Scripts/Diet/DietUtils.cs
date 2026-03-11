using System.Runtime.CompilerServices;

namespace Diet
{
    public static class DietUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float CurNutrientsFromLimit(float limit)
        {
            return limit / 2;
        }
    }
}