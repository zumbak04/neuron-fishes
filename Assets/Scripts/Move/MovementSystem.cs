using Brain;
using Config;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
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
            state.RequireForUpdate<Moving>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (velocity, moving, thoughOutput) in SystemAPI.Query<RefRW<PhysicsVelocity>, RefRO<Moving>, RefRO<ThoughOutput>>()) {
                velocity.ValueRW.Linear.xy += thoughOutput.ValueRO.AverageValue * moving.ValueRO.acceleration * SystemAPI.Time.DeltaTime;
            }
        }
    }
}