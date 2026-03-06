using Unity.Burst;
using Unity.Entities;

namespace Diet
{
    [BurstCompile]
    public partial struct BiteCooldownSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (bitingCooldown, bitingCooldownEnabled) in SystemAPI
                         .Query<RefRW<BitingCooldown>, EnabledRefRW<BitingCooldown>>()) {
                if (bitingCooldown.ValueRO.Value > 0) {
                    bitingCooldown.ValueRW.Value -= SystemAPI.Time.DeltaTime;
                }
                else {
                    bitingCooldownEnabled.ValueRW = false;
                }
            }
        }
    }
}