using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Config
{
    [Serializable]
    public struct DietBitingConfig
    {
        [field: SerializeField]
        public float MinStrength { get; private set; }

        [field: SerializeField]
        public float MaxStrength { get; private set; }

        [field: SerializeField] public float Cooldown { get; private set; }
        
        [field: SerializeField, Tooltip("The number of frames over which the bite update is staggered. " +
                                        "Using a power of two (e.g., 2, 4, 8, 16) is recommended for more efficient " +
                                        "modulo (%) operations.")]
        public ushort StaggeringInterval { get; private set; }
    }
}