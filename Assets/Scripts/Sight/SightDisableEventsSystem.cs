using Unity.Burst;
using Unity.Entities;

namespace Sight
{
    [BurstCompile, UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial struct SightDisableEventsSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var seenEventEnabled in SystemAPI.Query<EnabledRefRW<SeenEvent>>()) {
                seenEventEnabled.ValueRW = false;
            }
        }
    }
}