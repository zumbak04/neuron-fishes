using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace Move
{
    [BurstCompile, UpdateInGroup(typeof(AfterPhysicsSystemGroup), OrderFirst = true)]
    public partial struct StickTo2DSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            EntityQuery query = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<LocalTransform, PhysicsVelocity, Moving>().Build(ref state);
            state.RequireForUpdate(query);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            StickTo2DJob stickTo2DJob = new();

            state.Dependency = stickTo2DJob.ScheduleParallel(state.Dependency);
        }

        [BurstCompile, WithAll(typeof(Moving))]
        private partial struct StickTo2DJob : IJobEntity
        {
            private void Execute(ref LocalTransform transform, ref PhysicsVelocity velocity)
            {
                transform.Position.z = 0;
                transform.Rotation.value.xy = float2.zero;
                velocity.Linear.z = 0;
                velocity.Angular.xy = float2.zero;
            }
        }
    }
}