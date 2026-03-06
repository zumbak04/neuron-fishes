using Unity.Burst;
using Unity.Entities;

namespace Diet
{
    [BurstCompile, UpdateAfter(typeof(BiteEntitySystem))]
    public partial struct NutrientBiteTransferSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (bittenEvent, nutritious) in SystemAPI
                         .Query<RefRO<BittenEvent>, RefRW<Nutritious>>()) {
                nutritious.ValueRW.Current -= bittenEvent.ValueRO.NutrientLoss;
            }

            foreach (var (tookBiteEvent, nutritious) in SystemAPI
                         .Query<RefRO<TookBiteEvent>, RefRW<Nutritious>>()) {
                nutritious.ValueRW.Current += tookBiteEvent.ValueRO.NutrientGain;
            }
        }
    }
}