using Brain;
using Config;
using Diet;
using Lifetime;
using Move;
using Reproduction;
using Sight;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using World;
using Random = Unity.Mathematics.Random;

namespace Spawn
{
    [BurstCompile]
    public partial struct SpawnSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<MainConfig>();
            state.RequireForUpdate<WorldConfig>();
            state.RequireForUpdate<SpawnConfig>();

            NativeArray<EntityQuery> queries = new(3, Allocator.Temp);
            queries[0] = new EntityQueryBuilder(Allocator.Temp).WithAll<SpawnFishBiterRequest>().Build(ref state);
            queries[1] = new EntityQueryBuilder(Allocator.Temp).WithAll<SpawnFishPlantRequest>().Build(ref state);
            queries[2] = new EntityQueryBuilder(Allocator.Temp).WithAll<SpawnFishesRequest>().Build(ref state);

            state.RequireAnyForUpdate(queries);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var mainConfig = SystemAPI.GetSingleton<MainConfig>();
            var worldConfig = SystemAPI.GetSingleton<WorldConfig>();
            var spawnConfig = SystemAPI.GetSingleton<SpawnConfig>();

            var ecbSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            DynamicBuffer<FishBiterPrefabBufferElement> fishBiterPrefabs =
                SystemAPI.GetSingletonBuffer<FishBiterPrefabBufferElement>();
            DynamicBuffer<FishPlantPrefabBufferElement> fishPlantPrefabs =
                SystemAPI.GetSingletonBuffer<FishPlantPrefabBufferElement>();

            float2 botLeftCorner = WorldBoundsUtils.GetBotLeftCorner(worldConfig.Bounds) + worldConfig.HalfBoundStep;
            float2 topRightCorner = WorldBoundsUtils.GetTopRightCorner(worldConfig.Bounds) - worldConfig.HalfBoundStep;

            EntityCommandBuffer ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);
            uint seed = (uint)(SystemAPI.Time.ElapsedTime * 1000) + 1;
            Random random = new(seed);

            foreach (var (request, entity) in SystemAPI.Query<RefRO<SpawnFishesRequest>>().WithEntityAccess()) {
                for (var i = 0; i < request.ValueRO.Count; i++) {
                    uint fishTypeRoll = random.NextUInt(0, spawnConfig.BiterSpawnWeight + spawnConfig.PlantSpawnWeight);
                    if (fishTypeRoll < spawnConfig.BiterSpawnWeight) {
                        InstantiateRandomBiterFish(botLeftCorner, topRightCorner, in fishBiterPrefabs, in mainConfig,
                            ref random, ref ecb, ref state);
                    }
                    else {
                        InstantiateRandomPlantFish(botLeftCorner, topRightCorner, in fishPlantPrefabs, in mainConfig,
                            ref random, ref ecb);
                    }
                }

                ecb.DestroyEntity(entity);
            }

            int fishBiterPrefabsCount = fishBiterPrefabs.Length;

            foreach (var (request, entity) in SystemAPI.Query<RefRO<SpawnFishBiterRequest>>().WithEntityAccess()) {
                for (var i = 0; i < request.ValueRO.Count; i++) {
                    Entity prefab = fishBiterPrefabs[random.NextInt(0, fishBiterPrefabsCount)].Value;
                    Entity instance = ecb.Instantiate(prefab);

                    ecb.SetComponent(instance, new LocalTransform {
                        Position = new float3(request.ValueRO.Position, 0),
                        Scale = 1f,
                        Rotation = quaternion.identity
                    });

                    ecb.SetComponent(instance, request.ValueRO.Thinking);
                    ecb.SetComponent(instance, request.ValueRO.Seeing);
                    ecb.SetComponent(instance, request.ValueRO.Moving);
                    ecb.SetComponent(instance, request.ValueRO.Nutritious);
                    ecb.SetComponent(instance, request.ValueRO.Lasting);
                    ecb.SetComponent(instance, request.ValueRO.Reproductive);
                    ecb.SetComponent(instance, FamilyUtils.Create());

                    // Нужно задать Strength не трогая лучи
                    var biting = SystemAPI.GetComponent<Biting>(prefab);
                    biting.Strength = request.ValueRO.Biting.Strength;
                    ecb.SetComponent(instance, biting);
                    
                    // ecb.SetName(instance, $"Fish_Biter_{instance.Index}_{instance.Version}");
                }

                ecb.DestroyEntity(entity);
            }

            int fishPlantPrefabsCount = fishPlantPrefabs.Length;

            foreach (var (request, entity) in SystemAPI.Query<RefRO<SpawnFishPlantRequest>>().WithEntityAccess()) {
                for (var i = 0; i < request.ValueRO.Count; i++) {
                    Entity prefab = fishPlantPrefabs[random.NextInt(0, fishPlantPrefabsCount)].Value;
                    Entity instance = ecb.Instantiate(prefab);

                    ecb.SetComponent(instance, new LocalTransform {
                        Position = new float3(request.ValueRO.Position, 0),
                        Scale = 1f,
                        Rotation = quaternion.identity
                    });

                    ecb.SetComponent(instance, request.ValueRO.Thinking);
                    ecb.SetComponent(instance, request.ValueRO.Seeing);
                    ecb.SetComponent(instance, request.ValueRO.Moving);
                    ecb.SetComponent(instance, request.ValueRO.Nutritious);
                    ecb.SetComponent(instance, request.ValueRO.Lasting);
                    ecb.SetComponent(instance, request.ValueRO.Reproductive);
                    ecb.SetComponent(instance, FamilyUtils.Create());

                    ecb.SetComponent(instance, request.ValueRO.Synthesizing);
                    
                    // ecb.SetName(instance, $"Fish_Plant_{instance.Index}_{instance.Version}");
                }

                ecb.DestroyEntity(entity);
            }
        }

        private void InstantiateRandomBiterFish(float2 botLeftCorner, float2 topRightCorner,
            in DynamicBuffer<FishBiterPrefabBufferElement> prefabs,
            in MainConfig mainConfig, ref Random random, ref EntityCommandBuffer ecb, ref SystemState state)
        {
            int prefabCount = prefabs.Length;

            Entity prefab = prefabs[random.NextInt(0, prefabCount)].Value;
            Entity instance = ecb.Instantiate(prefab);

            SetCommonRandomizedComponents(instance, botLeftCorner, topRightCorner, in mainConfig, ref random, ref ecb);

            // Нужно рандомизировать Strength не трогая лучи
            var biting = SystemAPI.GetComponent<Biting>(prefab);
            biting.Strength =
                random.NextFloat(mainConfig.Diet.Biting.MinStrength, mainConfig.Diet.Biting.MaxStrength);
            ecb.SetComponent(instance, biting);
            
            // ecb.SetName(instance, $"Fish_Biter_{instance.Index}_{instance.Version}");
        }

        private void InstantiateRandomPlantFish(float2 botLeftCorner,
            float2 topRightCorner, in DynamicBuffer<FishPlantPrefabBufferElement> prefabs,
            in MainConfig mainConfig, ref Random random, ref EntityCommandBuffer ecb)
        {
            int prefabCount = prefabs.Length;

            Entity prefab = prefabs[random.NextInt(0, prefabCount)].Value;
            Entity instance = ecb.Instantiate(prefab);

            SetCommonRandomizedComponents(instance, botLeftCorner, topRightCorner, in mainConfig, ref random, ref ecb);

            ecb.SetComponent(instance,
                SynthesizingUtils.Create(mainConfig.Diet.Synthesizing.MinStrength,
                    mainConfig.Diet.Synthesizing.MaxStrength, ref random));
            // ecb.SetName(instance, $"Fish_Plant_{instance.Index}_{instance.Version}");
        }

        private void SetCommonRandomizedComponents(Entity instance, float2 botLeftCorner, float2 topRightCorner,
            in MainConfig mainConfig, ref Random random, ref EntityCommandBuffer ecb)
        {
            ecb.SetComponent(instance, CreateLocalTransformOnRandomPosition(ref random, botLeftCorner, topRightCorner));

            ecb.SetComponent(instance,
                ThinkingUtils.Create(mainConfig.Thinking.HiddenLayerSize, mainConfig.Thinking.HiddenLayersCount,
                    ref random));
            ecb.SetComponent(instance,
                SeeingUtils.Create(mainConfig.Seeing.MinRange, mainConfig.Seeing.MaxRange, ref random));
            ecb.SetComponent(instance,
                MovingUtils.Create(mainConfig.Movement.MinLinearAcceleration, mainConfig.Movement.MaxLinearAcceleration,
                    ref random));
            ecb.SetComponent(instance,
                NutritiousUtils.Create(mainConfig.Diet.MinNutrients, mainConfig.Diet.MaxNutrients, ref random));
            ecb.SetComponent(instance,
                LastingUtils.Create(mainConfig.Lifetime.MinLifetime, mainConfig.Lifetime.MaxLifetime, ref random));
            ecb.SetComponent(instance,
                ReproductiveUtils.Create(mainConfig.Reproduction.MinMutationChance,
                    mainConfig.Reproduction.MaxMutationChance, mainConfig.Reproduction.MinMutationDeviation,
                    mainConfig.Reproduction.MaxMutationDeviation, ref random));
            ecb.SetComponent(instance, FamilyUtils.Create());
        }

        private LocalTransform CreateLocalTransformOnRandomPosition(ref Random random, float2 botLeftCorner,
            float2 topRightCorner)
        {
            return new LocalTransform {
                Position = new float3(random.NextFloat2(botLeftCorner, topRightCorner), 0),
                Scale = 1f,
                Rotation = quaternion.identity
            };
        }
    }
}