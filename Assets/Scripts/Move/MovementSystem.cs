using Brain;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;

namespace Move
{
    [BurstCompile, UpdateInGroup(typeof(AfterPhysicsSystemGroup))]
    public partial struct MovementSystem : ISystem
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
            MoveByThoughJob moveByThoughJob = new() {
                DeltaTime = SystemAPI.Time.DeltaTime
            };
            
            state.Dependency = moveByThoughJob.ScheduleParallel(state.Dependency);
        }
        
        [BurstCompile]
        private partial struct MoveByThoughJob : IJobEntity
        {
            public float DeltaTime;
            
            private void Execute(ref PhysicsVelocity velocity, in Moving moving, in ThoughOutput thoughOutput)
            {
                velocity.Linear.xy += thoughOutput.AverageValue * moving.Acceleration * DeltaTime;
            }
        }
    }
}