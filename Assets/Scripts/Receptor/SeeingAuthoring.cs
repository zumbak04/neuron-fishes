using Brain;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Receptor
{
    public class SeeingAuthoring : MonoBehaviour
    {
        private class Baker : Baker<SeeingAuthoring>
        {
            public override void Bake(SeeingAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<Seeing>(entity);
                AddComponent(entity, new SeeingOutputEvent() {
                        outputs = new FixedList32Bytes<float2>() {
                                Length = ThinkingConsts.INPUT_SIZE
                        }
                });
                SetComponentEnabled<SeeingOutputEvent>(entity, false);
            }
        }
    }

    public struct Seeing : IComponentData
    {
        public float cooldown;
        public float range;
    }

    
    public struct SeeingOutputEvent : IComponentData, IEnableableComponent
    {
        // 7 элементов, 8 битов на каждый float2 + 4 бита на header
        public FixedList64Bytes<float2> outputs;
    }
}