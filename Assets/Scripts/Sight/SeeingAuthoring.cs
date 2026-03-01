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
                AddComponent(entity, new SightOutputEvent {
                    Outputs = new FixedList32Bytes<float2> {
                        Length = ThinkingConsts.INPUT_SIZE
                    }
                });
                SetComponentEnabled<SightOutputEvent>(entity, false);
            }
        }
    }

    public struct Seeing : IComponentData
    {
        public float Cooldown;
        public float Range;
    }

    public struct SightOutputEvent : IComponentData, IEnableableComponent
    {
        // todo zumbak передавать нормализованный вектор. Это должно убрать кружение вокруг цели.
        // 7 элементов, 8 битов на каждый float2 + 4 бита на header
        public FixedList64Bytes<float2> Outputs;
    }
}