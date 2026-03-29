using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Reproduction
{
    [BurstCompile, UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
    public partial struct FamilyMaskCalculationSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            EntityQuery query = new EntityQueryBuilder(Allocator.Temp).WithDisabled<Family>().Build(ref state);
            state.RequireForUpdate(query);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (family, familyEnabled, entity) in SystemAPI.Query<RefRW<Family>, EnabledRefRW<Family>>().WithDisabled<Family>().WithEntityAccess()) {
                int mask = MixIntoMask(entity, 0);
                for (var i = 0; i < family.ValueRO.Ancestors.Length; i++) {
                    mask = MixIntoMask(family.ValueRO.Ancestors[i], mask);
                }

                family.ValueRW.Mask = mask;
                familyEnabled.ValueRW = true;
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int MixIntoMask(Entity entity, int mask)
        {
            uint entityHash = math.hash(new int2(entity.Index, entity.Version));
            // В индекс берем последние 5 бит из entityHash (число от 0 до 31)
            int bitIndexA = (int)(entityHash & 31);
            // В индекс берем следующие 5 бит из entityHash (число от 0 до 31)
            int bitIndexB = (int)((entityHash >> 5) & 31);
            // Ставим 1 в бит на позиции bitIndexA
            mask |= 1 << bitIndexA;
            // Ставим 1 в бит на позиции bitIndexB
            // С ANCESTORS_DEPTH = 5 в маске будет до 12 бит.
            mask |= 1 << bitIndexB;
            return mask;
        }
    }
}