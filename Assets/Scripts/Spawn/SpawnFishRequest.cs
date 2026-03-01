using Brain;
using Diet;
using Life;
using Move;
using Sight;
using Unity.Entities;
using Unity.Mathematics;

namespace Spawn
{
    public struct SpawnFishRequest : IComponentData
    {
        public ushort Count;
        public float2 Position;
        public Thinking Thinking;
        public Seeing Seeing;
        public Moving Moving;
        public Nutritious Nutritious;
        public Lasting Lasting;
        public Synthesizing Synthesizing;
        public Biting Biting;
    }
}