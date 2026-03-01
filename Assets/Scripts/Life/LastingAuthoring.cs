using Unity.Entities;
using UnityEngine;

namespace Life
{
    public class LastingAuthoring : MonoBehaviour
    {
        private class Baker : Baker<LastingAuthoring>
        {
            public override void Bake(LastingAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<Lasting>(entity);
            }
        }
    }

    public struct Lasting : IComponentData
    {
        public float Lifetime;
    }
}