using System.Runtime.CompilerServices;
using Math;
using Unity.Collections;
using Unity.Mathematics;

namespace Brain
{
    public static class ThinkingUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Thinking Create(ushort hiddenLayersSize, ushort hiddenLayersCount, ref Random random)
        {
            FixedList32Bytes<ushort> layerSizes = new() {
                Length = 2 + hiddenLayersCount,
                [0] = ThinkingConsts.INPUT_SIZE
            };
            layerSizes[^1] = ThinkingConsts.OUTPUT_SIZE;

            for (ushort i = 0; i < hiddenLayersCount; i++) {
                layerSizes[i + 1] = hiddenLayersSize;
            }

            Thinking thinking = new(layerSizes);

            for (var i = 0; i < thinking.Weights.Length; i++) {
                thinking.Weights[i] =
                    (Snorm8) random.NextFloat(ThinkingConsts.MIN_NODE_WEIGHT, ThinkingConsts.MAX_NODE_WEIGHT);
            }

            return thinking;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Thinking Mutate(in Thinking source, float deviation, ref Random random)
        {
            Thinking result = source;
            var weightDelta =
                (Snorm8)((ThinkingConsts.MAX_NODE_WEIGHT - ThinkingConsts.MIN_NODE_WEIGHT) * deviation);
            for (var i = 0; i < result.Weights.Length; i++) {
                bool isPositive = random.NextBool();
                result.Weights[i] += isPositive ? weightDelta : -weightDelta;
            }

            return result;
        }
    }
}