using Config;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Transforms;
using World;
using RaycastHit = Unity.Physics.RaycastHit;

namespace Diet
{
    [BurstCompile, UpdateAfter(typeof(TransformSystemGroup)), UpdateAfter(typeof(BiteCooldownSystem))]
    public partial struct BiteChunkSystem : ISystem
    {
        private EntityQuery _biterQuery;

        private EntityTypeHandle _entityHandle;
        private ComponentTypeHandle<Biting> _bitingHandle;
        private ComponentTypeHandle<LocalToWorld> _ltwHandle;
        private ComponentTypeHandle<TookBiteEvent> _tookBiteEventHandle;
        private ComponentTypeHandle<BitingCooldown> _bitingCooldownHandle;

        private ComponentLookup<BittenEvent> _bittenEventLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _biterQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<LocalToWorld, Biting>()
                .WithDisabled<TookBiteEvent, BitingCooldown>().Build(ref state);

            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate(_biterQuery);
            state.RequireForUpdate<BittenEvent>();
            state.RequireForUpdate<MainConfig>();

            _entityHandle = state.GetEntityTypeHandle();
            _bitingHandle = state.GetComponentTypeHandle<Biting>(true);
            _ltwHandle = state.GetComponentTypeHandle<LocalToWorld>(true);
            _tookBiteEventHandle = state.GetComponentTypeHandle<TookBiteEvent>();
            _bitingCooldownHandle = state.GetComponentTypeHandle<BitingCooldown>();

            _bittenEventLookup = state.GetComponentLookup<BittenEvent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var mainConfig = SystemAPI.GetSingleton<MainConfig>();
            var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;

            _entityHandle.Update(ref state);
            _bitingHandle.Update(ref state);
            _ltwHandle.Update(ref state);
            _tookBiteEventHandle.Update(ref state);
            _bitingCooldownHandle.Update(ref state);
            _bittenEventLookup.Update(ref state);

            int biterCount = _biterQuery.CalculateEntityCount();
            NativeParallelMultiHashMap<Entity, float> bittenLossMap = new(biterCount * 2, Allocator.TempJob);

            TryTakeBiteJob tryTakeBiteJob = new() {
                EntityHandle = _entityHandle,
                BitingHandle = _bitingHandle,
                LtwHandle = _ltwHandle,
                CollisionFilter = new CollisionFilter {
                    BelongsTo = LayerConsts.FISH_MOUTH,
                    // Все сущности на слое обязаны иметь BittenEvent компонент чтобы не проверять через Lookup
                    CollidesWith = LayerConsts.FISH_BODY,
                    GroupIndex = 0
                },
                CollisionWorld = collisionWorld,
                Cooldown = mainConfig.Diet.Biting.Cooldown,
                BittenLossWriter = bittenLossMap.AsParallelWriter(),
                TookBiteEventHandle = _tookBiteEventHandle,
                BitingCooldownHandle = _bitingCooldownHandle
            };
            tryTakeBiteJob.ScheduleParallel(_biterQuery, state.Dependency).Complete();
            
            (NativeArray<Entity> uniqueBittens, int _) = bittenLossMap.GetUniqueKeyArray(Allocator.TempJob);
            SetBittenEventsJob setBittenEventsJob = new() {
                BittenLossMap = bittenLossMap.AsReadOnly(),
                UniqueBittens = uniqueBittens.AsReadOnly(),
                BittenEventLookup = _bittenEventLookup
            };
            state.Dependency = setBittenEventsJob.Schedule(state.Dependency);
            
            state.Dependency = uniqueBittens.Dispose(state.Dependency);
            state.Dependency = bittenLossMap.Dispose(state.Dependency);
        }

        [BurstCompile]
        private struct TryTakeBiteJob : IJobChunk
        {
            [ReadOnly] public EntityTypeHandle EntityHandle;
            [ReadOnly] public ComponentTypeHandle<Biting> BitingHandle;
            [ReadOnly] public ComponentTypeHandle<LocalToWorld> LtwHandle;
            [ReadOnly] public CollisionFilter CollisionFilter;
            [ReadOnly] public CollisionWorld CollisionWorld;
            public float Cooldown;
            public NativeParallelMultiHashMap<Entity, float>.ParallelWriter BittenLossWriter;
            public ComponentTypeHandle<TookBiteEvent> TookBiteEventHandle;
            public ComponentTypeHandle<BitingCooldown> BitingCooldownHandle;

            public unsafe void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask,
                in v128 chunkEnabledMask)
            {
                // todo zumbak можно добавить staggering

                var pBitings = (Biting*)chunk.GetRequiredComponentDataPtrRO(ref BitingHandle);
                var pLtws = (LocalToWorld*)chunk.GetRequiredComponentDataPtrRO(ref LtwHandle);
                
                NativeArray<Entity> entities = chunk.GetNativeArray(EntityHandle);
                NativeArray<TookBiteEvent> tookBiteEvents = chunk.GetNativeArray(ref TookBiteEventHandle);
                NativeArray<BitingCooldown> bitingCooldowns = chunk.GetNativeArray(ref BitingCooldownHandle);
                
                EnabledMask tookBiteEventEnabledMask = chunk.GetEnabledMask(ref TookBiteEventHandle);
                EnabledMask bitingCooldownEnabledMask = chunk.GetEnabledMask(ref BitingCooldownHandle);

                ChunkEntityEnumerator entityEnumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);

                while (entityEnumerator.NextEntityIndex(out int i)) {
                    if (!CastAllRays(entities[i], in pBitings[i].Rays, in pLtws[i], out RaycastHit firstClosestHit)) {
                        return;
                    }

                    float nutrientDelta = pBitings[i].Strength;

                    tookBiteEvents[i] = new TookBiteEvent {
                        NutrientGain = nutrientDelta
                    };
                    tookBiteEventEnabledMask[i] = true;

                    bitingCooldowns[i] = new BitingCooldown {
                        Value = Cooldown,
                    };
                    bitingCooldownEnabledMask[i] = true;

                    BittenLossWriter.Add(firstClosestHit.Entity, nutrientDelta);
                }
            }

            private bool CastAllRays(Entity biterEntity, in FixedList64Bytes<BitingRay> rays, in LocalToWorld ltw,
                out RaycastHit firstClosestHit)
            {
                RaycastInput input = new() {
                    Filter = CollisionFilter
                };
                firstClosestHit = new RaycastHit();
                for (var i = 0; i < rays.Length; i++) {
                    input.Start = ltw.Position + ltw.Right * rays[i].Start.x + ltw.Up * rays[i].Start.y;
                    input.End = ltw.Position + ltw.Right * rays[i].End.x + ltw.Up * rays[i].End.y;
                    if (!CollisionWorld.CastRay(input, out firstClosestHit)) {
                        continue;
                    }

                    if (biterEntity == firstClosestHit.Entity) {
                        continue;
                    }

                    return true;
                }

                firstClosestHit = new RaycastHit();
                return false;
            }
        }

        [BurstCompile]
        private struct SetBittenEventsJob : IJob
        {
            [ReadOnly] public NativeParallelMultiHashMap<Entity, float>.ReadOnly BittenLossMap;
            [ReadOnly] public NativeArray<Entity>.ReadOnly UniqueBittens;
            public ComponentLookup<BittenEvent> BittenEventLookup;

            public void Execute()
            {
                foreach (Entity bitten in UniqueBittens) {
                    // Складываем все потери и записываем в BittenEvent. Минимизируем random access через Lookup.
                    float lossSum = 0;
                    if (!BittenLossMap.TryGetFirstValue(bitten, out float loss,
                            out NativeParallelMultiHashMapIterator<Entity> it)) {
                        continue;
                    }

                    do {
                        lossSum += loss;
                    } while (BittenLossMap.TryGetNextValue(out loss, ref it));

                    BittenEventLookup[bitten] = new BittenEvent {
                        NutrientLoss = lossSum
                    };
                    BittenEventLookup.SetComponentEnabled(bitten, true);
                }
            }
        }
    }
}