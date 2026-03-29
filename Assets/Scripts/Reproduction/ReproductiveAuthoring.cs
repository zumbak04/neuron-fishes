using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Reproduction
{
    public class ReproductiveAuthoring : MonoBehaviour
    {
        private class Baker : Baker<ReproductiveAuthoring>
        {
            public override void Bake(ReproductiveAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<Reproductive>(entity);
                AddComponent<Family>(entity);
                SetComponentEnabled<Family>(entity, false);
            }
        }
    }

    public struct Reproductive : IComponentData
    {
        public float MutationChance;
        public float MutationDeviation;
    }
    
    /// <summary>
    /// Компонент родства сущности.
    /// До вычисления <see cref="Mask"/> компонент отключен.
    /// </summary>
    public struct Family : IComponentData, IEnableableComponent
    {
        /// <summary>
        /// Битовая маска родства для bloom-фильтра.
        /// </summary>
        public int Mask;

        /// <summary>
        /// Список предков. Используется для вычисления <see cref="Mask"/> и нахождения родства.
        /// 7 элементов максимум: 8 байт на каждый Entity + 4 байта на header.
        /// </summary>
        public FixedList64Bytes<Entity> Ancestors;
    }
}