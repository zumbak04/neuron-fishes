using Unity.Entities;
using Unity.Physics;

namespace Selection
{
    // todo zumbak
    public partial struct SelectionSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SelectRequest>();
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecbSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            
            // todo zumbak посмотреть видос по ECS. Переделать в SystemBase? Вызывать SystemBase напрямую без SelectionService?
            EntityCommandBuffer ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);

            Entity selectedEntity = Entity.Null;
            foreach ((RefRO<SelectRequest> request, Entity requestEntity) in SystemAPI.Query<RefRO<SelectRequest>>().WithEntityAccess()) {
                RaycastInput raycastInput = new RaycastInput() {
                        Start = request.ValueRO.worldPoint,
                        End = request.ValueRO.worldPoint
                };
                if (physicsWorld.CastRay(raycastInput, out RaycastHit hit)) {
                    // hit.
                }
                ecb.DestroyEntity(requestEntity);
            }
        }
    }
}