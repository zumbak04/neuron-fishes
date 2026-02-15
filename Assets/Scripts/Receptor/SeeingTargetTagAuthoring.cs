using Unity.Entities;
using UnityEngine;

namespace Receptor
{
    public struct SeeingTargetTag : IComponentData
    {
    }

    public class SeeingTargetTagAuthoring : MonoBehaviour
    {
        private class Baker : Baker<SeeingTargetTagAuthoring>
        {
            public override void Bake(SeeingTargetTagAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<SeeingTargetTag>(entity);
            }
        }
    }
}


