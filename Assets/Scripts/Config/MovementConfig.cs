using System;
using UnityEngine;

namespace Config
{
    [Serializable]
    public struct MovementConfig
    {
        [field: SerializeField] public float MinAcceleration { get; private set; }

        [field: SerializeField] public float MaxAcceleration { get; private set; }

        [field: SerializeField] public float MaxSpeed { get; private set; }

        [field: SerializeField] public float MinDrag { get; private set; }

        [field: SerializeField, Header("Rotation"), Tooltip("The coefficient of rotation smoothness along the linear velocity vector of the fish.")] 
        public float RotationSmoothness { get; private set; }
        [field: SerializeField, Range(3.14f, 6.28f)] 
        public float MaxAngularSpeed { get; private set; }
    }
}