using System;
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
    /// <remarks>
    /// Система без компонентов-событий.
    /// Значительной производительности не дала, а код в разы сложнее. Отложил на будущее.
    /// </remarks>
    [BurstCompile, UpdateAfter(typeof(TransformSystemGroup)), Obsolete]
    public partial struct BiteChunkSystem : ISystem
    {
        private EntityQuery _nutritiousBitingQuery;

        private ComponentTypeHandle<Biting> _bitingHandle;
        private ComponentTypeHandle<LocalToWorld> _ltwHandle;
        private EntityTypeHandle _entityHandle;
        private ComponentLookup<Nutritious> _nutritiousLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.Enabled = false;
            
            _nutritiousBitingQuery =
                new EntityQueryBuilder(Allocator.Temp).WithAll<Nutritious, Biting>().Build(ref state);

            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate(_nutritiousBitingQuery);

            _bitingHandle = state.GetComponentTypeHandle<Biting>(true);
            _ltwHandle = state.GetComponentTypeHandle<LocalToWorld>(true);
            _entityHandle = state.GetEntityTypeHandle();
            _nutritiousLookup = state.GetComponentLookup<Nutritious>();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            _nutritiousBitingQuery.Dispose();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            int chunkCount = _nutritiousBitingQuery.CalculateChunkCount();
            if (chunkCount == 0) {
                return;
            }

            var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;

            _bitingHandle.Update(ref state);
            _ltwHandle.Update(ref state);
            _entityHandle.Update(ref state);
            _nutritiousLookup.Update(ref state);
            
            NativeStream eventStream = new(chunkCount, Allocator.TempJob);

            RegisterBiteEventJob registerBiteEventJob = new() {
                EntityHandle = _entityHandle,
                BitingHandle = _bitingHandle,
                CollisionWorld = collisionWorld,
                DeltaTime = SystemAPI.Time.DeltaTime,
                LtwHandle = _ltwHandle,
                StreamWriter = eventStream.AsWriter()
            };
            state.Dependency = registerBiteEventJob.ScheduleParallel(_nutritiousBitingQuery, state.Dependency);

            ApplyBiteEventJob applyBiteEventJob = new() {
                StreamReader = eventStream.AsReader(),
                NutritiousLookup = _nutritiousLookup
            };
            state.Dependency = applyBiteEventJob.Schedule(chunkCount, 64, state.Dependency);

            state.Dependency = eventStream.Dispose(state.Dependency);
        }

        [BurstCompile]
        private struct RegisterBiteEventJob : IJobChunk
        {
            [ReadOnly] public EntityTypeHandle EntityHandle;
            [ReadOnly] public ComponentTypeHandle<Biting> BitingHandle;
            [ReadOnly] public ComponentTypeHandle<LocalToWorld> LtwHandle;
            [ReadOnly] public CollisionWorld CollisionWorld;
            public float DeltaTime;
            public NativeStream.Writer StreamWriter;

            public unsafe void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask,
                in v128 chunkEnabledMask)
            {
                NativeArray<Entity> entities = chunk.GetNativeArray(EntityHandle);
                var pBitings = (Biting*)chunk.GetNativeArray(ref BitingHandle).GetUnsafeReadOnlyPtr();
                var pLtws = (LocalToWorld*)chunk.GetNativeArray(ref LtwHandle).GetUnsafeReadOnlyPtr();

                StreamWriter.BeginForEachIndex(unfilteredChunkIndex);
                for (var i = 0; i < chunk.Count; i++) {
                    if (CastAllRays(in pBitings[i].Rays, in pLtws[i], out RaycastHit firstClosestHit)) {
                        StreamWriter.Write(new BiteEvent() {
                            Biter = entities[i],
                            Target = firstClosestHit.Entity,
                            Damage = pBitings[i].Strength * DeltaTime
                        });
                    }
                }

                StreamWriter.EndForEachIndex();
            }
            
            private bool CastAllRays(in FixedList64Bytes<BitingRay> rays, in LocalToWorld ltw,
                out RaycastHit firstClosestHit)
            {
                for (var i = 0; i < rays.Length; i++) {
                    RaycastInput input = new() {
                        Start = ltw.Position + ltw.Right * rays[i].Start.x + ltw.Up * rays[i].Start.y,
                        End = ltw.Position + ltw.Right * rays[i].End.x + ltw.Up * rays[i].End.y,
                        Filter = new CollisionFilter {
                            BelongsTo = LayerConsts.FISH_MOUTH,
                            // Все сущности на слое обязаны иметь Nutritious компонент
                            CollidesWith = LayerConsts.FISH_BODY,
                            GroupIndex = 0
                        }
                    };
                    if (!CollisionWorld.CastRay(input, out RaycastHit closestHit)) {
                        continue;
                    }

                    firstClosestHit = closestHit;
                    return true;
                }

                firstClosestHit = new RaycastHit();
                return false;
            }
        }

        [BurstCompile]
        private struct ApplyBiteEventJob : IJobParallelFor
        {
            [ReadOnly] 
            public NativeStream.Reader StreamReader;
            [NativeDisableParallelForRestriction]
            public ComponentLookup<Nutritious> NutritiousLookup;

            public void Execute(int index)
            {
                int count = StreamReader.BeginForEachIndex(index);

                for (int i = 0; i < count; i++) {
                    BiteEvent evt = StreamReader.Read<BiteEvent>();

                    if (!NutritiousLookup.TryGetComponent(evt.Target, out Nutritious targetNutritious)) {
                        continue;
                    }

                    targetNutritious.Current -= evt.Damage;
                    NutritiousLookup[evt.Target] = targetNutritious;

                    if (!NutritiousLookup.TryGetComponent(evt.Biter, out Nutritious biterNutritious)) {
                        continue;
                    }

                    biterNutritious.Current += evt.Damage;
                    NutritiousLookup[evt.Biter] = biterNutritious;
                }

                StreamReader.EndForEachIndex();
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