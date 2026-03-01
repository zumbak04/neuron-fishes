using Unity.Entities;
using UnityEngine;

namespace Diet
{
    public class SynthesizingAuthoring : MonoBehaviour
    {
        private class Baker : Baker<SynthesizingAuthoring>
        {
            public override void Bake(SynthesizingAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<Synthesizing>(entity);
            }
        }
    }

    public struct Synthesizing : IComponentData
    {
        public float Strength;
    }
}