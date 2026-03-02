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
            builder.Register<InputService>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();

            builder.Register<GameService>(Lifetime.Scoped).AsSelf().AsImplementedInterfaces();
            builder.Register<SpawnService>(Lifetime.Scoped).AsSelf();
            builder.Register<WorldBoundsService>(Lifetime.Scoped).AsSelf();
            builder.RegisterSystemFromDefaultWorld<EndSimulationEntityCommandBufferSystem>();

            ConsoleCommands.Scope = this;
        }
    }
}