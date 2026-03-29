using System;
using UnityEngine;

namespace Config
{
    [Serializable]
    public struct DietConfig
    {
        [field: SerializeField]
        public float MinNutrients { get; private set; }

        [field: SerializeField]
        public float MaxNutrients { get; private set; }

        [field: SerializeField]
        public float NutrientDecayPerSecond { get; private set; }

        [field: SerializeField]
        public DietSynthesizingConfig Synthesizing { get; private set; }

        [field: SerializeField]
        public DietBitingConfig Biting { get; private set; }
    }
}