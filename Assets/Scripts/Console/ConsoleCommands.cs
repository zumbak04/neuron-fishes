using Brain;
using Config;
using Diet;
using IngameDebugConsole;
using JetBrains.Annotations;
using Life;
using Math;
using Move;
using Receptor;
using Spawn;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using World;

namespace Console
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public static class ConsoleCommands
    {
        public static LifetimeScope scope;

        [ConsoleMethod("spawn.curiousFish", "Спавнит любопытную рыбу в центре камеры")]
        public static void SpawnCuriousFish()
        {
            MainConfig config = WorldUtils.GetSingleton<MainConfig>();

            FixedList32Bytes<ushort> brainLayerSizes = new() {
                    ThinkingConsts.INPUT_SIZE,
                    ThinkingConsts.OUTPUT_SIZE
            };
            Thinking thinking = new(brainLayerSizes);
            for (int i = 0; i < thinking.weights.Length; i++) {
                thinking.weights[i] = (Snorm8) 1f;
            }

            Seeing seeing = new() {
                    range = config.seeing._maxRange
            };

            Moving moving = new() {
                    acceleration = config.movement._maxAcceleration
            };

            Nutritious nutritious = new() {
                    current = DietUtils.CurNutrientsFromLimit(config.diet._maxNutrients)
            };
            
            Lasting lasting = new() {
                    lifetime = config.life._maxLifetime
            };
            
            Synthesizing synthesizing = new() {
                    strength = config.diet._synthesizing._maxStrength
            };

            UnityEngine.Camera current = UnityEngine.Camera.main;
            float2 position = current!.transform.position.ToFloat2();
            SpawnFishRequest request = new() {
                    count = 1,
                    position = position,
                    thinking = thinking,
                    seeing = seeing,
                    moving = moving,
                    nutritious = nutritious,
                    lasting = lasting,
                    synthesizing = synthesizing
            };
            Resolve<SpawnService>().SpawnFish(request);
            Debug.Log($"Spawned curious fish on position={position}");
        }

        [ConsoleMethod("time.scale", "Изменить Time.timeScale")]
        public static void TimeScale(float value)
        {
            Time.timeScale = value;
        }

        private static T Resolve<T>() => scope.Container.Resolve<T>();
    }
}