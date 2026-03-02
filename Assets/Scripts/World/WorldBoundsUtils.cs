using Unity.Mathematics;

namespace World
{
    public static class WorldBoundsUtils
    {
        public static float2 GetBotLeftCorner(float2 worldBounds)
        {
            return new float2(-worldBounds.x / 2, -worldBounds.y / 2);
        }
        
        public static float2 GetBotRightCorner(float2 worldBounds)
        {
            return new float2(worldBounds.x / 2, -worldBounds.y / 2);
        }

        public static float2 GetTopRightCorner(float2 worldBounds)
        {
            return new float2(worldBounds.x / 2, worldBounds.y / 2);
        }
        
        public static float2 GetTopLeftCorner(float2 worldBounds)
        {
            return new float2(-worldBounds.x / 2, worldBounds.y / 2);
        }
    }
}