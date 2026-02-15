using Config;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;

namespace Move
{
    [BurstCompile, UpdateInGroup(typeof(AfterPhysicsSystemGroup)), UpdateBefore(typeof(MovementSystem))]
    public partial struct DragSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MainConfig>();
            state.RequireForUpdate<Moving>();
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var config = SystemAPI.GetSingleton<MainConfig>();
            float maxSpeedSq = math.lengthsq(config.movement._maxSpeed);

            foreach (var velocity in SystemAPI.Query<RefRW<PhysicsVelocity>>().WithAll<Moving>()) {
                float speedRatio = math.lengthsq(velocity.ValueRO.Linear.xy) / maxSpeedSq;
                float drag = math.lerp(config.movement._minDrag, 1, speedRatio);
                velocity.ValueRW.Linear.xy *= 1 - drag * SystemAPI.Time.DeltaTime;
            }
        }
    }
}