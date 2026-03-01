using Unity.Entities;
using UnityEngine;

namespace Move
{
    public class MovingAuthoring : MonoBehaviour
    {
        private class Baker : Baker<MovingAuthoring>
        {
            public override void Bake(MovingAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<Moving>(entity);
            }
        }
    }

    public struct Moving : IComponentData
    {
        public float Acceleration;
    }
}