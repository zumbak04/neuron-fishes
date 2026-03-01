using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Config
{
    [Serializable]
    public struct WorldConfig
    {
        [field: SerializeField]
        public int2 Bounds { get; private set; }

        [field: SerializeField]
        public bool ImpassibleBounds { get; private set; }
    }
}