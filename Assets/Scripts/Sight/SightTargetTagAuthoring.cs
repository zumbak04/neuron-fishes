using Unity.Entities;
using UnityEngine;

namespace Sight
{
    public struct SightTargetTag : IComponentData
    {
    }

    public class SightTargetTagAuthoring : MonoBehaviour
    {
        private class Baker : Baker<SightTargetTagAuthoring>
        {
            public override void Bake(SightTargetTagAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<SightTargetTag>(entity);
            }
        }
    }
}