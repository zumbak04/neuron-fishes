using Unity.Burst;
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
        public void OnUpdate(ref SystemState state)
        {
            foreach ((RefRW<LocalTransform> transform, RefRW<PhysicsVelocity> velocity) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<PhysicsVelocity>>()) {
                transform.ValueRW.Position.z = 0;
                transform.ValueRW.Rotation.value.xy = float2.zero;
                velocity.ValueRW.Linear.z = 0;
                velocity.ValueRW.Angular.xy = float2.zero;
            }
        }
    }
}