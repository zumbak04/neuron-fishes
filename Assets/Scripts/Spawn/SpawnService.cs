using Unity.Entities;
using VContainer;
using VContainer.Unity;

namespace Spawn
{
    public class SpawnService : IInitializable
    {
        private readonly EndSimulationEntityCommandBufferSystem _ecbSystem;

        [Inject]
        public SpawnService(EndSimulationEntityCommandBufferSystem ecbSystem)
        {
            _ecbSystem = ecbSystem;
        }
        
        // todo zumbak временное решение
        void IInitializable.Initialize()
        {
            SpawnRandomFishes(100);
        }

        public void SpawnRandomFishes(ushort count)
        {
            EntityCommandBuffer ecb = _ecbSystem.CreateCommandBuffer();

            Entity entity = ecb.CreateEntity();
            ecb.AddComponent(entity, new SpawnRandomFishRequest() {
                    count = count
            });
        }

        public void SpawnFish(SpawnFishRequest request)
        {
            EntityCommandBuffer ecb = _ecbSystem.CreateCommandBuffer();
            
            Entity entity = ecb.CreateEntity();
            ecb.AddComponent(entity, request);
        }
    }
}