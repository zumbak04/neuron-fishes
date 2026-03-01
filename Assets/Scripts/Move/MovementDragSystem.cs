using Config;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;

namespace Move
{
    [BurstCompile, UpdateInGroup(typeof(AfterPhysicsSystemGroup)), UpdateBefore(typeof(MovementSystem))]
    public partial struct MovementDragSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MainConfig>();
            EntityQuery query = new EntityQueryBuilder(Allocator.Temp).WithAll<PhysicsVelocity, Moving>()
                .Build(ref state);
            state.RequireForUpdate(query);
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var config = SystemAPI.GetSingleton<MainConfig>();
            float maxSpeedSq = math.lengthsq(config.Movement.MaxSpeed);

            ApplyDragJob applyDragJob = new() {
                MaxSpeedSq = maxSpeedSq,
                MinDrag = config.Movement.MinDrag,
                DeltaTime = SystemAPI.Time.DeltaTime
            };

            state.Dependency = applyDragJob.ScheduleParallel(state.Dependency);
        }

        [BurstCompile, WithAll(typeof(Moving))]
        private partial struct ApplyDragJob : IJobEntity
        {
            public float MaxSpeedSq;
            public float MinDrag;
            public float DeltaTime;

            private void Execute(ref PhysicsVelocity velocity)
            {
                float speedRatio = math.lengthsq(velocity.Linear.xy) / MaxSpeedSq;
                float drag = math.lerp(MinDrag, 1, math.saturate(speedRatio));
                velocity.Linear.xy *= math.exp(- drag * DeltaTime);
            }
        }
    }
}