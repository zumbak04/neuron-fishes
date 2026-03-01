using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Config
{
    [Serializable]
    public struct SeeingConfig
    {
        [field: SerializeField]
        public float Cooldown { get; private set; }

        [field: SerializeField]
        public ushort MinRange { get; private set; }

        [field: SerializeField]
        public ushort MaxRange { get; private set; }
    }
}