using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Diet
{
    [BurstCompile]
    public partial struct SynthesisSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            EntityQuery query = new EntityQueryBuilder(Allocator.Temp).WithAll<Nutritious, Synthesizing>()
                .Build(ref state);
            state.RequireForUpdate(query);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (nutritious, synthesizing) in SystemAPI.Query<RefRW<Nutritious>, RefRO<Synthesizing>>()) {
                nutritious.ValueRW.Current =
                    math.min(nutritious.ValueRO.Current + synthesizing.ValueRO.Strength * SystemAPI.Time.DeltaTime,
                        nutritious.ValueRO.Limit);
            }
        }
    }
}