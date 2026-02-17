using Brain;
using Config;
using Diet;
using Life;
using Math;
using Move;
using Receptor;
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

            // todo zumbak Burst ругается на EntityQuery
            EntityQuery requestQuery = state.GetEntityQuery(ComponentType.ReadOnly<SpawnFishRequest>());
            EntityQuery randomRequestQuery = state.GetEntityQuery(ComponentType.ReadOnly<SpawnRandomFishRequest>());

            state.RequireAnyForUpdate(requestQuery, randomRequestQuery);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var config = SystemAPI.GetSingleton<MainConfig>();
            var ecbSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            DynamicBuffer<FishPrefabBufferElement> fishPrefabBuffer = SystemAPI.GetSingletonBuffer<FishPrefabBufferElement>();

            int2 botLeftCorner = WorldBoundsUtils.GetBotLeftCorner(config.world._bounds);
            int2 topRightCorner = WorldBoundsUtils.GetTopRightCorner(config.world._bounds);

            EntityCommandBuffer ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);
            uint seed = (uint) (SystemAPI.Time.ElapsedTime * 1000) + 1;
            Random random = new(seed);
            int fishPrefabsCount = fishPrefabBuffer.Length;

            foreach (var (request, entity) in SystemAPI.Query<RefRO<SpawnRandomFishRequest>>().WithEntityAccess()) {
                for (int i = 0; i < request.ValueRO.count; i++) {
                    Entity prefab = fishPrefabBuffer[random.NextInt(0, fishPrefabsCount)].value;
                    Entity instance = ecb.Instantiate(prefab);

                    ecb.SetComponent(instance, new LocalTransform() {
                            Position = new float3(random.NextFloat2(botLeftCorner, topRightCorner), 0),
                            Scale = 1f,
                            Rotation = quaternion.identity
                    });

                    ecb.SetComponent(instance, CreateBrain(config.thinking._hiddenLayerSize, config.thinking._hiddenLayersCount, random));
                    ecb.SetComponent(instance, CreateReceptor(config.seeing._minRange, config.seeing._maxRange, random));
                    ecb.SetComponent(instance, CreateMoving(config.movement._minAcceleration, config.movement._maxAcceleration, random));
                    ecb.SetComponent(instance, CreateNutritious(config.diet._minNutrients, config.diet._maxNutrients, random));
                    ecb.SetComponent(instance, CreateLasting(config.life._minLifetime, config.life._maxLifetime, random));
                    if (SystemAPI.HasComponent<Synthesizing>(prefab)) {
                        ecb.SetComponent(instance,
                                         CreateSynthesizing(config.diet._synthesizing._minStrength, config.diet._synthesizing._maxStrength, random));
                    }
                }
                ecb.DestroyEntity(entity);
            }

            foreach (var (request, entity) in SystemAPI.Query<RefRO<SpawnFishRequest>>().WithEntityAccess()) {
                for (int i = 0; i < request.ValueRO.count; i++) {
                    Entity prefab = fishPrefabBuffer[random.NextInt(0, fishPrefabsCount)].value;
                    Entity instance = ecb.Instantiate(prefab);

                    ecb.SetComponent(instance, new LocalTransform() {
                            Position = new float3(request.ValueRO.position, 0),
                            Scale = 1f,
                            Rotation = quaternion.identity
                    });

                    ecb.SetComponent(instance, request.ValueRO.thinking);
                    ecb.SetComponent(instance, request.ValueRO.seeing);
                    ecb.SetComponent(instance, request.ValueRO.moving);
                    ecb.SetComponent(instance, request.ValueRO.nutritious);
                    ecb.SetComponent(instance, request.ValueRO.lasting);
                    if (SystemAPI.HasComponent<Synthesizing>(prefab)) {
                        ecb.SetComponent(instance, request.ValueRO.synthesizing);
                    }
                }
                ecb.DestroyEntity(entity);
            }
        }

        private Thinking CreateBrain(ushort hiddenLayersSize, ushort hiddenLayersCount, Random random)
        {
            FixedList32Bytes<ushort> layerSizes = new();
            layerSizes.Length = 2 + hiddenLayersCount;
            layerSizes[0] = ThinkingConsts.INPUT_SIZE;
            layerSizes[^1] = ThinkingConsts.OUTPUT_SIZE;

            for (ushort i = 0; i < hiddenLayersCount; i++) {
                layerSizes[i + 1] = hiddenLayersSize;
            }
            Thinking thinking = new(layerSizes);

            for (int i = 0; i < thinking.weights.Length; i++) {
                thinking.weights[i] = (Snorm8) random.NextFloat(ThinkingConsts.MIN_NODE_WEIGHT, ThinkingConsts.MAX_NODE_WEIGHT);
            }
            return thinking;
        }

        private Seeing CreateReceptor(float minRange, float maxRange, Random random)
        {
            return new Seeing {
                    range = random.NextFloat(minRange, maxRange)
            };
        }

        private Moving CreateMoving(float minAcceleration, float maxAccelaration, Random random)
        {
            return new Moving {
                    acceleration = random.NextFloat(minAcceleration, maxAccelaration)
            };
        }

        private Nutritious CreateNutritious(float minNutrients, float maxNutrients, Random random)
        {
            float limit = random.NextFloat(minNutrients, maxNutrients);
            return new Nutritious {
                    cur = DietUtils.CurNutrientsFromLimit(limit),
                    limit = limit,
            };
        }

        private Lasting CreateLasting(float minLifetime, float maxLifetime, Random random)
        {
            return new Lasting() {
                    lifetime = random.NextFloat(minLifetime, maxLifetime)
            };
        }

        private Synthesizing CreateSynthesizing(float minStrength, float maxStrength, Random random)
        {
            return new Synthesizing() {
                    strength = random.NextFloat(minStrength, maxStrength)
            };
        }
    }
}