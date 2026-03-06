using Brain;
using Config;
using Diet;
using Life;
using Math;
using Move;
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
                        InstantiateRandomBiterFish(in fishBiterPrefabs, in mainConfig, ref random, ref ecb, ref state, botLeftCorner, topRightCorner);
                    }
                    else {
                        InstantiateRandomPlantFish(in fishPlantPrefabs, in mainConfig, ref random, ref ecb, ref state, botLeftCorner, topRightCorner);
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

                    // Нужно задать Strength не трогая лучи
                    var biting = SystemAPI.GetComponent<Biting>(prefab);
                    biting.Strength = request.ValueRO.Biting.Strength;
                    ecb.SetComponent(instance, biting);
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

                    ecb.SetComponent(instance, request.ValueRO.Synthesizing);
                }

                ecb.DestroyEntity(entity);
            }
        }
        
        private void InstantiateRandomBiterFish(in DynamicBuffer<FishBiterPrefabBufferElement> prefabs,
            in MainConfig mainConfig, ref Random random, ref EntityCommandBuffer ecb, ref SystemState state, float2 botLeftCorner,
            float2 topRightCorner)
        {
            int prefabCount = prefabs.Length;

            Entity prefab = prefabs[random.NextInt(0, prefabCount)].Value;
            Entity instance = ecb.Instantiate(prefab);

            ecb.SetComponent(instance, CreateLocalTransformOnRandomPosition(ref random, botLeftCorner, topRightCorner));

            ecb.SetComponent(instance,
                CreateBrain(mainConfig.Thinking.HiddenLayerSize, mainConfig.Thinking.HiddenLayersCount,
                    ref random));
            ecb.SetComponent(instance,
                CreateSeeing(mainConfig.Seeing.MinRange, mainConfig.Seeing.MaxRange, ref random));
            ecb.SetComponent(instance,
                CreateMoving(mainConfig.Movement.MinAcceleration, mainConfig.Movement.MaxAcceleration,
                    ref random));
            ecb.SetComponent(instance,
                CreateNutritious(mainConfig.Diet.MinNutrients, mainConfig.Diet.MaxNutrients, ref random));
            ecb.SetComponent(instance,
                CreateLasting(mainConfig.Life.MinLifetime, mainConfig.Life.MaxLifetime, ref random));

            // Нужно рандомизировать Strength не трогая лучи
            var biting = SystemAPI.GetComponent<Biting>(prefab);
            biting.Strength =
                random.NextFloat(mainConfig.Diet.Biting.MinStrength, mainConfig.Diet.Biting.MaxStrength);
            ecb.SetComponent(instance, biting);
        }

        private void InstantiateRandomPlantFish(in DynamicBuffer<FishPlantPrefabBufferElement> prefabs,
            in MainConfig mainConfig, ref Random random, ref EntityCommandBuffer ecb, ref SystemState state, float2 botLeftCorner,
            float2 topRightCorner)
        {
            int prefabCount = prefabs.Length;

            Entity prefab = prefabs[random.NextInt(0, prefabCount)].Value;
            Entity instance = ecb.Instantiate(prefab);

            ecb.SetComponent(instance, CreateLocalTransformOnRandomPosition(ref random, botLeftCorner, topRightCorner));

            ecb.SetComponent(instance,
                CreateBrain(mainConfig.Thinking.HiddenLayerSize, mainConfig.Thinking.HiddenLayersCount,
                    ref random));
            ecb.SetComponent(instance,
                CreateSeeing(mainConfig.Seeing.MinRange, mainConfig.Seeing.MaxRange, ref random));
            ecb.SetComponent(instance,
                CreateMoving(mainConfig.Movement.MinAcceleration, mainConfig.Movement.MaxAcceleration,
                    ref random));
            ecb.SetComponent(instance,
                CreateNutritious(mainConfig.Diet.MinNutrients, mainConfig.Diet.MaxNutrients, ref random));
            ecb.SetComponent(instance,
                CreateLasting(mainConfig.Life.MinLifetime, mainConfig.Life.MaxLifetime, ref random));

            ecb.SetComponent(instance,
                CreateSynthesizing(mainConfig.Diet.Synthesizing.MinStrength,
                    mainConfig.Diet.Synthesizing.MaxStrength,
                    ref random));
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

        private Thinking CreateBrain(ushort hiddenLayersSize, ushort hiddenLayersCount, ref Random random)
        {
            FixedList32Bytes<ushort> layerSizes = new() {
                Length = 2 + hiddenLayersCount,
                [0] = ThinkingConsts.INPUT_SIZE
            };
            layerSizes[^1] = ThinkingConsts.OUTPUT_SIZE;

            for (ushort i = 0; i < hiddenLayersCount; i++) {
                layerSizes[i + 1] = hiddenLayersSize;
            }

            Thinking thinking = new(layerSizes);

            for (var i = 0; i < thinking.Weights.Length; i++) {
                thinking.Weights[i] =
                    (Snorm8)random.NextFloat(ThinkingConsts.MIN_NODE_WEIGHT, ThinkingConsts.MAX_NODE_WEIGHT);
            }

            return thinking;
        }

        private Seeing CreateSeeing(float minRange, float maxRange, ref Random random)
        {
            return new Seeing {
                Range = random.NextFloat(minRange, maxRange)
            };
        }

        private Moving CreateMoving(float minAcceleration, float maxAcceleration, ref Random random)
        {
            return new Moving {
                Acceleration = random.NextFloat(minAcceleration, maxAcceleration)
            };
        }

        private Nutritious CreateNutritious(float minNutrients, float maxNutrients, ref Random random)
        {
            float limit = random.NextFloat(minNutrients, maxNutrients);
            return new Nutritious {
                Current = DietUtils.CurNutrientsFromLimit(limit),
                Limit = limit
            };
        }

        private Lasting CreateLasting(float minLifetime, float maxLifetime, ref Random random)
        {
            return new Lasting {
                Lifetime = random.NextFloat(minLifetime, maxLifetime)
            };
        }

        private Synthesizing CreateSynthesizing(float minStrength, float maxStrength, ref Random random)
        {
            return new Synthesizing {
                Strength = random.NextFloat(minStrength, maxStrength)
            };
        }
    }
}