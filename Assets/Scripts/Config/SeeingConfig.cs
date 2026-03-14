using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Config
{
    [Serializable]
    public struct SeeingConfig
    {
        [field: SerializeField]
        public ushort MinRange { get; private set; }

        [field: SerializeField]
        public ushort MaxRange { get; private set; }
        
        [field: SerializeField, Tooltip("The number of frames over which the sight update is staggered. " +
                                        "Using a power of two (e.g., 2, 4, 8, 16) is recommended for more efficient " +
                                        "modulo (%) operations.")]
        public ushort StaggeringInterval { get; private set; }
    }
}