using Math;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Brain
{
    public class ThinkingAuthoring : MonoBehaviour
    {
        private class Baker : Baker<ThinkingAuthoring>
        {
            public override void Bake(ThinkingAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<Thinking>(entity);
                AddComponent(entity, new ThoughOutput {
                    Values = {
                        Length = ThinkingConsts.OUTPUT_SIZE
                    }
                });
            }
        }
    }

    public struct Thinking : IComponentData
    {
        // 15 элементов, 2 байта на каждый ushort + 4 байта на header
        public FixedList32Bytes<ushort> LayerSizes;

        // 508 элементов, 1 байт на каждый Snorm8 + 4 байта на header
        public FixedList512Bytes<Snorm8> Weights;

        // 15 элементов, 2 байта на каждый ushort + 4 байта на header
        private readonly FixedList32Bytes<ushort> _layerOffsets;

        public Thinking(FixedList32Bytes<ushort> layerSizes)
        {
            LayerSizes = layerSizes;
            Weights = new FixedList512Bytes<Snorm8>();
            _layerOffsets = new FixedList32Bytes<ushort>();

            ushort currentOffset = 0;
            for (var i = 0; i < layerSizes.Length - 1; i++) {
                _layerOffsets.Add(currentOffset);
                currentOffset += (ushort)(layerSizes[i] * layerSizes[i + 1]);
            }

            Weights.Length = currentOffset;
        }

        public readonly Snorm8 GetWeight(ushort layer, ushort outputNode, ushort inputNode)
        {
            int index = _layerOffsets[layer] + outputNode * LayerSizes[layer] + inputNode;
            return Weights[index];
        }
    }

    public struct ThoughOutput : IComponentData
    {
        // 7 элементов, 8 битов на каждый float2 + 4 бита на header
        public FixedList64Bytes<float2> Values;

        public float2 AverageValue {
            get {
                float2 sum = float2.zero;
                foreach (float2 value in Values) {
                    sum += value;
                }

                return sum / Values.Length;
            }
        }
    }
}