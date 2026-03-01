using Brain;
using Config;
using Diet;
using IngameDebugConsole;
using JetBrains.Annotations;
using Life;
using Math;
using Move;
using Sight;
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
        public static LifetimeScope Scope;

        [ConsoleMethod("spawn.curiousFish", "Спавнит любопытную рыбу в центре камеры")]
        public static void SpawnCuriousFish()
        {
            var config = WorldUtils.GetSingleton<MainConfig>();

            FixedList32Bytes<ushort> brainLayerSizes = new() {
                ThinkingConsts.INPUT_SIZE,
                ThinkingConsts.OUTPUT_SIZE
            };
            Thinking thinking = new(brainLayerSizes);
            for (var i = 0; i < thinking.Weights.Length; i++) {
                thinking.Weights[i] = (Snorm8)1f;
            }

            Seeing seeing = new() {
                Range = config.Seeing.MaxRange
            };

            Moving moving = new() {
                Acceleration = config.Movement.MaxAcceleration
            };

            Nutritious nutritious = new() {
                Current = DietUtils.CurNutrientsFromLimit(config.Diet.MaxNutrients),
                Limit = config.Diet.MaxNutrients
            };

            Lasting lasting = new() {
                Lifetime = config.Life.MaxLifetime
            };

            Synthesizing synthesizing = new() {
                Strength = config.Diet.Synthesizing.MaxStrength
            };

            Biting biting = new() {
                Strength = config.Diet.Biting.MaxStrength
            };

            UnityEngine.Camera current = UnityEngine.Camera.main;
            float2 position = current!.transform.position.ToFloat2();
            SpawnFishRequest request = new() {
                Count = 1,
                Position = position,
                Thinking = thinking,
                Seeing = seeing,
                Moving = moving,
                Nutritious = nutritious,
                Lasting = lasting,
                Synthesizing = synthesizing,
                Biting = biting
            };
            Resolve<SpawnService>().SpawnFish(request);
            Debug.Log($"Spawned curious fish on position={position}");
        }

        [ConsoleMethod("time.scale", "Изменить Time.timeScale")]
        public static void TimeScale(float value)
        {
            Time.timeScale = value;
        }

        private static T Resolve<T>()
        {
            return Scope.Container.Resolve<T>();
        }
    }
}