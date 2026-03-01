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
            state.RequireForUpdate<MainConfig>();
            state.RequireForUpdate<Nutritious>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var config = SystemAPI.GetSingleton<MainConfig>();

            // Synthesizing по дефолту не теряют nutrients
            foreach (var nutritious in SystemAPI.Query<RefRW<Nutritious>>().WithNone<Synthesizing>()) {
                nutritious.ValueRW.Current -= config.Diet.NutrientLossPerSecond * SystemAPI.Time.DeltaTime;
            }
        }
    }
}