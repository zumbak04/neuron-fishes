using Math;
using Sight;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Brain
{
    [BurstCompile, UpdateAfter(typeof(SightSystem))]
    public partial struct ThinkingSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Thinking>();
            state.RequireForUpdate<SeenEvent>();
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
            private void Execute(in Thinking thinking, in SeenEvent seenEvent,
                ref ThoughOutput thoughOutput)
            {
                FixedList32Bytes<float> currValues = new() {
                    Length = ThinkingConsts.MAX_LAYER_SIZE
                };
                FixedList32Bytes<float> nextValues = new() {
                    Length = ThinkingConsts.MAX_LAYER_SIZE
                };

                // Заполняем prevValues изначальными данными из рецептора
                for (var i = 0; i < seenEvent.ToTargets.Length; i++) {
                    currValues[i] = math.lengthsq(seenEvent.ToTargets[i]);
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

                        nextValues[nextNode] = sumSignal;
                    }

                    currValues = nextValues;
                }

                // Заполняем ThoughOutput полученными данными
                for (var i = 0; i < seenEvent.ToTargets.Length; i++) {
                    thoughOutput.Values[i] = MathUtils.Clamp(seenEvent.ToTargets[i] * currValues[i], 1);
                }
            }
        }
    }
}