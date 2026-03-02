using Unity.Entities;
using VContainer;

namespace Spawn
{
    public class SpawnService
    {
        private readonly EndSimulationEntityCommandBufferSystem _ecbSystem;

        [Inject]
        public SpawnService(EndSimulationEntityCommandBufferSystem ecbSystem)
        {
            _ecbSystem = ecbSystem;
        }

        public void SpawnRandomFishes(ushort count)
        {
            EntityCommandBuffer ecb = _ecbSystem.CreateCommandBuffer();

            Entity entity = ecb.CreateEntity();
            ecb.AddComponent(entity, new SpawnRandomFishRequest {
                Count = count
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