using Brain;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Sight
{
    public class SeeingAuthoring : MonoBehaviour
    {
        private class Baker : Baker<SeeingAuthoring>
        {
            public override void Bake(SeeingAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<Seeing>(entity);
                AddComponent(entity, new SeenEvent {
                    ToTargets = new FixedList32Bytes<float2> {
                        Length = ThinkingConsts.INPUT_SIZE
                    }
                });
                SetComponentEnabled<SeenEvent>(entity, false);
            }
        }
    }

    public struct Seeing : IComponentData
    {
        public float Range;
    }

    public struct SeenEvent : IComponentData, IEnableableComponent
    {
        // 7 элементов, 8 байт на каждый float2 + 4 байта на header
        public FixedList64Bytes<float2> ToTargets;
    }
}