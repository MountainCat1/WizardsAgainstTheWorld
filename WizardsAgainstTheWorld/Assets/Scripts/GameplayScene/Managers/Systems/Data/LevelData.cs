using System.Collections.Generic;
using UnityEngine;

namespace Managers.Data
{
    [CreateAssetMenu(
        fileName = "LevelData",
        menuName = "WizardsAgainstTheWorld/Managers/Data/LevelData",
        order = 0
    )]
    public class LevelData : ScriptableObject
    {
        [field: SerializeField] public List<Creature> EnemyTypes { get; private set; }
        [field: SerializeField] public float BaseEnemySpawnManaPerSecond { get; private set; }
        [field: SerializeField] public float EnemySpawnManaGrowthRate { get; private set; }
    }
}