using System;
using UnityEngine;

namespace Reproduction
{
    [Serializable]
    public struct ReproductionConfig
    {
        [field: SerializeField, Range(0, 1)] public float MinMutationChance;
        [field: SerializeField, Range(0, 1)] public float MaxMutationChance;
        [field: SerializeField, Range(0, 1)] public float MinMutationDeviation;
        [field: SerializeField, Range(0, 1)] public float MaxMutationDeviation;
    }
}