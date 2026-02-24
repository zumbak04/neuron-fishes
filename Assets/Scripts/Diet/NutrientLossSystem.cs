using Config;
using Unity.Burst;
using Unity.Entities;

namespace Diet
{
    [BurstCompile]
    public partial struct NutrientLossSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<MainConfig>();
            state.RequireForUpdate<Nutritious>();
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var config = SystemAPI.GetSingleton<MainConfig>();
            var ecbSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            
            EntityCommandBuffer ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);
            
            // Synthesizing по дефолту не теряют nutrients
            foreach (var (health, entity) in SystemAPI.Query<RefRW<Nutritious>>().WithNone<Synthesizing>().WithEntityAccess()) {
                health.ValueRW.current -= config.diet._nutrientLossPerSecond * SystemAPI.Time.DeltaTime;
                
                if (health.ValueRO.current <= 0) {
                    ecb.DestroyEntity(entity);
                }
            }
        }
    }
}