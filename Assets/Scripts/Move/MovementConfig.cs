using System;
using UnityEngine;

namespace Move
{
    [Serializable]
    public struct MovementConfig
    {
        [field: SerializeField] public float MinLinearAcceleration { get; private set; }
        [field: SerializeField] public float MaxLinearAcceleration { get; private set; }
        [field: SerializeField, Tooltip("Максимальная линейная скорость в водной среде. " +
                                        "При приближении к этому значению затухание будет увеличиваться до 1.")] 
        public float MaxLinearSpeed { get; private set; }
        [field: SerializeField, Range(0, 1), Tooltip("Минимальное затухание линейной скорости из-за сопротивления в водной среде.")] 
        public float MinLinearDamping { get; private set; }
        [field: SerializeField, Range(0, 1), Tooltip("Коэффициент затухания линейной скорости не сонаправленной с целью рыбы.")]
        public float MisalignedLinearDamping { get; private set; }

        [field: SerializeField, Header("Rotation"), Tooltip("Коэффициент гладкости поворота в сторону цели рыбы.")] 
        public float RotationSmoothness { get; private set; }
        [field: SerializeField, Range(3.14f, 6.28f)] 
        public float MaxAngularSpeed { get; private set; }
    }
}