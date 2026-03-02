using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using VContainer;
using Random = Unity.Mathematics.Random;

namespace World
{
    public class WorldBoundsService
    {
        private readonly EndSimulationEntityCommandBufferSystem _ecbSystem;

        private EntityQuery _worldConfigQuery;
        private EntityQuery _horizontalBoundPrefabsQuery;
        private EntityQuery _verticalBoundPrefabsQuery;

        [Inject]
        public WorldBoundsService(EndSimulationEntityCommandBufferSystem ecbSystem)
        {
            _ecbSystem = ecbSystem;

            EntityManager em = Unity.Entities.World.DefaultGameObjectInjectionWorld.EntityManager;
            _worldConfigQuery = em.CreateEntityQuery(ComponentType.ReadOnly<WorldConfig>());
            _horizontalBoundPrefabsQuery =
                em.CreateEntityQuery(ComponentType.ReadOnly<HorizontalBoundPrefabBufferElement>());
            _verticalBoundPrefabsQuery =
                em.CreateEntityQuery(ComponentType.ReadOnly<VerticalBoundPrefabBufferElement>());
        }

        public void Create()
        {
            var worldConfig = _worldConfigQuery.GetSingleton<WorldConfig>();
            if (!worldConfig.ImpassibleBounds) {
                return;
            }

            DynamicBuffer<HorizontalBoundPrefabBufferElement> horizontalBoundPrefabs =
                _horizontalBoundPrefabsQuery.GetSingletonBuffer<HorizontalBoundPrefabBufferElement>();
            DynamicBuffer<VerticalBoundPrefabBufferElement> verticalBoundPrefabs =
                _verticalBoundPrefabsQuery.GetSingletonBuffer<VerticalBoundPrefabBufferElement>();

            EntityCommandBuffer ecb = _ecbSystem.CreateCommandBuffer();

            float2 topRightCorner = WorldBoundsUtils.GetTopRightCorner(worldConfig.Bounds);
            float2 topLeftCorner = WorldBoundsUtils.GetTopLeftCorner(worldConfig.Bounds);
            float2 botRightCorner = WorldBoundsUtils.GetBotRightCorner(worldConfig.Bounds);
            float2 botLeftCorner = WorldBoundsUtils.GetBotLeftCorner(worldConfig.Bounds);

            InstantiateOnPosition(worldConfig.NorthEastBoundCornerPrefab, topRightCorner, ref ecb);
            InstantiateOnPosition(worldConfig.NorthWestBoundCornerPrefab, topLeftCorner, ref ecb);
            InstantiateOnPosition(worldConfig.SouthEastBoundCornerPrefab, botRightCorner, ref ecb);
            InstantiateOnPosition(worldConfig.SouthWestBoundCornerPrefab, botLeftCorner, ref ecb);

            uint seed = (uint)DateTime.Now.Ticks;
            Random random = new(seed);

            InstantiateFromToHorizontalPosition(horizontalBoundPrefabs, topLeftCorner.x + worldConfig.BoundStep,
                topRightCorner.x - worldConfig.BoundStep, worldConfig.BoundStep, topLeftCorner.y, ref ecb,
                ref random);
            InstantiateFromToHorizontalPosition(horizontalBoundPrefabs, botLeftCorner.x + worldConfig.BoundStep,
                botRightCorner.x - worldConfig.BoundStep, worldConfig.BoundStep, botLeftCorner.y, ref ecb,
                ref random);
            InstantiateFromToVerticalPosition(verticalBoundPrefabs, botLeftCorner.y + worldConfig.BoundStep,
                topLeftCorner.y - worldConfig.BoundStep, worldConfig.BoundStep, botLeftCorner.x, ref ecb,
                ref random);
            InstantiateFromToVerticalPosition(verticalBoundPrefabs, botRightCorner.y + worldConfig.BoundStep,
                topRightCorner.y - worldConfig.BoundStep, worldConfig.BoundStep, botRightCorner.x, ref ecb,
                ref random);
        }

        private void InstantiateOnPosition(Entity prefab, float2 position, ref EntityCommandBuffer ecb)
        {
            Entity instance = ecb.Instantiate(prefab);
            ecb.SetComponent(instance, new LocalTransform {
                Position = new float3(position.xy, 0),
                Scale = 1f,
                Rotation = quaternion.identity
            });
        }

        private void InstantiateFromToHorizontalPosition(in DynamicBuffer<HorizontalBoundPrefabBufferElement> prefabs,
            float fromX, float toX,
            float step, float y, ref EntityCommandBuffer ecb, ref Random random)
        {
            int prefabCount = prefabs.Length;

            for (float i = fromX; i <= toX; i += step) {
                Entity prefab = prefabs[random.NextInt(0, prefabCount)].Value;
                InstantiateOnPosition(prefab, new float2(i, y), ref ecb);
            }
        }

        private void InstantiateFromToVerticalPosition(in DynamicBuffer<VerticalBoundPrefabBufferElement> prefabs,
            float fromY, float toY,
            float step, float x, ref EntityCommandBuffer ecb, ref Random random)
        {
            int prefabCount = prefabs.Length;

            for (float i = fromY; i <= toY; i += step) {
                Entity prefab = prefabs[random.NextInt(0, prefabCount)].Value;
                InstantiateOnPosition(prefab, new float2(x, i), ref ecb);
            }
        }
    }
}