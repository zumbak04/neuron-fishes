using Sight;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Brain
{
    [BurstCompile, UpdateAfter(typeof(SightSystem))]
    public partial struct ThinkingEntitySystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.Enabled = false;
            
            EntityQuery query = new EntityQueryBuilder(Allocator.Temp).WithAll<Thinking, SeenEvent, ThoughOutput>()
                .Build(ref state);

            state.RequireForUpdate(query);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            ThinkJob thinkJob = new();

            state.Dependency = thinkJob.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        private partial struct ThinkJob : IJobEntity
        {
            private void Execute(in Thinking thinking, in SeenEvent seenEvent, ref ThoughOutput thoughOutput)
            {
                FixedList32Bytes<float> currValues = new() {
                    Length = ThinkingConsts.MAX_LAYER_SIZE
                };
                FixedList32Bytes<float> nextValues = new() {
                    Length = ThinkingConsts.MAX_LAYER_SIZE
                };

                // Заполняем currValues изначальными данными.
                for (var i = 0; i < seenEvent.ToTargets.Length; i++) {
                    currValues[i] = math.lengthsq(seenEvent.ToTargets[i]);
                }

                for (var i = 0; i < thinking.LayerSizes.Length - 1; i++) {
                    int currLayerSize = thinking.LayerSizes[i];
                    int nextLayerSize = thinking.LayerSizes[i + 1];

                    for (var nextNode = 0; nextNode < nextLayerSize; nextNode++) {
                        float sumSignal = 0;

                        for (var currNode = 0; currNode < currLayerSize; currNode++) {
                            float signal = currValues[currNode];
                            sumSignal += signal * thinking.GetWeight(i, nextNode, currNode);
                        }

                        nextValues[nextNode] = sumSignal;
                    }

                    currValues = nextValues;
                }

                // Умножаем все вектора до целей на финальное значение узла.
                float2 direction = float2.zero;
                for (var i = 0; i < seenEvent.ToTargets.Length; i++) {
                    direction += seenEvent.ToTargets[i] * currValues[i];
                }

                thoughOutput.Direction = math.normalizesafe(direction, float2.zero);
            }
        }
    }
}