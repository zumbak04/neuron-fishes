using Unity.Entities;

namespace World
{
    public static class EcsWorldUtils
    {
        public static T GetSingleton<T>() where T : unmanaged, IComponentData
        {
            EntityManager em = Unity.Entities.World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityQuery query = em.CreateEntityQuery(ComponentType.ReadOnly<T>());
            return query.GetSingleton<T>();
        }
    }
}