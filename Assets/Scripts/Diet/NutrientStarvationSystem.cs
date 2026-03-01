using Unity.Burst;
using Unity.Entities;

namespace Diet
{
    [BurstCompile, UpdateAfter(typeof(NutrientLossSystem)), UpdateAfter(typeof(BiteSystem)),
     UpdateAfter(typeof(SynthesisSystem))]
    public partial struct NutrientStarvationSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<Nutritious>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();

            EntityCommandBuffer ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var (nutritious, entity) in SystemAPI.Query<RefRO<Nutritious>>().WithEntityAccess()) {
                if (nutritious.ValueRO.Current <= 0) {
                    ecb.DestroyEntity(entity);
                }
            }
        }
    }
}