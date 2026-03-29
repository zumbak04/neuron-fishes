using Brain;
using Config;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

namespace Move
{
    [BurstCompile, UpdateAfter(typeof(ThinkingSystem))]
    public partial struct LinearVelocityByThoughSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            EntityQuery query = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<PhysicsVelocity, Moving, ThoughOutput>().Build(ref state);
            state.RequireForUpdate(query);
            state.RequireForUpdate<MainConfig>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var mainConfig = SystemAPI.GetSingleton<MainConfig>();
            
            CalculateLinearVelocityJob calculateLinearVelocityJob = new() {
                DeltaTime = SystemAPI.Time.DeltaTime,
                MisalignedDamping = mainConfig.Movement.MisalignedLinearDamping
            };
            
            state.Dependency = calculateLinearVelocityJob.ScheduleParallel(state.Dependency);
        }
        
        [BurstCompile]
        private partial struct CalculateLinearVelocityJob : IJobEntity
        {
            public float DeltaTime;
            public float MisalignedDamping;
            
            private void Execute(ref PhysicsVelocity velocity, in Moving moving, in ThoughOutput thoughOutput)
            {
                float2 linearVelocity = velocity.Linear.xy;
                float2 direction = thoughOutput.Direction;
                
                // Разбиваем скорость на сонаправленную мысли и не сонаправленную.
                float alignedSpeed = math.max(0f, math.dot(linearVelocity, direction));
                float2 alignedVelocity = direction * alignedSpeed;
                float2 misalignedVelocity = linearVelocity - alignedVelocity;
                
                // Сохраняем сонаправленную скорость. Остальную гасим на MisalignedDamping.
                linearVelocity = alignedVelocity + misalignedVelocity * (1 - MisalignedDamping * DeltaTime);

                linearVelocity += direction * moving.Acceleration * DeltaTime;
                velocity.Linear.xy = linearVelocity;
            }
        }
    }
}