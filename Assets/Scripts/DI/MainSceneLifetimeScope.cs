using Console;
using Core;
using Input;
using Spawn;
using Unity.Entities;
using VContainer;
using VContainer.Unity;
using World;

namespace DI
{
    public class MainSceneLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<InputService>(VContainer.Lifetime.Singleton).AsSelf().AsImplementedInterfaces();

            builder.Register<InitService>(VContainer.Lifetime.Scoped).AsSelf().AsImplementedInterfaces();
            builder.Register<SpawnService>(VContainer.Lifetime.Scoped).AsSelf();
            builder.Register<WorldBoundsService>(VContainer.Lifetime.Scoped).AsSelf();
            builder.RegisterSystemFromDefaultWorld<EndSimulationEntityCommandBufferSystem>();

            ConsoleCommands.Scope = this;
        }
    }
}