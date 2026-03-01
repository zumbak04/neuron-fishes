using Brain;
using Config;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Sight
{
    // todo zumbak подумать может можно ещё оптимизировать?
    [BurstCompile]
    public partial struct SightSystem : ISystem
    {
        private NativeParallelMultiHashMap<int, SpatialHashItem> _spatialHash;

        private struct SpatialHashItem
        {
            public Entity Entity;
            public float2 Position;
        }

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MainConfig>();
            state.RequireForUpdate<Seeing>();
            state.RequireForUpdate<SightTargetTag>();

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
            var anyToUpdate = false;
            foreach (RefRW<Seeing> receptor in SystemAPI.Query<RefRW<Seeing>>()) {
                if (receptor.ValueRO.Cooldown > 0) {
                    receptor.ValueRW.Cooldown -= SystemAPI.Time.DeltaTime;
                }
                else {
                    anyToUpdate = true;
                }
            }

            if (!anyToUpdate) {
                return;
            }

            float cellSize = math.max(1f, config.Seeing.MaxRange);

            // Переиспользуем буфер. Ёмкость подстраиваем по числу целей.
            int targetCount = SystemAPI.QueryBuilder().WithAll<SightTargetTag, LocalTransform>().Build()
                .CalculateEntityCount();
            if (_spatialHash.Capacity < targetCount * 2) {
                _spatialHash.Capacity = math.max(_spatialHash.Capacity, targetCount * 2);
            }

            _spatialHash.Clear();

            JobHandle buildHashJobHandle = new BuildSpatialHashJob {
                CellSize = cellSize,
                Writer = _spatialHash.AsParallelWriter()
            }.ScheduleParallel(state.Dependency);

            state.Dependency = new SeeSpatialTargetsJob {
                Cooldown = config.Seeing.Cooldown,
                CellSize = cellSize,
                Hash = _spatialHash
            }.ScheduleParallel(buildHashJobHandle);
        }

        [BurstCompile, WithAll(typeof(SightTargetTag))]
        private partial struct BuildSpatialHashJob : IJobEntity
        {
            public float CellSize;
            public NativeParallelMultiHashMap<int, SpatialHashItem>.ParallelWriter Writer;

            private void Execute(Entity entity, in LocalTransform transform)
            {
                float2 pos = transform.Position.xy;
                var cell = (int2)math.floor(pos / CellSize);
                var key = (int)math.hash(cell);

                Writer.Add(key, new SpatialHashItem {
                    Entity = entity,
                    Position = pos
                });
            }
        }

        [BurstCompile, WithDisabled(typeof(SightOutputEvent))]
        private partial struct SeeSpatialTargetsJob : IJobEntity
        {
            public float Cooldown;
            public float CellSize;
            // todo zumbak хорошая ли идея копировать целую hash map?
            [ReadOnly] public NativeParallelMultiHashMap<int, SpatialHashItem> Hash;

            // todo zumbak нужно уменшить Cognitive Complexity
            private void Execute(Entity entity,
                ref Seeing seeing,
                ref SightOutputEvent triggeredEvent,
                EnabledRefRW<SightOutputEvent> triggeredEventEnabled,
                in LocalTransform transform)
            {
                if (seeing.Cooldown > 0) {
                    // Гарантируем, что событие выключено.
                    triggeredEventEnabled.ValueRW = false;
                    return;
                }

                seeing.Cooldown = Cooldown;

                float2 selfPos = transform.Position.xy;
                float rangeSq = seeing.Range * seeing.Range;

                SightTarget fishTarget = new();

                var selfCell = (int2)math.floor(selfPos / CellSize);

                for (int dy = -1; dy <= 1; dy++)
                for (int dx = -1; dx <= 1; dx++) {
                    int2 cell = selfCell + new int2(dx, dy);
                    var key = (int)math.hash(cell);

                    if (!Hash.TryGetFirstValue(key, out SpatialHashItem hashItem,
                            out NativeParallelMultiHashMapIterator<int> iterator)) {
                        continue;
                    }

                    do {
                        if (hashItem.Entity == entity) {
                            continue;
                        }

                        float2 to = hashItem.Position - selfPos;

                        float distanceSq = math.lengthsq(to);
                        if (distanceSq > rangeSq) {
                            continue;
                        }

                        fishTarget.TryUpdateNearest(to, distanceSq);
                    } while (Hash.TryGetNextValue(out hashItem, ref iterator));
                }

                triggeredEvent.Outputs.Length = ThinkingConsts.INPUT_SIZE;
                triggeredEvent.Outputs[0] = fishTarget.To;
                // todo zumbak
                triggeredEvent.Outputs[1] = float2.zero;
                triggeredEvent.Outputs[2] = float2.zero;

                triggeredEventEnabled.ValueRW = true;
            }
        }

        private struct SightTarget
        {
            public float2 To { get; private set; }
            private float _distanceSq;

            public SightTarget()
            {
                To = float2.zero;
                _distanceSq = float.MaxValue;
            }

            public bool TryUpdateNearest(float2 to, float distanceSq)
            {
                if (distanceSq >= _distanceSq) {
                    return false;
                }

                To = to;
                _distanceSq = distanceSq;
                return true;
            }
        }
    }
}