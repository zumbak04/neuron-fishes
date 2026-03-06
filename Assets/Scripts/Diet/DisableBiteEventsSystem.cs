using Unity.Burst;
using Unity.Entities;

namespace Diet
{
    [BurstCompile, UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial struct DisableDietEventsSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var evtEnabled in SystemAPI.Query<EnabledRefRW<TookBiteEvent>>()) {
                evtEnabled.ValueRW = false;
            }

            foreach (var (evt, evtEnabled) in SystemAPI.Query<RefRW<BittenEvent>, EnabledRefRW<BittenEvent>>()) {
                evt.ValueRW = default;
                evtEnabled.ValueRW = false;
            }
        }
    }
}