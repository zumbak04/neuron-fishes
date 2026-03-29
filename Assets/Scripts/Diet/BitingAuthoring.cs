using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Diet
{
    public class BitingAuthoring : MonoBehaviour
    {
        [field: SerializeField]
        public List<BitingRayAuthoring> Rays;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            foreach (BitingRayAuthoring ray in Rays) {
                Vector2 raycastEnd = ray.Start + ray.Direction;
                Gizmos.DrawSphere(ray.Start, 0.01f);
                Gizmos.DrawSphere(raycastEnd, 0.02f);
                Gizmos.DrawLine(ray.Start, raycastEnd);
            }
        }

        [Serializable]
        public class BitingRayAuthoring
        {
            [field: SerializeField]
            public Vector2 Start { get; private set; }
            [field: SerializeField]
            public Vector2 Direction { get; private set; }
        }

        private class Baker : Baker<BitingAuthoring>
        {
            public override void Bake(BitingAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                
                Biting biting = new() {
                    Rays = new FixedList64Bytes<BitingRay>() {
                        Capacity = authoring.Rays.Count
                    }
                };
                foreach (BitingRayAuthoring rayAuthoring in authoring.Rays) {
                    biting.Rays.Add(new BitingRay {
                       Start = rayAuthoring.Start,
                       End = rayAuthoring.Start + rayAuthoring.Direction
                    });
                }
                AddComponent(entity, biting);
                
                AddComponent<TookBiteEvent>(entity);
                SetComponentEnabled<TookBiteEvent>(entity, false);
                
                AddComponent<BitingCooldown>(entity);
                SetComponentEnabled<BitingCooldown>(entity, false);
            }
        }
    }
    
    public struct Biting : IComponentData
    {
        public float Strength;
        
        // 3 элемента, 16 байт на каждый BitingRay + 4 байта на header
        public FixedList64Bytes<BitingRay> Rays;
    }
    
    public struct BitingRay
    {
        public float2 Start;
        public float2 End;
    }

    public struct TookBiteEvent : IComponentData, IEnableableComponent
    {
        public float NutrientGain;
    }

    public struct BitingCooldown : IComponentData, IEnableableComponent
    {
        public float Value;
    }
}