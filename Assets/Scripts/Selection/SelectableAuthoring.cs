using Unity.Entities;
using UnityEngine;

namespace Selection
{
    public class SelectableAuthoring : MonoBehaviour
    {
        private class Baker : Baker<SelectableAuthoring>
        {
            public override void Bake(SelectableAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<SelectableTag>(entity);
                AddComponent<SelectedTag>(entity);
                SetComponentEnabled<SelectedTag>(entity, false);
            }
        }
    }

    public struct SelectableTag : IComponentData
    {
    }

    public struct SelectedTag : IComponentData, IEnableableComponent
    {
        
    }
}