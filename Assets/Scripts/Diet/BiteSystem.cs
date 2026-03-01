using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using World;
using RaycastHit = Unity.Physics.RaycastHit;

namespace Diet
{
    [BurstCompile, UpdateInGroup(typeof(AfterPhysicsSystemGroup))]
    public partial struct BiteSystem : ISystem
    {
        private ComponentLookup<Nutritious> _nutritiousLookup;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            EntityQuery biterWithNutrientsQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Nutritious, Biting>().Build(ref state);
            
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate(biterWithNutrientsQuery);
            
            _nutritiousLookup = state.GetComponentLookup<Nutritious>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
            
            _nutritiousLookup.Update(ref state);
            
            NativeQueue<BiteEvent> eventQueue = new(Allocator.TempJob);

            RegisterBiteEventJob registerBiteEventJob = new() {
                EventWriter = eventQueue.AsParallelWriter(),
                DeltaTime = SystemAPI.Time.DeltaTime,
                CollisionWorld = collisionWorld,
                NutritiousLookup = _nutritiousLookup
            };
            state.Dependency = registerBiteEventJob.ScheduleParallel(state.Dependency);

            ApplyBiteEventJob applyBiteDamageJob = new() {
                EventQueue = eventQueue,
                NutritiousLookup = _nutritiousLookup
            };
            state.Dependency = applyBiteDamageJob.Schedule(state.Dependency);

            eventQueue.Dispose(state.Dependency);
        }
        
        [BurstCompile]
        private partial struct RegisterBiteEventJob : IJobEntity
        {
            public NativeQueue<BiteEvent>.ParallelWriter EventWriter;
            public float DeltaTime;
            [ReadOnly] public CollisionWorld CollisionWorld;
            [ReadOnly] public ComponentLookup<Nutritious> NutritiousLookup;

            private void Execute(Entity entity, in Biting biting, in LocalToWorld ltw)
            {
                RaycastInput input = new() {
                    Start = ltw.Position + ltw.Right * biting.RaycastStart.x + ltw.Up * biting.RaycastStart.y,
                    End = ltw.Position + ltw.Right * biting.RaycastEnd.x + ltw.Up * biting.RaycastEnd.y,
                    Filter = new CollisionFilter {
                        BelongsTo = LayerConsts.FISH_MOUTH,
                        CollidesWith = LayerConsts.FISH_BODY,
                        GroupIndex = 0
                    }
                };

                if (CollisionWorld.CastRay(input, out RaycastHit hit) && NutritiousLookup.HasComponent(hit.Entity)) {
                    EventWriter.Enqueue(new BiteEvent {
                        Biter = entity,
                        Target = hit.Entity,
                        Damage = biting.Strength * DeltaTime
                    });
                }
            }
        }
        
        [BurstCompile]
        private struct ApplyBiteEventJob : IJob
        {
            public NativeQueue<BiteEvent> EventQueue;
            public ComponentLookup<Nutritious> NutritiousLookup;

            public void Execute()
            {
                while (EventQueue.TryDequeue(out BiteEvent biteEvent)) {
                    if (!NutritiousLookup.TryGetComponent(biteEvent.Target, out Nutritious targetNutritious)) {
                        continue;
                    }

                    targetNutritious.Current -= biteEvent.Damage;
                    NutritiousLookup[biteEvent.Target] = targetNutritious;

                    if (!NutritiousLookup.TryGetComponent(biteEvent.Biter, out Nutritious biterNutritious)) {
                        continue;
                    }

                    biterNutritious.Current += biteEvent.Damage;
                    NutritiousLookup[biteEvent.Biter] = biterNutritious;
                }
            }
        }
        
        private struct BiteEvent
        {
            public Entity Biter;
            public Entity Target;
            public float Damage;
        }
    }
}