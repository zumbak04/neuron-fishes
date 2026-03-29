using Brain;
using Diet;
using Lifetime;
using Move;
using Reproduction;
using Sight;
using Unity.Entities;
using Unity.Mathematics;

namespace Spawn
{
    public struct SpawnFishBiterRequest : IComponentData
    {
        public ushort Count;
        public float2 Position;
        public Thinking Thinking;
        public Seeing Seeing;
        public Moving Moving;
        public Nutritious Nutritious;
        public Lasting Lasting;
        public Reproductive Reproductive;
        
        public Biting Biting;
    }
}