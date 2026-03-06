using Unity.Entities;

namespace Spawn
{
    public struct SpawnFishesRequest : IComponentData
    {
        public ushort Count;
    }
}