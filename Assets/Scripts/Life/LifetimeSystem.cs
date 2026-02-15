using Unity.Burst;
using Unity.Entities;

namespace Life
{
    [BurstCompile]
    public partial struct LifetimeSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<Lasting>();
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            
            EntityCommandBuffer ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);
            
            foreach (var (lasting, entity) in SystemAPI.Query<RefRW<Lasting>>().WithEntityAccess()) {
                lasting.ValueRW.lifetime -= SystemAPI.Time.DeltaTime;
                
                if (lasting.ValueRO.lifetime <= 0) {
                    ecb.DestroyEntity(entity);
                }
            }
        }
    }
}