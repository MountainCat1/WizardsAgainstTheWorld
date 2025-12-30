using System;
using System.Collections.Generic;
using System.Linq;
using Steam;
using Steam.Steam;
using UnityEngine;
using Zenject;

namespace ScriptableActions
{
    public class UnlockAchievementAction : ScriptableAction
    {
        [Inject] private IAchievementsManager _achievementsManager;

        [SerializeField] private string achievementId;

        private static readonly HashSet<string> ValidAchievements;

        static UnlockAchievementAction()
        {
            // Collect all public constant string fields from Steam.Steam.Achievements
            ValidAchievements = typeof(Achievements)
                .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
                .Select(f => (string)f.GetRawConstantValue())
                .ToHashSet();
        }

        public override void Execute()
        {
            base.Execute();

            if (!ValidAchievements.Contains(achievementId))
            {
                throw new ArgumentException(
                    $"Invalid achievement ID '{achievementId}'. " +
                    "It must be one of the constants defined in Steam.Steam.Achievements."
                );
            }

            _achievementsManager.UnlockAchievement(achievementId);
        }

#if UNITY_EDITOR

        private void OnValidate()
        {
            if (!string.IsNullOrWhiteSpace(achievementId) && !ValidAchievements.Contains(achievementId))
            {
                Debug.LogError(
                    $"[UnlockAchievementAction] '{achievementId}' is not a valid achievement constant.",
                    this
                );
                achievementId = string.Empty; // Optional: reset the field
            }
            else
            {
                Debug.Log($"[UnlockAchievementAction] Valid achievement ID: {achievementId}", this);
            }
        }
#endif
    }
}