using Config;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace Move
{
    // todo zumbak поворачивать рыб на цель чтобы они не терлись боками друг от друга до смерти
    [BurstCompile, UpdateInGroup(typeof(BeforePhysicsSystemGroup))]
    public partial struct AngularVelocityAlongLinearSystem : ISystem
    {
        private const float LINEAR_SPEED_EPSILON_SQ = 0.1f;
            
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            EntityQuery query = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<PhysicsVelocity, LocalTransform, Moving>().Build(ref state);

            state.RequireForUpdate<MainConfig>();
            state.RequireForUpdate(query);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var config = SystemAPI.GetSingleton<MainConfig>();
            
            CalculateAngularVelocityJob calculateAngularVelocityJob = new() {
                Smoothness = config.Movement.RotationSmoothness,
                MaxAngularVelocity = config.Movement.MaxAngularSpeed
            };

            state.Dependency = calculateAngularVelocityJob.ScheduleParallel(state.Dependency);
        }

        [BurstCompile, WithAll(typeof(Moving))]
        private partial struct CalculateAngularVelocityJob : IJobEntity
        {
            public float Smoothness;
            public float MaxAngularVelocity;

            private void Execute(ref PhysicsVelocity velocity, in LocalTransform transform)
            {
                float2 linearVelocity = velocity.Linear.xy;
                // Если линейная скорость ниже, то мы не трогаем угловую. Лишняя угловая скорость загаситься Damping системой.
                if (math.lengthsq(linearVelocity) < LINEAR_SPEED_EPSILON_SQ) {
                    return;
                }
                
                float targetAngle = math.atan2(velocity.Linear.y, velocity.Linear.x) - math.PIHALF;

                float currentAngle = 2.0f * math.atan2(transform.Rotation.value.z, transform.Rotation.value.w);

                // Может быть от -Pi до Pi
                float angleError = math.atan2(math.sin(targetAngle - currentAngle),
                    math.cos(targetAngle - currentAngle));

                float targetAngularVelocity = -angleError * Smoothness;
                
                // Здесь нет DeltaTime, так как тут нет ускорения.
                // Если не ограничить угловую скорость, то при большом Smoothness рыбы могут толкать друг друга.
                velocity.Angular.z = math.clamp(targetAngularVelocity, -MaxAngularVelocity, MaxAngularVelocity);
            }
        }
    }
}