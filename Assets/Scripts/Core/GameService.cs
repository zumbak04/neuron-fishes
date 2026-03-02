using System.Threading;
using Cysharp.Threading.Tasks;
using Spawn;
using VContainer;
using VContainer.Unity;
using World;

namespace Core
{
    public class GameService : IAsyncStartable
    {
        private readonly SpawnService _spawnService;
        private readonly WorldBoundsService _worldBoundsService;
        
        [Inject]
        public GameService(SpawnService spawnService, WorldBoundsService worldBoundsService)
        {
            _spawnService = spawnService;
            _worldBoundsService = worldBoundsService;
        }

        // todo zumbak временная точка входа
        async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            await UniTask.WaitUntil(() => Unity.Entities.World.DefaultGameObjectInjectionWorld != null, cancellationToken: cancellation);
            
            _worldBoundsService.Create();
            _spawnService.SpawnRandomFishes(600);
        }
    }
}