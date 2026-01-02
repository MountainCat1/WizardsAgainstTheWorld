using Building;
using Building.Managers;
using Managers;
using Zenject;

namespace GameplayScene.Managers.Pathfinding
{
    public class PathfindingRecalculator
    {
        private IAstarManager _astarManager;
        
        [Inject]
        private void Construct(
            [Inject] IPathfinding pathfinding,
            [Inject] IBuilderManager builderManager,
            [Inject] IAstarManager astarManager,
            [Inject] IMapGenerator mapGenerator
        )
        {
            _astarManager = astarManager;
            
            builderManager.EntityPlaced += OnEntityPlaced;
            mapGenerator.MapGenerated += OnMapGenerated;
        }

        private void OnMapGenerated()
        {
            _astarManager.ScanDelayed();
        }

        private void OnEntityPlaced(Entity obj)
        {
            _astarManager.ScanDelayed();
        }
    }
}