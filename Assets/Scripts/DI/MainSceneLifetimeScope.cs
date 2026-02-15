using Console;
using Input;
using Spawn;
using Unity.Entities;
using VContainer;
using VContainer.Unity;

namespace DI
{
    public class MainSceneLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<InputService>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            
            builder.Register<SpawnService>(Lifetime.Scoped).AsSelf().AsImplementedInterfaces();
            builder.RegisterSystemFromDefaultWorld<EndSimulationEntityCommandBufferSystem>();
            
            ConsoleCommands.scope = this;
        }
    }
}