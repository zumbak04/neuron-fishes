using Unity.Entities;
using UnityEngine;

namespace Selection
{
    public class SelectionService
    {
        private readonly EndSimulationEntityCommandBufferSystem _ecbSystem;

        public void Select(Vector3 worldPoint)
        {
            EntityCommandBuffer ecb = _ecbSystem.CreateCommandBuffer();
            Entity entity = ecb.CreateEntity();
            ecb.AddComponent(entity, new SelectRequest {
                WorldPoint = worldPoint
            });
        }
    }
}