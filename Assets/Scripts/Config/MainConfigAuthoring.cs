using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Config
{
    // todo zumbak вынести в ScriptableObject
    public class MainConfigAuthoring : MonoBehaviour
    {
        [field: SerializeField]
        public List<GameObject> FishPrefabs { get; private set; }

        [SerializeField]
        public WorldConfig _world;

        [SerializeField]
        public ThinkingConfig _thinking;
        
        [SerializeField]
        public SeeingConfig _seeing;

        [SerializeField]
        public MovementConfig _movement;

        [SerializeField]
        public DietConfig _diet;
        
        [SerializeField]
        public LifeConfig _life;

        private class Baker : Baker<MainConfigAuthoring>
        {
            public override void Bake(MainConfigAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new MainConfig() {
                        world = authoring._world,
                        thinking = authoring._thinking,
                        seeing = authoring._seeing,
                        movement = authoring._movement,
                        diet = authoring._diet,
                        life = authoring._life
                });

                DynamicBuffer<FishPrefabBufferElement> buffer = AddBuffer<FishPrefabBufferElement>(entity);
                foreach (GameObject fishPrefab in authoring.FishPrefabs) {
                    buffer.Add(new FishPrefabBufferElement() {
                            value = GetEntity(fishPrefab, TransformUsageFlags.Dynamic)
                    });
                }
            }
        }
    }

    public struct MainConfig : IComponentData
    {
        public WorldConfig world;

        public ThinkingConfig thinking;

        public SeeingConfig seeing;

        public MovementConfig movement;

        public DietConfig diet;

        public LifeConfig life;
    }
}