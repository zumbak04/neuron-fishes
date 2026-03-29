using Config;
using Reproduction;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using World;
using RaycastHit = Unity.Physics.RaycastHit;

namespace Diet
{
    [BurstCompile, UpdateAfter(typeof(TransformSystemGroup)), UpdateAfter(typeof(BiteCooldownSystem))]
    public partial struct BiteSystem : ISystem
    {
        private EntityQuery _biterQuery;

        private EntityTypeHandle _entityHandle;
        private ComponentTypeHandle<Biting> _bitingHandle;
        private ComponentTypeHandle<LocalToWorld> _ltwHandle;
        private ComponentTypeHandle<Family> _familyHandle;
        private ComponentTypeHandle<BitingCooldown> _bitingCooldownHandle;

        private ComponentLookup<Nutritious> _nutritiousLookup;
        private ComponentLookup<Family> _familyLookup;
        private ComponentLookup<BittenEvent> _bittenEventLookup;
        private ComponentLookup<TookBiteEvent> _tookBiteEventLookup;

        private uint _frameCount;

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
            _familyHandle = state.GetComponentTypeHandle<Family>(true);
            _bitingCooldownHandle = state.GetComponentTypeHandle<BitingCooldown>();

            _nutritiousLookup = state.GetComponentLookup<Nutritious>(true);
            _familyLookup = state.GetComponentLookup<Family>(true);
            _bittenEventLookup = state.GetComponentLookup<BittenEvent>();
            _tookBiteEventLookup = state.GetComponentLookup<TookBiteEvent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var mainConfig = SystemAPI.GetSingleton<MainConfig>();
            var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;

            _frameCount++;

            _entityHandle.Update(ref state);
            _bitingHandle.Update(ref state);
            _ltwHandle.Update(ref state);
            _familyHandle.Update(ref state);
            _bitingCooldownHandle.Update(ref state);

            _nutritiousLookup.Update(ref state);
            _familyLookup.Update(ref state);
            _bittenEventLookup.Update(ref state);
            _tookBiteEventLookup.Update(ref state);

            // todo zumbak переделать в поле?
            int biterCount = _biterQuery.CalculateEntityCount();
            NativeParallelMultiHashMap<Entity, BiterData> bittenBitterMap = new(biterCount * 2, Allocator.TempJob);

            TryTakeBiteJob tryTakeBiteJob = new() {
                EntityHandle = _entityHandle,
                BitingHandle = _bitingHandle,
                LtwHandle = _ltwHandle,
                FamilyHandle = _familyHandle,
                CollisionFilter = new CollisionFilter {
                    BelongsTo = LayerConsts.FISH_MOUTH,
                    // Все сущности на слое обязаны иметь BittenEvent компонент чтобы не проверять через Lookup
                    CollidesWith = LayerConsts.FISH_BODY,
                    GroupIndex = 0
                },
                CollisionWorld = collisionWorld,
                Cooldown = mainConfig.Diet.Biting.Cooldown,
                BittenBitterWriter = bittenBitterMap.AsParallelWriter(),
                BitingCooldownHandle = _bitingCooldownHandle,
                FamilyLookup = _familyLookup,
                FrameCount = _frameCount,
                StaggeringInterval = mainConfig.Diet.Biting.StaggeringInterval
            };
            tryTakeBiteJob.ScheduleParallel(_biterQuery, state.Dependency).Complete();

            (NativeArray<Entity> uniqueBittens, int _) = bittenBitterMap.GetUniqueKeyArray(Allocator.TempJob);
            SetBittenEventsJob setBittenEventsJob = new() {
                BittenBitterReader = bittenBitterMap.AsReadOnly(),
                UniqueBittens = uniqueBittens.AsReadOnly(),
                NutritiousLookup = _nutritiousLookup,
                BittenEventLookup = _bittenEventLookup,
                TookBiteEventLookup = _tookBiteEventLookup
            };
            state.Dependency = setBittenEventsJob.Schedule(state.Dependency);

            state.Dependency = uniqueBittens.Dispose(state.Dependency);
            state.Dependency = bittenBitterMap.Dispose(state.Dependency);
        }

        [BurstCompile]
        private struct TryTakeBiteJob : IJobChunk
        {
            [ReadOnly] public EntityTypeHandle EntityHandle;
            [ReadOnly] public ComponentTypeHandle<Biting> BitingHandle;
            [ReadOnly] public ComponentTypeHandle<LocalToWorld> LtwHandle;
            [ReadOnly] public ComponentTypeHandle<Family> FamilyHandle;
            public ComponentTypeHandle<BitingCooldown> BitingCooldownHandle;

            [ReadOnly] public ComponentLookup<Family> FamilyLookup;

            [ReadOnly] public CollisionFilter CollisionFilter;
            [ReadOnly] public CollisionWorld CollisionWorld;
            public NativeParallelMultiHashMap<Entity, BiterData>.ParallelWriter BittenBitterWriter;
            public float Cooldown;
            public uint FrameCount;
            public ushort StaggeringInterval;

            public unsafe void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask,
                in v128 chunkEnabledMask)
            {
                if (StaggeringInterval > 1 &&
                    unfilteredChunkIndex % StaggeringInterval != FrameCount % StaggeringInterval) {
                    return;
                }

                Entity* pEntities = chunk.GetEntityDataPtrRO(EntityHandle);
                var pBitings = (Biting*)chunk.GetRequiredComponentDataPtrRO(ref BitingHandle);
                var pLtws = (LocalToWorld*)chunk.GetRequiredComponentDataPtrRO(ref LtwHandle);
                var pFamilies = (Family*)chunk.GetRequiredComponentDataPtrRO(ref FamilyHandle);
                var pBitingCooldowns = (BitingCooldown*)chunk.GetRequiredComponentDataPtrRW(ref BitingCooldownHandle);

                EnabledMask bitingCooldownEnabledMask = chunk.GetEnabledMask(ref BitingCooldownHandle);

                ChunkEntityEnumerator entityEnumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);

                while (entityEnumerator.NextEntityIndex(out int i)) {
                    if (!CastAllRays(pEntities[i], in pBitings[i].Rays, in pLtws[i], in pFamilies[i], out RaycastHit firstClosestHit)) {
                        return;
                    }

                    pBitingCooldowns[i] = new BitingCooldown {
                        Value = Cooldown,
                    };
                    bitingCooldownEnabledMask[i] = true;

                    BittenBitterWriter.Add(firstClosestHit.Entity, new BiterData {
                        Entity = pEntities[i],
                        BitingStrength = pBitings[i].Strength
                    });
                }
            }

            private bool CastAllRays(Entity biterEntity, in FixedList64Bytes<BitingRay> rays, in LocalToWorld ltw,
                in Family family, out RaycastHit firstClosestHit)
            {
                RaycastInput input = new() {
                    Filter = CollisionFilter
                };
                firstClosestHit = default;
                for (var i = 0; i < rays.Length; i++) {
                    input.Start = ltw.Position + ltw.Right * rays[i].Start.x + ltw.Up * rays[i].Start.y;
                    input.End = ltw.Position + ltw.Right * rays[i].End.x + ltw.Up * rays[i].End.y;
                    if (!CollisionWorld.CastRay(input, out firstClosestHit)) {
                        continue;
                    }

                    if (biterEntity == firstClosestHit.Entity) {
                        continue;
                    }

                    if (FamilyUtils.AreRelated(biterEntity, firstClosestHit.Entity, in family,
                            FamilyLookup[firstClosestHit.Entity])) {
                        continue;
                    }

                    return true;
                }

                firstClosestHit = default;
                return false;
            }
        }

        [BurstCompile]
        private struct SetBittenEventsJob : IJob
        {
            [ReadOnly] public NativeParallelMultiHashMap<Entity, BiterData>.ReadOnly BittenBitterReader;
            [ReadOnly] public NativeArray<Entity>.ReadOnly UniqueBittens;
            [ReadOnly] public ComponentLookup<Nutritious> NutritiousLookup;
            public ComponentLookup<BittenEvent> BittenEventLookup;
            public ComponentLookup<TookBiteEvent> TookBiteEventLookup;

            public void Execute()
            {
                foreach (Entity bitten in UniqueBittens) {
                    if (!BittenBitterReader.TryGetFirstValue(bitten, out BiterData biter,
                            out NativeParallelMultiHashMapIterator<Entity> it)) {
                        continue;
                    }

                    float currentNutrients = NutritiousLookup.GetRefRO(bitten).ValueRO.Current;
                    float biteNutrientLossSum = 0;

                    do {
                        if (currentNutrients <= 0) {
                            continue;
                        }

                        float biteNutrientLoss = math.min(currentNutrients, biter.BitingStrength);
                        currentNutrients -= biteNutrientLoss;
                        biteNutrientLossSum += biteNutrientLoss;
                        TookBiteEventLookup[biter.Entity] = new TookBiteEvent {
                            NutrientGain = biteNutrientLoss
                        };
                        TookBiteEventLookup.SetComponentEnabled(biter.Entity, true);
                    } while (BittenBitterReader.TryGetNextValue(out biter, ref it));

                    BittenEventLookup[bitten] = new BittenEvent {
                        NutrientLoss = biteNutrientLossSum
                    };
                    BittenEventLookup.SetComponentEnabled(bitten, true);
                }
            }
        }

        private struct BiterData
        {
            public Entity Entity;
            public float BitingStrength;
        }
    }
}