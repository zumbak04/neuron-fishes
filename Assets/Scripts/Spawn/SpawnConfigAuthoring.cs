using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Spawn
{
    public class SpawnConfigAuthoring : MonoBehaviour
    {
        [field: SerializeField] public List<GameObject> BiterPrefabs = new();
        [field: SerializeField] public List<GameObject> PlantPrefabs = new();
        
        [field: SerializeField] public uint BiterSpawnWeight { get; private set; }
        [field: SerializeField] public uint PlantSpawnWeight { get; private set; }
        
        private class Baker : Baker<SpawnConfigAuthoring>
        {
            public override void Bake(SpawnConfigAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new SpawnConfig {
                    BiterSpawnWeight = authoring.BiterSpawnWeight,
                    PlantSpawnWeight = authoring.PlantSpawnWeight
                });
                
                var biterPrefabs = AddBuffer<FishBiterPrefabBufferElement>(entity);
                foreach (GameObject biterPrefab in authoring.BiterPrefabs) {
                    biterPrefabs.Add(new FishBiterPrefabBufferElement {
                        Value = GetEntity(biterPrefab, TransformUsageFlags.Dynamic)
                    });
                }
                
                var plantPrefabs = AddBuffer<FishPlantPrefabBufferElement>(entity);
                foreach (GameObject plantPrefab in authoring.PlantPrefabs) {
                    plantPrefabs.Add(new FishPlantPrefabBufferElement {
                        Value = GetEntity(plantPrefab, TransformUsageFlags.Dynamic)
                    });
                }
            }
        }
    }

    public struct SpawnConfig : IComponentData
    {
        public uint BiterSpawnWeight;
        public uint PlantSpawnWeight;
    }
}