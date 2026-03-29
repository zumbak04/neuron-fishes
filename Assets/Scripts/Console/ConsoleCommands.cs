using Brain;
using Config;
using Diet;
using IngameDebugConsole;
using JetBrains.Annotations;
using Lifetime;
using Math;
using Move;
using Reproduction;
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

        [ConsoleMethod("spawn.curiousBiterFish", "Спавнит любопытную кусачую рыбу в центре камеры.")]
        public static void SpawnCuriousBiterFish()
        {
            var mainConfig = EcsWorldUtils.GetSingleton<MainConfig>();

            UnityEngine.Camera current = UnityEngine.Camera.main;
            float2 position = current!.transform.position.ToFloat2();
            SpawnFishBiterRequest request = new() {
                Count = 1,
                Position = position,
                Thinking = CreateCuriousThinking(),
                Seeing = CreateBestSeeing(in mainConfig),
                Moving = CreateBestMoving(in mainConfig),
                Nutritious = CreateBestNutritious(in mainConfig),
                Lasting = CreateBestLasting(in mainConfig),
                Reproductive = default,
                Biting = CreateBestBiting(in mainConfig)
            };
            Resolve<SpawnService>().SpawnBiterFish(in request);
            Debug.Log($"Spawned curious biter fish on position={position}");
        }
        
        [ConsoleMethod("spawn.curiousPlantFish", "Спавнит любопытную растительную рыбу в центре камеры.")]
        public static void SpawnCuriousPlantFish()
        {
            var mainConfig = EcsWorldUtils.GetSingleton<MainConfig>();

            UnityEngine.Camera current = UnityEngine.Camera.main;
            float2 position = current!.transform.position.ToFloat2();
            SpawnFishPlantRequest request = new() {
                Count = 1,
                Position = position,
                Thinking = CreateCuriousThinking(),
                Seeing = CreateBestSeeing(in mainConfig),
                Moving = CreateBestMoving(in mainConfig),
                Nutritious = CreateBestNutritious(in mainConfig),
                Lasting = CreateBestLasting(in mainConfig),
                Reproductive = default,
                Synthesizing = CreateBestSynthesizing(in mainConfig)
            };
            Resolve<SpawnService>().SpawnPlantFish(in request);
            Debug.Log($"Spawned curious plant fish on position={position}");
        }
        
        [ConsoleMethod("spawn.tastyFish", "Спавнит вкусную рыбу в центре камеры.")]
        public static void SpawnTastyFish()
        {
            var mainConfig = EcsWorldUtils.GetSingleton<MainConfig>();

            UnityEngine.Camera current = UnityEngine.Camera.main;
            float2 position = current!.transform.position.ToFloat2();
            SpawnFishPlantRequest request = new() {
                Count = 1,
                Position = position,
                Thinking = new Thinking(new FixedList32Bytes<ushort>()),
                Nutritious = new Nutritious {
                    Current = mainConfig.Diet.MaxNutrients - 1,
                    Limit = mainConfig.Diet.MaxNutrients
                },
                Lasting = CreateBestLasting(in mainConfig),
            };
            Resolve<SpawnService>().SpawnPlantFish(in request);
            Debug.Log($"Spawned tasty fish on position={position}");
        }

        [ConsoleMethod("time.scale", "Изменить Time.timeScale.")]
        public static void TimeScale(float value)
        {
            Time.timeScale = value;
        }
        
        [ConsoleMethod("pause", "Ставит мир на паузу (Time.timeScale=0).")]
        public static void TimeScale()
        {
            Time.timeScale = 0;
        }

        private static T Resolve<T>()
        {
            return Scope.Container.Resolve<T>();
        }

        private static Thinking CreateCuriousThinking()
        {
            FixedList32Bytes<ushort> thinkingLayerSizes = new() {
                ThinkingConsts.INPUT_SIZE,
                ThinkingConsts.OUTPUT_SIZE
            };
            Thinking thinking = new(thinkingLayerSizes);
            for (var i = 0; i < thinking.Weights.Length; i++) {
                thinking.Weights[i] = (Snorm8)1f;
            }

            return thinking;
        }

        private static Seeing CreateBestSeeing(in MainConfig mainConfig)
        {
            return new Seeing {
                Range = mainConfig.Seeing.MaxRange
            };
        }
        
        private static Moving CreateBestMoving(in MainConfig mainConfig)
        {
            return new Moving {
                Acceleration = mainConfig.Movement.MaxLinearAcceleration
            };
        }
        
        private static Nutritious CreateBestNutritious(in MainConfig mainConfig)
        {
            return new Nutritious {
                Current = NutritiousUtils.CurrentFromLimit(mainConfig.Diet.MaxNutrients),
                Limit = mainConfig.Diet.MaxNutrients
            };
        }
        
        private static Lasting CreateBestLasting(in MainConfig mainConfig)
        {
            return new Lasting {
                Lifetime = mainConfig.Lifetime.MaxLifetime
            };
        }
        
        private static Biting CreateBestBiting(in MainConfig mainConfig)
        {
            return new Biting {
                Strength = mainConfig.Diet.Biting.MaxStrength
            };
        }
        
        private static Synthesizing CreateBestSynthesizing(in MainConfig mainConfig)
        {
            return new Synthesizing {
                Strength = mainConfig.Diet.Synthesizing.MaxStrength
            };
        }
    }
}