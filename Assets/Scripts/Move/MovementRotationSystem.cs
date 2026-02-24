using Config;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace Move
{
    [BurstCompile, UpdateInGroup(typeof(AfterPhysicsSystemGroup)), UpdateAfter(typeof(MovementSystem))]
    public partial struct MovementRotationSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Moving>();
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach ((RefRW<LocalTransform> transform, RefRO<PhysicsVelocity> velocity) in
                     SystemAPI.Query<RefRW<LocalTransform>, RefRO<PhysicsVelocity>>().WithAll<Moving>()) {
                float zRotation = math.atan2(velocity.ValueRO.Linear.y, velocity.ValueRO.Linear.x) - math.PIHALF;
                transform.ValueRW.Rotation = quaternion.RotateZ(zRotation);
            }
        }
    }
}