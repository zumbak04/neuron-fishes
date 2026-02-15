using Math;
using Receptor;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using SeeingSystem = Receptor.SeeingSystem;

namespace Brain
{
    // todo zumbak надо оптимизировать и добавить Jobs
    [BurstCompile, UpdateAfter(typeof(SeeingSystem))]
    public partial struct ThinkingSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Thinking>();
            state.RequireForUpdate<SeeingOutputEvent>();
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

            foreach (var (brain, output, receptorEvent) in SystemAPI.Query<RefRO<Thinking>, RefRW<ThoughOutput>, RefRO<SeeingOutputEvent>>()) {
                // Заполняем prevValues изначальными данными из рецептора
                for (int i = 0; i < receptorEvent.ValueRO.outputs.Length; i++) {
                    currValues[i] = math.lengthsq(receptorEvent.ValueRO.outputs[i]);
                }

                for (ushort i = 0; i < brain.ValueRO.layerSizes.Length - 1; i++) {
                    ushort currLayerSize = brain.ValueRO.layerSizes[i];
                    ushort nextLayerSize = brain.ValueRO.layerSizes[i + 1];
                    
                    for (ushort nextNode = 0; nextNode < nextLayerSize; nextNode++) 
                    {
                        float sumSignal = 0;
                        
                        for (ushort currNode = 0; currNode < currLayerSize; currNode++) 
                        {
                            float signal = currValues[currNode];
                            float weight = brain.ValueRO.GetWeight(i, nextNode, currNode);
            
                            sumSignal += signal * weight;
                        }
                        
                        nextValues[nextNode] = math.clamp(sumSignal, -1, 1); 
                    }
                    
                    currValues = nextValues;
                }

                // Заполняем ThoughOutput полученными данными
                for (int i = 0; i < receptorEvent.ValueRO.outputs.Length; i++) {
                    output.ValueRW.values[i] = MathUtils.Clamp(receptorEvent.ValueRO.outputs[i] * nextValues[i], 1);
                }
            }
        }
    }
}