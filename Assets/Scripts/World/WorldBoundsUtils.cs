using Unity.Mathematics;

namespace World
{
    public static class WorldBoundsUtils
    {
        public static int2 GetBotLeftCorner(int2 worldBounds) => new(-worldBounds.x / 2, -worldBounds.y / 2);
        
        public static int2 GetTopRightCorner(int2 worldBounds) => new(worldBounds.x / 2, worldBounds.y / 2);
        
        public static float3 GetRandomPosition(int2 worldBounds, Random random)
        {
            int2 botLeftCorner = GetBotLeftCorner(worldBounds);
            int2 topRightCorner = GetTopRightCorner(worldBounds);
            return new float3(random.NextFloat2(botLeftCorner, topRightCorner), 0);
        }
    }
}