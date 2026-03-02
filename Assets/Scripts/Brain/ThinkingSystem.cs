using Math;
using Sight;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Brain
{
    // todo zumbak надо оптимизировать и добавить Jobs
    [BurstCompile, UpdateAfter(typeof(SightSystem))]
    public partial struct ThinkingSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Thinking>();
            state.RequireForUpdate<SightOutputEvent>();
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
            private void Execute(in Thinking thinking, in SightOutputEvent sightOutputEvent,
                ref ThoughOutput thoughOutput)
            {
                FixedList32Bytes<float> currValues = new() {
                    Length = ThinkingConsts.MAX_LAYER_SIZE
                };
                FixedList32Bytes<float> nextValues = new() {
                    Length = ThinkingConsts.MAX_LAYER_SIZE
                };

                // Заполняем prevValues изначальными данными из рецептора
                for (var i = 0; i < sightOutputEvent.Outputs.Length; i++) {
                    currValues[i] = math.lengthsq(sightOutputEvent.Outputs[i]);
                }

                for (ushort i = 0; i < thinking.LayerSizes.Length - 1; i++) {
                    ushort currLayerSize = thinking.LayerSizes[i];
                    ushort nextLayerSize = thinking.LayerSizes[i + 1];

                    for (ushort nextNode = 0; nextNode < nextLayerSize; nextNode++) {
                        float sumSignal = 0;

                        for (ushort currNode = 0; currNode < currLayerSize; currNode++) {
                            float signal = currValues[currNode];
                            float weight = thinking.GetWeight(i, nextNode, currNode);

                            sumSignal += signal * weight;
                        }

                        nextValues[nextNode] = math.clamp(sumSignal, -1, 1);
                    }

                    currValues = nextValues;
                }

                // Заполняем ThoughOutput полученными данными
                for (var i = 0; i < sightOutputEvent.Outputs.Length; i++) {
                    thoughOutput.Values[i] = MathUtils.Clamp(sightOutputEvent.Outputs[i] * nextValues[i], 1);
                }
            }
        }
    }
}