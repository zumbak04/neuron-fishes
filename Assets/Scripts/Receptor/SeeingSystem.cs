using Brain;
using Config;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Receptor
{
    // todo zumbak эту систему можно выделить в отдельную группу и автоматически устанавливать timestep в зависимости от загрузки
    [BurstCompile]
    public partial struct SeeingSystem : ISystem
    {
        private NativeParallelMultiHashMap<int, SpatialHashItem> _spatialHash;

        private struct SpatialHashItem
        {
            public Entity entity;
            public float2 position;
        }

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MainConfig>();
            state.RequireForUpdate<Seeing>();

            _spatialHash = new NativeParallelMultiHashMap<int, SpatialHashItem>(1024, Allocator.Persistent);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            if (_spatialHash.IsCreated) {
                _spatialHash.Dispose();
            }
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var config = SystemAPI.GetSingleton<MainConfig>();
            
            // Сначала уменьшаем cooldown и смотрим есть ли вообще работа в этом кадре
            bool anyToUpdate = false;
            foreach (RefRW<Seeing> receptor in SystemAPI.Query<RefRW<Seeing>>()) {
                if (receptor.ValueRO.cooldown > 0) {
                    receptor.ValueRW.cooldown -= SystemAPI.Time.DeltaTime;
                } else {
                    anyToUpdate = true;
                }
            }

            if (!anyToUpdate) {
                return;
            }

            float cellSize = math.max(1f, config.seeing._maxRange);

            // Переиспользуем буфер; ёмкость подстраиваем по числу целей.
            int targetCount = SystemAPI.QueryBuilder().WithAll<SeeingTargetTag, LocalTransform>().Build().CalculateEntityCount();
            if (_spatialHash.Capacity < targetCount * 2) {
                _spatialHash.Capacity = math.max(_spatialHash.Capacity, targetCount * 2);
            }

            _spatialHash.Clear();

            JobHandle buildHashJobHandle = new BuildSpatialHashJob {
                    cellSize = cellSize,
                    writer = _spatialHash.AsParallelWriter()
            }.ScheduleParallel(state.Dependency);

            JobHandle searchJobHandle = new ReceptorSearchJob {
                    receptorCooldown = config.seeing._cooldown,
                    cellSize = cellSize,
                    hash = _spatialHash
            }.ScheduleParallel(buildHashJobHandle);

            state.Dependency = searchJobHandle;
        }

        [BurstCompile, WithAll(typeof(SeeingTargetTag))]
        private partial struct BuildSpatialHashJob : IJobEntity
        {
            public float cellSize;
            public NativeParallelMultiHashMap<int, SpatialHashItem>.ParallelWriter writer;
            
            private void Execute(Entity entity, in LocalTransform transform)
            {
                float2 pos = transform.Position.xy;
                int2 cell = (int2) math.floor(pos / cellSize);
                int key = (int) math.hash(cell);

                writer.Add(key, new SpatialHashItem {
                        entity = entity,
                        position = pos
                });
            }
        }

        [BurstCompile, WithDisabled(typeof(SeeingOutputEvent))]
        private partial struct ReceptorSearchJob : IJobEntity
        {
            public float receptorCooldown;
            public float cellSize;
            [ReadOnly]
            public NativeParallelMultiHashMap<int, SpatialHashItem> hash;

            // todo zumbak нужно уменшить Cognitive Complexity
            private void Execute(Entity entity,
                                 ref Seeing seeing,
                                 ref SeeingOutputEvent triggeredEvent,
                                 EnabledRefRW<SeeingOutputEvent> triggeredEventEnabled,
                                 in LocalTransform transform)
            {
                if (seeing.cooldown > 0) {
                    // Гарантируем, что событие выключено.
                    triggeredEventEnabled.ValueRW = false;
                    return;
                }

                seeing.cooldown = receptorCooldown;

                float2 selfPos = transform.Position.xy;
                float rangeSq = seeing.range * seeing.range;

                float2 sumVec = float2.zero;
                int count = 0;

                int2 selfCell = (int2) math.floor(selfPos / cellSize);

                for (int dy = -1; dy <= 1; dy++) {
                    for (int dx = -1; dx <= 1; dx++) {
                        int2 cell = selfCell + new int2(dx, dy);
                        int key = (int) math.hash(cell);

                        if (!hash.TryGetFirstValue(key, out SpatialHashItem item, out NativeParallelMultiHashMapIterator<int> it)) {
                            continue;
                        }
                        do {
                            if (item.entity == entity) {
                                continue;
                            }

                            float2 diff = item.position - selfPos;
                            float distSq = math.lengthsq(diff);
                            if (distSq > rangeSq) {
                                continue;
                            }
                            
                            sumVec += diff;
                            count++;
                        } while (hash.TryGetNextValue(out item, ref it));
                    }
                }

                triggeredEvent.outputs.Length = ThinkingConsts.INPUT_SIZE;
                triggeredEvent.outputs[0] = count == 0 ? float2.zero : (sumVec / count);
                // todo zumbak
                triggeredEvent.outputs[1] = float2.zero;
                triggeredEvent.outputs[2] = float2.zero;

                triggeredEventEnabled.ValueRW = true;
            }
        }
    }
}