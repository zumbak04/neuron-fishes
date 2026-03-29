using Move;
using Reproduction;
using Unity.Entities;
using UnityEngine;

namespace Config
{
    // todo zumbak вынести в ScriptableObject
    public class MainConfigAuthoring : MonoBehaviour
    {
        [field: SerializeField]
        public ThinkingConfig Thinking { get; private set; }

        [field: SerializeField]
        public SeeingConfig Seeing { get; private set; }

        [field: SerializeField]
        public MovementConfig Movement { get; private set; }

        [field: SerializeField]
        public DietConfig Diet { get; private set; }

        [field: SerializeField]
        public LifetimeConfig Lifetime { get; private set; }
        
        [field: SerializeField]
        public ReproductionConfig Reproduction { get; private set; }

        private class Baker : Baker<MainConfigAuthoring>
        {
            public override void Bake(MainConfigAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new MainConfig {
                    Thinking = authoring.Thinking,
                    Seeing = authoring.Seeing,
                    Movement = authoring.Movement,
                    Diet = authoring.Diet,
                    Lifetime = authoring.Lifetime,
                    Reproduction = authoring.Reproduction
                });
            }
        }
    }

    public struct MainConfig : IComponentData
    {
        public ThinkingConfig Thinking;

        public SeeingConfig Seeing;

        public MovementConfig Movement;

        public DietConfig Diet;

        public LifetimeConfig Lifetime;

        public ReproductionConfig Reproduction;
    }
}