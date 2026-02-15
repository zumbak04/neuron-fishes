using Unity.Entities;

namespace World
{
    public static class WorldUtils
    {
        public static T GetSingleton<T>() where T : unmanaged, IComponentData
        {
            EntityManager em = Unity.Entities.World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityQuery query = em.CreateEntityQuery(typeof(T));
            return query.GetSingleton<T>();
        }
    }
}