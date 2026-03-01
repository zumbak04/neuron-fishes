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
    // todo zumbak есть сомнения в необходимости этой системы. Тут два варианта:
    // todo zumbak 1. Пусть SpawnService сам создает рыб через EndSimulationEntityCommandBufferSystem
    // todo zumbak 2. Сделать SystemBase и зарегать в VContainer
    [BurstCompile]
    public partial struct SpawnSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<MainConfig>();

            NativeArray<EntityQuery> queries = new(2, Allocator.Temp);
            queries[0] = new EntityQueryBuilder(Allocator.Temp).WithAll<SpawnFishRequest>().Build(ref state);
            queries[1] = new EntityQueryBuilder(Allocator.Temp).WithAll<SpawnRandomFishRequest>().Build(ref state);

            state.RequireAnyForUpdate(queries);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var config = SystemAPI.GetSingleton<MainConfig>();
            var ecbSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            DynamicBuffer<FishPrefabBufferElement> fishPrefabBuffer =
                SystemAPI.GetSingletonBuffer<FishPrefabBufferElement>();

            int2 botLeftCorner = WorldBoundsUtils.GetBotLeftCorner(config.World.Bounds);
            int2 topRightCorner = WorldBoundsUtils.GetTopRightCorner(config.World.Bounds);

            EntityCommandBuffer ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);
            uint seed = (uint)(SystemAPI.Time.ElapsedTime * 1000) + 1;
            Random random = new(seed);
            int fishPrefabsCount = fishPrefabBuffer.Length;

            foreach (var (request, entity) in SystemAPI.Query<RefRO<SpawnRandomFishRequest>>().WithEntityAccess()) {
                for (var i = 0; i < request.ValueRO.Count; i++) {
                    Entity prefab = fishPrefabBuffer[random.NextInt(0, fishPrefabsCount)].Value;
                    Entity instance = ecb.Instantiate(prefab);

                    ecb.SetComponent(instance, new LocalTransform {
                        Position = new float3(random.NextFloat2(botLeftCorner, topRightCorner), 0),
                        Scale = 1f,
                        Rotation = quaternion.identity
                    });

                    ecb.SetComponent(instance,
                        CreateBrain(config.Thinking.HiddenLayerSize, config.Thinking.HiddenLayersCount, ref random));
                    ecb.SetComponent(instance, CreateReceptor(config.Seeing.MinRange, config.Seeing.MaxRange, ref random));
                    ecb.SetComponent(instance,
                        CreateMoving(config.Movement.MinAcceleration, config.Movement.MaxAcceleration, ref random));
                    ecb.SetComponent(instance,
                        CreateNutritious(config.Diet.MinNutrients, config.Diet.MaxNutrients, ref random));
                    ecb.SetComponent(instance, CreateLasting(config.Life.MinLifetime, config.Life.MaxLifetime, ref random));
                    if (SystemAPI.HasComponent<Synthesizing>(prefab)) {
                        ecb.SetComponent(instance,
                            CreateSynthesizing(config.Diet.Synthesizing.MinStrength,
                                config.Diet.Synthesizing.MaxStrength,
                                ref random));
                    }

                    if (SystemAPI.HasComponent<Biting>(prefab)) {
                        // Нужно рандомизировать Strength не трогая лучи
                        var biting = SystemAPI.GetComponent<Biting>(prefab);
                        biting.Strength =
                            random.NextFloat(config.Diet.Biting.MinStrength, config.Diet.Biting.MaxStrength);
                        ecb.SetComponent(instance, biting);
                    }
                }

                ecb.DestroyEntity(entity);
            }

            foreach (var (request, entity) in SystemAPI.Query<RefRO<SpawnFishRequest>>().WithEntityAccess()) {
                for (var i = 0; i < request.ValueRO.Count; i++) {
                    Entity prefab = fishPrefabBuffer[random.NextInt(0, fishPrefabsCount)].Value;
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
                    if (SystemAPI.HasComponent<Synthesizing>(prefab)) {
                        ecb.SetComponent(instance, request.ValueRO.Synthesizing);
                    }
                    
                    if (SystemAPI.HasComponent<Biting>(prefab)) {
                        // Нужно задать Strength не трогая лучи
                        var biting = SystemAPI.GetComponent<Biting>(prefab);
                        biting.Strength = request.ValueRO.Biting.Strength;
                        ecb.SetComponent(instance, biting);
                    }
                }

                ecb.DestroyEntity(entity);
            }
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

        private Seeing CreateReceptor(float minRange, float maxRange, ref Random random)
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