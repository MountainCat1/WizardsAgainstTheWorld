using Components;
using UnityEngine;

namespace Helper
{
    public static class DifficultyApplier
    {
        public static void ApplyDifficulty(Creature creature)
        {
            var difficultyModifier = GameSettings.Instance.EnemyDifficulty;

            var healthComponents = creature.GetComponentsInChildren<HealthComponent>();
            foreach (var healthComponent in healthComponents)
            {
                healthComponent.SetMaxHealth(healthComponent.MaxValue * difficultyModifier);
            }

            GameLogger.Log($"Scaling {creature.name} to multiplier: {difficultyModifier} with difficulty: {GameSettings.Instance.Difficulty}");
        }
    }
}