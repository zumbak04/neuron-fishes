using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Config
{
    [Serializable]
    public struct LifetimeConfig
    {
        [field: SerializeField]
        public float MinLifetime { get; private set; }

        [field: SerializeField]
        public float MaxLifetime { get; private set; }
    }
}