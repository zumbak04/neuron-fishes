using Unity.Entities;
using UnityEngine;

namespace Diet
{
    public class NutritiousAuthoring : MonoBehaviour
    {
        private class Baker : Baker<NutritiousAuthoring>
        {
            public override void Bake(NutritiousAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<Nutritious>(entity);
            }
        }
    }

    public struct Nutritious : IComponentData
    {
        public float Current;
        public float Limit;
    }
}