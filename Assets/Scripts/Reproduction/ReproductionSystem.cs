using Brain;
using Config;
using Diet;
using Lifetime;
using Move;
using Sight;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Reproduction
{
    [BurstCompile, UpdateAfter(typeof(NutrientDecaySystem)), UpdateAfter(typeof(SynthesisSystem)),
     UpdateAfter(typeof(NutrientBiteTransferSystem)), UpdateAfter(typeof(TransformSystemGroup))]
    public partial struct ReproductionSystem : ISystem
    {
        private EntityQuery _query;
        
        private EntityTypeHandle _entityHandle;
        private ComponentTypeHandle<LocalToWorld> _ltwHandle;
        private ComponentTypeHandle<Thinking> _thinkingHandle;
        private ComponentTypeHandle<Seeing> _seeingHandle;
        private ComponentTypeHandle<Moving> _movingHandle;
        private ComponentTypeHandle<Lasting> _lastingHandle;
        private ComponentTypeHandle<Reproductive> _reproductiveHandle;
        private ComponentTypeHandle<Family> _familyHandle;
        private ComponentTypeHandle<Biting> _bitingHandle;
        private ComponentTypeHandle<Synthesizing> _synthesizingHandle;
        private ComponentTypeHandle<Nutritious> _nutritiousHandle;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _query = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<LocalToWorld, Thinking, Seeing, Lasting, Reproductive, Family, Nutritious>()
                .WithAny<Biting, Synthesizing>()
                .Build(ref state);

            state.RequireForUpdate(_query);
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<MainConfig>();

            _entityHandle = state.GetEntityTypeHandle();
            _ltwHandle = state.GetComponentTypeHandle<LocalToWorld>(true);
            _thinkingHandle = state.GetComponentTypeHandle<Thinking>(true);
            _seeingHandle = state.GetComponentTypeHandle<Seeing>(true);
            _movingHandle = state.GetComponentTypeHandle<Moving>(true);
            _lastingHandle = state.GetComponentTypeHandle<Lasting>(true);
            _reproductiveHandle = state.GetComponentTypeHandle<Reproductive>(true);
            _familyHandle = state.GetComponentTypeHandle<Family>(true);
            _bitingHandle = state.GetComponentTypeHandle<Biting>(true);
            _synthesizingHandle = state.GetComponentTypeHandle<Synthesizing>(true);
            _nutritiousHandle = state.GetComponentTypeHandle<Nutritious>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var mainConfig = SystemAPI.GetSingleton<MainConfig>();
            
            _entityHandle.Update(ref state);
            _ltwHandle.Update(ref state);
            _thinkingHandle.Update(ref state);
            _seeingHandle.Update(ref state);
            _movingHandle.Update(ref state);
            _lastingHandle.Update(ref state);
            _reproductiveHandle.Update(ref state);
            _familyHandle.Update(ref state);
            _bitingHandle.Update(ref state);
            _synthesizingHandle.Update(ref state);
            _nutritiousHandle.Update(ref state);

            EntityCommandBuffer ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);
            uint seed = (uint)(SystemAPI.Time.ElapsedTime * 1000) + 1;

            TryReproduceJob tryReproduceJob = new() {
                EntityHandle = _entityHandle,
                LtwHandle = _ltwHandle,
                ThinkingHandle = _thinkingHandle,
                SeeingHandle = _seeingHandle,
                MovingHandle = _movingHandle,
                LastingHandle = _lastingHandle,
                ReproductiveHandle = _reproductiveHandle,
                FamilyHandle = _familyHandle,
                BitingHandle = _bitingHandle,
                SynthesizingHandle = _synthesizingHandle,
                NutritiousHandle = _nutritiousHandle,
                EcbWriter = ecb.AsParallelWriter(),
                FrameSeed = seed,
                MinSeeingRange = mainConfig.Seeing.MinRange,
                MaxSeeingRange = mainConfig.Seeing.MaxRange,
                MinAcceleration = mainConfig.Movement.MinLinearAcceleration,
                MaxAcceleration = mainConfig.Movement.MaxLinearAcceleration,
                MinNutrients = mainConfig.Diet.MinNutrients,
                MaxNutrients = mainConfig.Diet.MaxNutrients,
                MinLifetime = mainConfig.Lifetime.MinLifetime,
                MaxLifetime = mainConfig.Lifetime.MaxLifetime,
                MinBitingStrength = mainConfig.Diet.Biting.MinStrength,
                MaxBitingStrength = mainConfig.Diet.Biting.MaxStrength,
                MinSynthesizingStrength = mainConfig.Diet.Synthesizing.MinStrength,
                MaxSynthesizingStrength = mainConfig.Diet.Synthesizing.MaxStrength,
                MinMutationChance = mainConfig.Reproduction.MinMutationChance,
                MaxMutationChance = mainConfig.Reproduction.MaxMutationChance,
                MinMutationDeviation = mainConfig.Reproduction.MinMutationDeviation,
                MaxMutationDeviation = mainConfig.Reproduction.MaxMutationDeviation
            };

            state.Dependency = tryReproduceJob.ScheduleParallel(_query, state.Dependency);
        }

        [BurstCompile]
        private struct TryReproduceJob : IJobChunk
        {
            [ReadOnly] public EntityTypeHandle EntityHandle;
            
            [ReadOnly] public ComponentTypeHandle<LocalToWorld> LtwHandle;
            [ReadOnly] public ComponentTypeHandle<Thinking> ThinkingHandle;
            [ReadOnly] public ComponentTypeHandle<Seeing> SeeingHandle;
            [ReadOnly] public ComponentTypeHandle<Moving> MovingHandle;
            [ReadOnly] public ComponentTypeHandle<Lasting> LastingHandle;
            [ReadOnly] public ComponentTypeHandle<Reproductive> ReproductiveHandle;
            [ReadOnly] public ComponentTypeHandle<Family> FamilyHandle;
            [ReadOnly] public ComponentTypeHandle<Biting> BitingHandle;
            [ReadOnly] public ComponentTypeHandle<Synthesizing> SynthesizingHandle;
            public ComponentTypeHandle<Nutritious> NutritiousHandle;
            
            public EntityCommandBuffer.ParallelWriter EcbWriter;
            
            public uint FrameSeed;
            
            public float MinSeeingRange;
            public float MaxSeeingRange;
            public float MinAcceleration;
            public float MaxAcceleration;
            public float MinNutrients;
            public float MaxNutrients;
            public float MinLifetime;
            public float MaxLifetime;
            public float MinMutationChance;
            public float MaxMutationChance;
            public float MinMutationDeviation;
            public float MaxMutationDeviation;
            public float MinBitingStrength;
            public float MaxBitingStrength;
            public float MinSynthesizingStrength;
            public float MaxSynthesizingStrength;

            public unsafe void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                Random random = new(FrameSeed + (uint)unfilteredChunkIndex);

                Entity* pEntities = chunk.GetEntityDataPtrRO(EntityHandle);
                var pLtws = (LocalToWorld*)chunk.GetRequiredComponentDataPtrRO(ref LtwHandle);
                var pThinkings = (Thinking*)chunk.GetRequiredComponentDataPtrRO(ref ThinkingHandle);
                var pSeeings = (Seeing*)chunk.GetRequiredComponentDataPtrRO(ref SeeingHandle);
                var pMovings = (Moving*)chunk.GetRequiredComponentDataPtrRO(ref MovingHandle);
                var pLastings = (Lasting*)chunk.GetRequiredComponentDataPtrRO(ref LastingHandle);
                var pReproductive = (Reproductive*)chunk.GetRequiredComponentDataPtrRO(ref ReproductiveHandle);
                var pFamilies = (Family*)chunk.GetRequiredComponentDataPtrRO(ref FamilyHandle);
                Biting* pBitings = chunk.GetComponentDataPtrRO(ref BitingHandle);
                Synthesizing* pSynthesizings = chunk.GetComponentDataPtrRO(ref SynthesizingHandle);
                var pNutritious = (Nutritious*)chunk.GetRequiredComponentDataPtrRW(ref NutritiousHandle);
                
                ChunkEntityEnumerator entityEnumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);
                while (entityEnumerator.NextEntityIndex(out int i)) {
                    float currNutrients = pNutritious[i].Current;
                    if (currNutrients < pNutritious[i].Limit) {
                        continue;
                    }

                    pNutritious[i].Current = currNutrients / 2;

                    int sortKey = i + unfilteredChunkIndex;
                    
                    Entity child = EcbWriter.Instantiate(sortKey, pEntities[i]);
                    EcbWriter.SetComponent(sortKey, child, new LocalTransform {
                        Position = pLtws[i].Position - pLtws[i].Up,
                        Rotation = quaternion.identity,
                        Scale = 1f
                    });

                    float mutationDeviation = pReproductive[i].MutationDeviation;
                    float mutationChance = pReproductive[i].MutationChance;
                    if (random.NextFloat(0, 1) < mutationChance) {
                        EcbWriter.SetComponent(sortKey, child,
                            ThinkingUtils.Mutate(in pThinkings[i], mutationDeviation, ref random));
                    }

                    if (random.NextFloat(0, 1) < mutationChance) {
                        EcbWriter.SetComponent(sortKey, child,
                            SeeingUtils.Mutate(in pSeeings[i], mutationDeviation, MinSeeingRange, MaxSeeingRange, ref random));
                    }

                    if (random.NextFloat(0, 1) < mutationChance) {
                        EcbWriter.SetComponent(sortKey, child,
                            MovingUtils.Mutate(in pMovings[i], mutationDeviation, MinAcceleration, MaxAcceleration, ref random));
                    }

                    if (random.NextFloat(0, 1) < mutationChance) {
                        EcbWriter.SetComponent(sortKey, child,
                            NutritiousUtils.Mutate(in pNutritious[i], mutationDeviation, MinNutrients,
                                MaxNutrients, ref random));
                    }

                    if (random.NextFloat(0, 1) < mutationChance) {
                        EcbWriter.SetComponent(sortKey, child,
                            LastingUtils.Mutate(in pLastings[i], mutationDeviation, MinLifetime,
                                MaxLifetime, ref random));
                    }

                    if (random.NextFloat(0, 1) < mutationChance) {
                        EcbWriter.SetComponent(sortKey, child,
                            ReproductiveUtils.Mutate(in pReproductive[i], MinMutationChance, MaxMutationChance,
                                MinMutationDeviation, MaxMutationDeviation, ref random));
                    }
                    EcbWriter.SetComponent(sortKey, child, FamilyUtils.Create(pEntities[i], in pFamilies[i]));

                    if (pBitings != null && random.NextFloat(0, 1) < mutationChance) {
                        EcbWriter.SetComponent(sortKey, child,
                            BitingUtils.Mutate(in pBitings[i], mutationDeviation, MinBitingStrength,
                                MaxBitingStrength, ref random));
                        
                        // EcbWriter.SetName(sortKey, child, $"Fish_Biter_{child.Index}_{child.Version}");
                    }

                    if (pSynthesizings != null && random.NextFloat(0, 1) < mutationChance) {
                        EcbWriter.SetComponent(sortKey, child,
                            SynthesizingUtils.Mutate(in pSynthesizings[i], mutationDeviation, MinSynthesizingStrength,
                                MaxSynthesizingStrength, ref random));
                        
                        // EcbWriter.SetName(sortKey, child, $"Fish_Plant_{child.Index}_{child.Version}");
                    }
                }
            }
        }
    }
}