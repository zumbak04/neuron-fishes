using Unity.Entities;
using UnityEngine;

namespace Diet
{
    public class BitingAuthoring : MonoBehaviour
    {
        private class Baker : Baker<BitingAuthoring>
        {
            public override void Bake(BitingAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<Biting>(entity);
            }
        }
    }
    
    public struct Biting : IComponentData
    {
        public float strength;
    }
}