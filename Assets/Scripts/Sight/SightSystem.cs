using Config;
using Diet;
using Reproduction;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Sight
{
    [BurstCompile]
    public partial struct SightSystem : ISystem
    {
        private EntityQuery _targetQuery;
        private EntityQuery _seeingQuery;

        private EntityTypeHandle _entityHandle;
        private ComponentTypeHandle<Biting> _bitingHandle;
        private ComponentTypeHandle<LocalTransform> _transformHandle;
        private ComponentTypeHandle<Seeing> _seeingHandle;
        private ComponentTypeHandle<Family> _familyHandle;
        private ComponentTypeHandle<SeenEvent> _seenEventHandle;

        private NativeParallelMultiHashMap<uint, SpatialHashItem> _spatialHash;
        private uint _frameCount;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _targetQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<SightTargetTag, LocalTransform>()
                .Build(ref state);
            _seeingQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<LocalTransform, Seeing>()
                .WithDisabled<SeenEvent>()
                .Build(ref state);

            state.RequireForUpdate<MainConfig>();
            state.RequireForUpdate(_targetQuery);
            state.RequireForUpdate(_seeingQuery);

            _entityHandle = state.GetEntityTypeHandle();
            _bitingHandle = state.GetComponentTypeHandle<Biting>(true);
            _transformHandle = state.GetComponentTypeHandle<LocalTransform>(true);
            _seeingHandle = state.GetComponentTypeHandle<Seeing>(true);
            _familyHandle = state.GetComponentTypeHandle<Family>(true);
            _seenEventHandle = state.GetComponentTypeHandle<SeenEvent>();

            _spatialHash = new NativeParallelMultiHashMap<uint, SpatialHashItem>(1024, Allocator.Persistent);
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
            var mainConfig = SystemAPI.GetSingleton<MainConfig>();
            float cellSize = math.max(1f, mainConfig.Seeing.MaxRange);

            // Переиспользуем буфер. Ёмкость подстраиваем по числу целей.
            int targetCount = _targetQuery.CalculateEntityCount();
            if (_spatialHash.Capacity < targetCount * 2) {
                _spatialHash.Capacity = math.max(_spatialHash.Capacity, targetCount * 2);
            }

            _spatialHash.Clear();

            _frameCount++;

            _entityHandle.Update(ref state);
            _bitingHandle.Update(ref state);
            _transformHandle.Update(ref state);
            _seeingHandle.Update(ref state);
            _familyHandle.Update(ref state);
            _seenEventHandle.Update(ref state);

            BuildSpatialHashJob buildSpatialHashJob = new() {
                EntityHandle = _entityHandle,
                BitingHandle = _bitingHandle,
                TransformHandle = _transformHandle,
                FamilyHandle = _familyHandle,
                HashWriter = _spatialHash.AsParallelWriter(),
                CellSize = cellSize
            };

            state.Dependency = buildSpatialHashJob.ScheduleParallel(_targetQuery, state.Dependency);

            TrySeeSpatialTargetsJob trySeeSpatialTargetsJob = new() {
                EntityHandle = _entityHandle,
                BitingHandle = _bitingHandle,
                TransformHandle = _transformHandle,
                SeeingHandle = _seeingHandle,
                FamilyHandle = _familyHandle,
                HashReader = _spatialHash.AsReadOnly(),
                SeenEventHandle = _seenEventHandle,
                CellSize = cellSize,
                FrameCount = _frameCount,
                StaggeringInterval = mainConfig.Seeing.StaggeringInterval
            };

            state.Dependency = trySeeSpatialTargetsJob.ScheduleParallel(_seeingQuery, state.Dependency);
        }

        [BurstCompile]
        private struct BuildSpatialHashJob : IJobChunk
        {
            [ReadOnly] public EntityTypeHandle EntityHandle;
            [ReadOnly] public ComponentTypeHandle<Biting> BitingHandle;
            [ReadOnly] public ComponentTypeHandle<LocalTransform> TransformHandle;
            [ReadOnly] public ComponentTypeHandle<Family> FamilyHandle;
            public NativeParallelMultiHashMap<uint, SpatialHashItem>.ParallelWriter HashWriter;
            public float CellSize;

            public unsafe void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask,
                in v128 chunkEnabledMask)
            {
                Entity* pEntities = chunk.GetEntityDataPtrRO(EntityHandle);
                var pTransforms = (LocalTransform*)chunk.GetRequiredComponentDataPtrRO(ref TransformHandle);
                var pFamilies = (Family*)chunk.GetRequiredComponentDataPtrRO(ref FamilyHandle);
                Biting* pBitings = chunk.GetComponentDataPtrRO(ref BitingHandle);

                ChunkEntityEnumerator entityEnumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);
                while (entityEnumerator.NextEntityIndex(out int i)) {
                    float2 pos = pTransforms[i].Position.xy;
                    var cell = (int2)math.floor(pos / CellSize);
                    uint key = math.hash(cell);
                    float bitingStrength = 0;
                    if (pBitings != null) {
                        bitingStrength = pBitings[i].Strength;
                    }

                    HashWriter.Add(key, new SpatialHashItem {
                        Entity = pEntities[i],
                        Position = pos,
                        BitingStrength = bitingStrength,
                        Family = pFamilies[i]
                    });
                }
            }
        }

        [BurstCompile]
        private struct TrySeeSpatialTargetsJob : IJobChunk
        {
            [ReadOnly] public EntityTypeHandle EntityHandle;
            [ReadOnly] public ComponentTypeHandle<Biting> BitingHandle;
            [ReadOnly] public ComponentTypeHandle<LocalTransform> TransformHandle;
            [ReadOnly] public ComponentTypeHandle<Seeing> SeeingHandle;
            [ReadOnly] public ComponentTypeHandle<Family> FamilyHandle;
            public ComponentTypeHandle<SeenEvent> SeenEventHandle;

            [ReadOnly] public NativeParallelMultiHashMap<uint, SpatialHashItem>.ReadOnly HashReader;
            public float CellSize;
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
                var pTransforms = (LocalTransform*)chunk.GetRequiredComponentDataPtrRO(ref TransformHandle);
                var pSeeing = (Seeing*)chunk.GetRequiredComponentDataPtrRO(ref SeeingHandle);
                var pFamilies = (Family*)chunk.GetRequiredComponentDataPtrRO(ref FamilyHandle);
                Biting* pBitings = chunk.GetComponentDataPtrRO(ref BitingHandle);

                var pSeenEvents = (SeenEvent*)chunk.GetRequiredComponentDataPtrRW(ref SeenEventHandle);

                EnabledMask seenEventEnabledMask = chunk.GetEnabledMask(ref SeenEventHandle);

                ChunkEntityEnumerator entityEnumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);

                while (entityEnumerator.NextEntityIndex(out int i)) {
                    Target biterTarget = new();
                    Target preyTarget = new();
                    Target familyTarget = new();

                    float2 position = pTransforms[i].Position.xy;
                    SelfData selfData = new() {
                        Cell = (int2)math.floor(position / CellSize),
                        Position = position,
                        Entity = pEntities[i],
                        BitingStrength = pBitings != null ? pBitings[i].Strength : 0,
                        SeeingRangeSq = math.lengthsq(pSeeing[i].Range)
                    };

                    for (int dy = -1; dy <= 1; dy++) {
                        for (int dx = -1; dx <= 1; dx++) {
                            ProcessCell(dx, dy, in selfData, in pFamilies[i], ref biterTarget, ref preyTarget,
                                ref familyTarget);
                        }
                    }

                    pSeenEvents[i].ToTargets[0] = biterTarget.To;
                    pSeenEvents[i].ToTargets[1] = preyTarget.To;
                    pSeenEvents[i].ToTargets[2] = familyTarget.To;

                    seenEventEnabledMask[i] = true;
                }
            }

            private void ProcessCell(int dx, int dy, in SelfData selfData, in Family selfFamily, ref Target biterTarget,
                ref Target preyTarget, ref Target familyTarget)
            {
                int2 cell = selfData.Cell + new int2(dx, dy);
                uint key = math.hash(cell);

                if (!HashReader.TryGetFirstValue(key, out SpatialHashItem hashItem,
                        out NativeParallelMultiHashMapIterator<uint> it)) {
                    return;
                }

                do {
                    if (hashItem.Entity == selfData.Entity) {
                        continue;
                    }

                    float2 to = hashItem.Position - selfData.Position;

                    float distanceSq = math.lengthsq(to);
                    if (distanceSq > selfData.SeeingRangeSq) {
                        continue;
                    }

                    if (FamilyUtils.AreRelated(selfData.Entity, hashItem.Entity, in selfFamily, in hashItem.Family)) {
                        familyTarget.TryUpdateNearest(to, distanceSq);
                    }
                    else if (hashItem.BitingStrength > selfData.BitingStrength) {
                        biterTarget.TryUpdateNearest(to, distanceSq);
                    }
                    else {
                        preyTarget.TryUpdateNearest(to, distanceSq);
                    }
                } while (HashReader.TryGetNextValue(out hashItem, ref it));
            }

            private struct Target
            {
                public float2 To { get; private set; }
                private float _distanceSq;

                public Target()
                {
                    To = float2.zero;
                    _distanceSq = float.MaxValue;
                }

                public void TryUpdateNearest(float2 to, float distanceSq)
                {
                    if (distanceSq >= _distanceSq) {
                        return;
                    }

                    To = to;
                    _distanceSq = distanceSq;
                }
            }

            private struct SelfData
            {
                public int2 Cell;
                public float2 Position;
                public Entity Entity;
                public float BitingStrength;
                public float SeeingRangeSq;
            }
        }
        
        private struct SpatialHashItem
        {
            public Entity Entity;
            public float2 Position;
            public float BitingStrength;
            public Family Family;
        }
    }
}