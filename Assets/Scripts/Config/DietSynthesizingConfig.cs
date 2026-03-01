using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Config
{
    [Serializable]
    public struct DietSynthesizingConfig
    {
        [field: SerializeField]
        public float MinStrength { get; private set; }

        [field: SerializeField]
        public float MaxStrength { get; private set; }
    }
}