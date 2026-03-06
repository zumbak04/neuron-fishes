using Config;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Transforms;
using World;
using RaycastHit = Unity.Physics.RaycastHit;

namespace Diet
{
    [BurstCompile, UpdateAfter(typeof(TransformSystemGroup)), UpdateAfter(typeof(BiteCooldownSystem))]
    public partial struct BiteEntitySystem : ISystem
    {
        private ComponentLookup<BittenEvent> _bittenEventLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            EntityQuery biterQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<LocalToWorld, Biting>()
                .WithDisabled<TookBiteEvent, BitingCooldown>().Build(ref state);

            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate(biterQuery);
            state.RequireForUpdate<BittenEvent>();
            state.RequireForUpdate<MainConfig>();

            _bittenEventLookup = state.GetComponentLookup<BittenEvent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var mainConfig = SystemAPI.GetSingleton<MainConfig>();
            var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;

            _bittenEventLookup.Update(ref state);

            NativeQueue<BittenData> bittenQueue = new(Allocator.TempJob);

            TryTakeBiteJob tryTakeBiteJob = new() {
                CollisionWorld = collisionWorld,
                CollisionFilter = new CollisionFilter {
                    BelongsTo = LayerConsts.FISH_MOUTH,
                    // Все сущности на слое обязаны иметь BittenEvent компонент чтобы не проверять через Lookup
                    CollidesWith = LayerConsts.FISH_BODY,
                    GroupIndex = 0
                },
                BittenWriter = bittenQueue.AsParallelWriter(),
                Cooldown = mainConfig.Diet.Biting.Cooldown
            };
            state.Dependency = tryTakeBiteJob.ScheduleParallel(state.Dependency);

            SetBittenEventsJob setBittenEventsJob = new() {
                BittenQueue = bittenQueue,
                BittenEventLookup = _bittenEventLookup
            };

            state.Dependency = setBittenEventsJob.Schedule(state.Dependency);

            state.Dependency = bittenQueue.Dispose(state.Dependency);
        }

        [BurstCompile, WithDisabled(typeof(TookBiteEvent), typeof(BitingCooldown))]
        private partial struct TryTakeBiteJob : IJobEntity
        {
            [ReadOnly] public CollisionWorld CollisionWorld;
            [ReadOnly] public CollisionFilter CollisionFilter;
            public NativeQueue<BittenData>.ParallelWriter BittenWriter;
            public float Cooldown;

            private void Execute(Entity entity, in LocalToWorld ltw, ref Biting biting, ref TookBiteEvent tookBiteEvent,
                ref BitingCooldown bitingCooldown, EnabledRefRW<TookBiteEvent> tookBiteEventEnabled,
                EnabledRefRW<BitingCooldown> bitingCooldownEnabled)
            {
                if (!CastAllRays(entity, in biting.Rays, in ltw, out RaycastHit firstClosestHit)) {
                    return;
                }

                float nutrientDelta = biting.Strength;

                tookBiteEventEnabled.ValueRW = true;
                tookBiteEvent.NutrientGain = nutrientDelta;

                bitingCooldown.Value = Cooldown;
                bitingCooldownEnabled.ValueRW = true;
                
                BittenWriter.Enqueue(new BittenData {
                    Entity = firstClosestHit.Entity,
                    NutrientLoss = nutrientDelta
                });
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
            public NativeQueue<BittenData> BittenQueue;
            public ComponentLookup<BittenEvent> BittenEventLookup;

            public void Execute()
            {
                while (BittenQueue.TryDequeue(out BittenData data)) {
                    if (!BittenEventLookup.TryGetComponent(data.Entity, out BittenEvent bittenEvent)) {
                        continue;
                    }

                    bittenEvent.NutrientLoss += data.NutrientLoss;
                    BittenEventLookup.SetComponentEnabled(data.Entity, true);
                    BittenEventLookup[data.Entity] = bittenEvent;
                }
            }
        }

        private struct BittenData
        {
            public Entity Entity;
            public float NutrientLoss;
        }
    }
}