using Unity.Entities;
using Unity.Mathematics;

namespace Selection
{
    public struct SelectRequest : IComponentData
    {
        public float3 worldPoint;
    }
}