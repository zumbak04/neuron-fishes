using Sight;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Brain
{
    [BurstCompile, UpdateAfter(typeof(SightSystem))]
    public partial struct ThinkingSystem : ISystem
    {
        private EntityQuery _entityQuery;

        private ComponentTypeHandle<Thinking> _thinkingHandle;
        private ComponentTypeHandle<SeenEvent> _seenEventHandle;
        private ComponentTypeHandle<ThoughOutput> _thoughOutputHandle;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Thinking, SeenEvent, ThoughOutput>()
                .Build(ref state);

            state.RequireForUpdate(_entityQuery);

            _thinkingHandle = state.GetComponentTypeHandle<Thinking>(true);
            _seenEventHandle = state.GetComponentTypeHandle<SeenEvent>(true);
            _thoughOutputHandle = state.GetComponentTypeHandle<ThoughOutput>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _thinkingHandle.Update(ref state);
            _seenEventHandle.Update(ref state);
            _thoughOutputHandle.Update(ref state);

            ThinkJob thinkJob = new() {
                ThinkingHandle = _thinkingHandle,
                SeenEventHandle = _seenEventHandle,
                ThoughOutputHandle = _thoughOutputHandle
            };

            state.Dependency = thinkJob.ScheduleParallel(_entityQuery, state.Dependency);
        }

        [BurstCompile]
        private struct ThinkJob : IJobChunk
        {
            [ReadOnly] public ComponentTypeHandle<Thinking> ThinkingHandle;
            [ReadOnly] public ComponentTypeHandle<SeenEvent> SeenEventHandle;
            public ComponentTypeHandle<ThoughOutput> ThoughOutputHandle;
            
            public unsafe void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask,
                in v128 chunkEnabledMask)
            {
                NativeArray<Thinking> thinkings = chunk.GetNativeArray(ref ThinkingHandle);
                NativeArray<SeenEvent> seenEvents = chunk.GetNativeArray(ref SeenEventHandle);
                NativeArray<ThoughOutput> thoughOutputs = chunk.GetNativeArray(ref ThoughOutputHandle);
                
                // Раньше использовал FixedList.
                // Теперь нет лишней обертки. Можно сделать двойную буферизацию.
                float* pCurrValues = stackalloc float[ThinkingConsts.MAX_LAYER_SIZE];
                float* pNextValues = stackalloc float[ThinkingConsts.MAX_LAYER_SIZE];

                ChunkEntityEnumerator entityEnumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);

                while (entityEnumerator.NextEntityIndex(out int i)) {
                    Thinking thinking = thinkings[i];
                    SeenEvent seenEvent = seenEvents[i];
                    
                    // Заполняем currValues изначальными данными.
                    for (var j = 0; j < seenEvent.ToTargets.Length; j++) {
                        pCurrValues[j] = math.lengthsq(seenEvent.ToTargets[j]);
                    }

                    for (var layer = 0; layer < thinking.LayerSizes.Length - 1; layer++) {
                        int currLayerSize = thinking.LayerSizes[layer];
                        int nextLayerSize = thinking.LayerSizes[layer + 1];

                        for (var nextNode = 0; nextNode < nextLayerSize; nextNode++) {
                            float sumSignal = 0;

                            for (var currNode = 0; currNode < currLayerSize; currNode++) {
                                float signal = pCurrValues[currNode];
                                sumSignal += signal * thinking.GetWeight(layer, nextNode, currNode);
                            }

                            pNextValues[nextNode] = sumSignal;
                        }

                        float* pTemp = pCurrValues;
                        pCurrValues = pNextValues;
                        pNextValues = pTemp;
                    }

                    // Умножаем все вектора до целей на финальное значение узла.
                    float2 direction = float2.zero;
                    for (var j = 0; j < seenEvent.ToTargets.Length; j++) {
                        direction += seenEvent.ToTargets[j] * pCurrValues[j];
                    }

                    thoughOutputs[i] = new ThoughOutput {
                        Direction = math.normalizesafe(direction, float2.zero)
                    };
                }
            }
        }
    }
}