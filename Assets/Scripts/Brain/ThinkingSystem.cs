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
            FixedList32Bytes<float> currValues = new() {
                Length = ThinkingConsts.MAX_LAYER_SIZE
            };
            FixedList32Bytes<float> nextValues = new() {
                Length = ThinkingConsts.MAX_LAYER_SIZE
            };

            foreach (var (brain, output, receptorEvent) in SystemAPI
                         .Query<RefRO<Thinking>, RefRW<ThoughOutput>, RefRO<SightOutputEvent>>()) {
                // Заполняем prevValues изначальными данными из рецептора
                for (var i = 0; i < receptorEvent.ValueRO.Outputs.Length; i++) {
                    currValues[i] = math.lengthsq(receptorEvent.ValueRO.Outputs[i]);
                }

                for (ushort i = 0; i < brain.ValueRO.LayerSizes.Length - 1; i++) {
                    ushort currLayerSize = brain.ValueRO.LayerSizes[i];
                    ushort nextLayerSize = brain.ValueRO.LayerSizes[i + 1];

                    for (ushort nextNode = 0; nextNode < nextLayerSize; nextNode++) {
                        float sumSignal = 0;

                        for (ushort currNode = 0; currNode < currLayerSize; currNode++) {
                            float signal = currValues[currNode];
                            float weight = brain.ValueRO.GetWeight(i, nextNode, currNode);

                            sumSignal += signal * weight;
                        }

                        nextValues[nextNode] = math.clamp(sumSignal, -1, 1);
                    }

                    currValues = nextValues;
                }

                // Заполняем ThoughOutput полученными данными
                for (var i = 0; i < receptorEvent.ValueRO.Outputs.Length; i++) {
                    output.ValueRW.Values[i] = MathUtils.Clamp(receptorEvent.ValueRO.Outputs[i] * nextValues[i], 1);
                }
            }
        }
    }
}