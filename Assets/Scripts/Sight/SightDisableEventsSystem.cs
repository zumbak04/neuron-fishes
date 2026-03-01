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
            foreach (EnabledRefRW<SightOutputEvent> triggeredEvent in
                     SystemAPI.Query<EnabledRefRW<SightOutputEvent>>()) {
                triggeredEvent.ValueRW = false;
            }
        }
    }
}