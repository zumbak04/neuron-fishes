using Unity.Burst;
using Unity.Entities;

namespace Receptor
{
    [BurstCompile, UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial struct SeeingDisableEventsSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var triggeredEvent in SystemAPI.Query<EnabledRefRW<SeeingOutputEvent>>()) {
                triggeredEvent.ValueRW = false;
            }
        }
    }
}