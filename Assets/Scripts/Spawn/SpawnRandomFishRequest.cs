using Unity.Entities;

namespace Spawn
{
    public struct SpawnRandomFishRequest : IComponentData
    {
        public ushort count;
    }
}