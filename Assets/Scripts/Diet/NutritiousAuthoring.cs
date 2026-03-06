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
                AddComponent<BittenEvent>(entity);
                SetComponentEnabled<BittenEvent>(entity, false);
            }
        }
    }

    public struct Nutritious : IComponentData
    {
        public float Current;
        public float Limit;
    }

    public struct BittenEvent : IComponentData, IEnableableComponent
    {
        public float NutrientLoss;
    }
}