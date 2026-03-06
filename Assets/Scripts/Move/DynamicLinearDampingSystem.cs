using Config;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;

namespace Move
{
    [BurstCompile, UpdateInGroup(typeof(BeforePhysicsSystemGroup))]
    public partial struct DynamicLinearDampingSystem : ISystem
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

            CalculateDampingJob calculateDampingJob = new() {
                MaxSpeedSq = maxSpeedSq,
                MinDamping = config.Movement.MinDamping
            };

            state.Dependency = calculateDampingJob.ScheduleParallel(state.Dependency);
        }
        
        [BurstCompile, WithAll(typeof(Moving))]
        private partial struct CalculateDampingJob : IJobEntity
        {
            public float MaxSpeedSq;
            public float MinDamping;

            private void Execute(ref PhysicsDamping damping, in PhysicsVelocity velocity)
            {
                float speedRatio = math.lengthsq(velocity.Linear.xy) / MaxSpeedSq;
                float linearDamping = math.lerp(MinDamping, 1, math.saturate(speedRatio));
                damping.Linear = linearDamping;
            }
        }
    }
}