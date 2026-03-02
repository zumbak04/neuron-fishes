using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace World
{
    public class WorldConfigAuthoring : MonoBehaviour
    {
        [field: SerializeField] public float2 Bounds { get; private set; }
        [field: SerializeField] public bool ImpassibleBounds { get; private set; }
        [field: SerializeField] public float BoundStep { get; private set; }

        [field: SerializeField, Header("Straight Prefabs")]
        public List<GameObject> HorizontalBoundPrefabs { get; private set; }
        [field: SerializeField] 
        public List<GameObject> VerticalBoundPrefabs { get; private set; }
        
        [field: SerializeField, Header("Corner Prefabs")]
        public GameObject NorthEastBoundCornerPrefab { get; private set; }
        [field: SerializeField] 
        public GameObject NorthWestBoundCornerPrefab { get; private set; }
        [field: SerializeField] 
        public GameObject SouthEastBoundCornerPrefab { get; private set; }
        [field: SerializeField] 
        public GameObject SouthWestBoundCornerPrefab { get; private set; }

        private class Baker : Baker<WorldConfigAuthoring>
        {
            public override void Bake(WorldConfigAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new WorldConfig {
                    Bounds = authoring.Bounds,
                    ImpassibleBounds = authoring.ImpassibleBounds,
                    BoundStep = authoring.BoundStep,
                    NorthEastBoundCornerPrefab =
                        GetEntity(authoring.NorthEastBoundCornerPrefab, TransformUsageFlags.Renderable),
                    NorthWestBoundCornerPrefab =
                        GetEntity(authoring.NorthWestBoundCornerPrefab, TransformUsageFlags.Renderable),
                    SouthEastBoundCornerPrefab =
                        GetEntity(authoring.SouthEastBoundCornerPrefab, TransformUsageFlags.Renderable),
                    SouthWestBoundCornerPrefab =
                        GetEntity(authoring.SouthWestBoundCornerPrefab, TransformUsageFlags.Renderable)
                });

                var horizontalBoundPrefabs = AddBuffer<HorizontalBoundPrefabBufferElement>(entity);
                foreach (GameObject horizontalBoundPrefab in authoring.HorizontalBoundPrefabs) {
                    horizontalBoundPrefabs.Add(new HorizontalBoundPrefabBufferElement() {
                        Value = GetEntity(horizontalBoundPrefab, TransformUsageFlags.Renderable)
                    });
                }
                
                var verticalBoundPrefabs = AddBuffer<VerticalBoundPrefabBufferElement>(entity);
                foreach (GameObject verticalBoundPrefab in authoring.VerticalBoundPrefabs) {
                    verticalBoundPrefabs.Add(new VerticalBoundPrefabBufferElement() {
                        Value = GetEntity(verticalBoundPrefab, TransformUsageFlags.Renderable)
                    });
                }
            }
        }
    }

    public struct WorldConfig : IComponentData
    {
        public float2 Bounds;
        public bool ImpassibleBounds;
        public float BoundStep;

        public Entity NorthEastBoundCornerPrefab;
        public Entity NorthWestBoundCornerPrefab;
        public Entity SouthEastBoundCornerPrefab;
        public Entity SouthWestBoundCornerPrefab;
        
        public float HalfBoundStep => BoundStep / 2f;
    }
}