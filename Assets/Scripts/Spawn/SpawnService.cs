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

        public void SpawnFishes(ushort count)
        {
            EntityCommandBuffer ecb = _ecbSystem.CreateCommandBuffer();

            Entity entity = ecb.CreateEntity();
            ecb.AddComponent(entity, new SpawnFishesRequest {
                Count = count
            });
        }

        public void SpawnBiterFish(in SpawnFishBiterRequest request)
        {
            EntityCommandBuffer ecb = _ecbSystem.CreateCommandBuffer();

            Entity entity = ecb.CreateEntity();
            ecb.AddComponent(entity, request);
        }
        
        public void SpawnPlantFish(in SpawnFishPlantRequest request)
        {
            EntityCommandBuffer ecb = _ecbSystem.CreateCommandBuffer();

            Entity entity = ecb.CreateEntity();
            ecb.AddComponent(entity, request);
        }
    }
}