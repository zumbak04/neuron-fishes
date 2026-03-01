using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;

namespace Config
{
    // todo zumbak вынести в ScriptableObject
    public class MainConfigAuthoring : MonoBehaviour
    {
        [field: SerializeField] 
        public List<GameObject> FishPrefabs { get; private set; }

        [field: SerializeField]
        public WorldConfig World { get; private set; }

        [field: SerializeField]
        public ThinkingConfig Thinking { get; private set; }

        [field: SerializeField]
        public SeeingConfig Seeing { get; private set; }

        [field: SerializeField]
        public MovementConfig Movement { get; private set; }

        [field: SerializeField]
        public DietConfig Diet { get; private set; }

        [field: SerializeField]
        public LifeConfig Life { get; private set; }

        private class Baker : Baker<MainConfigAuthoring>
        {
            public override void Bake(MainConfigAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new MainConfig {
                    World = authoring.World,
                    Thinking = authoring.Thinking,
                    Seeing = authoring.Seeing,
                    Movement = authoring.Movement,
                    Diet = authoring.Diet,
                    Life = authoring.Life
                });

                DynamicBuffer<FishPrefabBufferElement> buffer = AddBuffer<FishPrefabBufferElement>(entity);
                foreach (GameObject fishPrefab in authoring.FishPrefabs) {
                    buffer.Add(new FishPrefabBufferElement {
                        Value = GetEntity(fishPrefab, TransformUsageFlags.Dynamic)
                    });
                }
            }
        }
    }

    public struct MainConfig : IComponentData
    {
        public WorldConfig World;

        public ThinkingConfig Thinking;

        public SeeingConfig Seeing;

        public MovementConfig Movement;

        public DietConfig Diet;

        public LifeConfig Life;
    }
}