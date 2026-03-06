using Brain;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;

namespace Move
{
    [BurstCompile, UpdateAfter(typeof(ThinkingSystem))]
    public partial struct MovementByThoughSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            EntityQuery query = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<PhysicsVelocity, Moving, ThoughOutput>().Build(ref state);
            state.RequireForUpdate(query);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            CalculateVelocityByThoughJob calculateVelocityByThoughJob = new() {
                DeltaTime = SystemAPI.Time.DeltaTime
            };
            
            state.Dependency = calculateVelocityByThoughJob.ScheduleParallel(state.Dependency);
        }
        
        [BurstCompile]
        private partial struct CalculateVelocityByThoughJob : IJobEntity
        {
            public float DeltaTime;
            
            private void Execute(ref PhysicsVelocity velocity, in Moving moving, in ThoughOutput thoughOutput)
            {
                velocity.Linear.xy += thoughOutput.AverageValue * moving.Acceleration * DeltaTime;
            }
        }
    }
}