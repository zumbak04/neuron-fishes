using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Config
{
    [Serializable]
    public struct ThinkingConfig
    {
        [field: SerializeField]
        public ushort HiddenLayerSize { get; private set; }

        [field: SerializeField]
        public ushort HiddenLayersCount { get; private set; }
    }
}