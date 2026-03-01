using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Diet
{
    public class BitingAuthoring : MonoBehaviour
    {
        [field: SerializeField]
        public Vector2 RaycastStart;
        [field: SerializeField]
        public Vector2 RaycastDirection;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Vector2 raycastEnd = RaycastStart + RaycastDirection;
            Gizmos.DrawSphere(RaycastStart, 0.01f);
            Gizmos.DrawSphere(raycastEnd, 0.02f);
            Gizmos.DrawLine(RaycastStart, raycastEnd);
        }

        private class Baker : Baker<BitingAuthoring>
        {
            public override void Bake(BitingAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Biting {
                    RaycastStart = authoring.RaycastStart,
                    RaycastEnd = authoring.RaycastStart + authoring.RaycastDirection
                });
            }
        }
    }
    
    public struct Biting : IComponentData
    {
        // todo zumbak добавить несколько лучей
        public float2 RaycastStart;
        public float2 RaycastEnd;
        public float Strength;
    }
}