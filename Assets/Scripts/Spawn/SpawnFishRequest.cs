using Brain;
using Diet;
using Life;
using Move;
using Receptor;
using Unity.Entities;
using Unity.Mathematics;

namespace Spawn
{
    public struct SpawnFishRequest : IComponentData
    {
        public ushort count;
        public float2 position;
        public Thinking thinking;
        public Seeing seeing;
        public Moving moving;
        public Nutritious nutritious;
        public Lasting lasting;
        public Synthesizing synthesizing;
    }
}