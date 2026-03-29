using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Entities;

namespace Reproduction
{
    public static class FamilyUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Family Create()
        {
            return new Family {
                // Маску вычисляем в следующем кадре.
                Mask = int.MaxValue,
                Ancestors = new FixedList64Bytes<Entity>()
            };
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Family Create(Entity parentEntity, in Family parentFamily)
        {
            Family result = new() {
                // Маску вычисляем в следующем кадре.
                Mask = int.MaxValue,
                Ancestors = new FixedList64Bytes<Entity>()
            };
            result.Ancestors.Add(parentEntity);
            for (var i = 0; i < parentFamily.Ancestors.Length && i < FamilyConsts.ANCESTORS_DEPTH - 1; i++) {
                result.Ancestors.Add(parentFamily.Ancestors[i]);
            }
            
            return result;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AreRelated(Entity entityA, Entity entityB, in Family familyA, in Family familyB)
        {
            if ((familyA.Mask & familyB.Mask) == 0) {
                return false;
            }

            if (entityA == entityB) {
                return true;
            }
            
            for (var i = 0; i < familyA.Ancestors.Length; i++) {
                Entity ancestorA = familyA.Ancestors[i];
                if (ancestorA == entityB) {
                    return true;
                }
                for (var j = 0; j < familyB.Ancestors.Length; j++) {
                    Entity ancestorB = familyB.Ancestors[j];
                    if (ancestorA == ancestorB) {
                        return true;
                    }
                }
            }
            
            for (int i = 0; i < familyB.Ancestors.Length; i++) {
                Entity ancestorB = familyB.Ancestors[i];
                if (ancestorB == entityA) {
                    return true;
                }
            }

            return false;
        }
    }
}