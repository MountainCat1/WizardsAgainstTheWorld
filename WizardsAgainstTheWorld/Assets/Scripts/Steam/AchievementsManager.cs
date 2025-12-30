using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DefaultNamespace.PersistentData;
using Managers;
using Steam.Steam;
using Steamworks;
using UnityEngine;
using Utilities;
using Zenject;

namespace Steam
{
    public interface IAchievementsManager
    {
        void UnlockAchievement(string achievementId);
        void ResetAllAchievements();
        public void CheckForAchievements();
        public void SetProgress(string statisticId, float progress);
        public void SetProgress(string statisticId, int progress);
        void HandleInventoryAchievements(List<InventoryData> allInventories);
    }

    public class AchievementsManager : MonoBehaviour, IAchievementsManager
    {
        [Inject] private SteamManager _steamManager;
        [Inject] private IItemManager _itemManager;

        void Awake()
        {
            SteamManager.ScheduleToExecuteOnInitialize(OnSteamInitialized);
        }

        private void OnSteamInitialized()
        {
            if (!SteamManager.Initialized)
            {
                GameLogger.LogError("SteamManager not initialized.");
                return;
            }

            UnlockAchievement(Achievements.AchievementHello);

            DontDestroyOnLoad(gameObject);
        }

        #region Public Methods

        public void UnlockAchievement(string achievementId)
        {
            if (!SteamManager.Initialized)
            {
                GameLogger.LogWarning("SteamManager not initialized. Cannot unlock achievement: " + achievementId);
                return;
            }

            if (string.IsNullOrEmpty(achievementId))
            {
                GameLogger.LogWarning("Achievement ID is null or empty.");
                return;
            }

            bool success = SteamUserStats.SetAchievement(achievementId);
            if (success)
            {
                SteamUserStats.StoreStats();
                GameLogger.Log($"Achievement {achievementId} unlocked successfully.");
            }
            else
            {
                GameLogger.LogError($"Failed to unlock achievement: {achievementId}");
            }
        }

        public void ResetAllAchievements()
        {
            if (!SteamManager.Initialized)
            {
                GameLogger.LogWarning("SteamManager not initialized. Cannot reset achievements.");
                return;
            }

            SteamUserStats.ResetAllStats(true); // true also resets achievements

            Type achievementsType = typeof(Achievements);
            FieldInfo[] fields =
                achievementsType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            foreach (FieldInfo field in fields)
            {
                if (field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string))
                {
                    string achievementId = (string)field.GetValue(null);
                    SteamUserStats.ClearAchievement(achievementId);
                    SteamUserStats.SetStat(achievementId, 0f);
                }
            }

            SteamUserStats.StoreStats();
            GameLogger.Log("All achievements have been reset via reflection.");
        }

        #endregion

        public void CheckForAchievements()
        {
            var achievementProgress = SaveLoadManager.Load<AchievementProgress>();

            SetProgress(CloudStatistics.StatisticEnemiesKilled, achievementProgress.EnemiesKilled);
            SetProgress(CloudStatistics.StatisticEnemiesKilled, achievementProgress.PropsKilledWithGrenades);

            if (achievementProgress.EnemiesKilled >= 50)
            {
                UnlockAchievement(Achievements.AchievementKill50Enemies);
            }

            if (achievementProgress.EnemiesKilled >= 250)
            {
                UnlockAchievement(Achievements.AchievementKill250Enemies);
            }

            if (achievementProgress.BossesKilled >= 1)
            {
                UnlockAchievement(Achievements.AchievementKillBoss);
            }
            
            if( achievementProgress.PropsKilledWithGrenades >= 25)
            {
                UnlockAchievement(Achievements.AchievementMiscKillPropsWithGrenade);
            }
            
            UnlockIfKilled(achievementProgress, "megatron", Achievements.AchievementKillMegatron);
            UnlockIfKilled(achievementProgress, "killerbot", Achievements.AchievementKillKillerbot);
            UnlockIfKilled(achievementProgress, "queen", Achievements.AchievementKillQueen);
            UnlockIfKilled(achievementProgress, "bloodspawn", Achievements.AchievementKillBloodspawn);
            UnlockIfKilled(achievementProgress, "corpobot", Achievements.AchievementKillMrBonson);
            UnlockIfKilled(achievementProgress, "cerebrate", Achievements.AchievementKillHive);
            
            SaveLoadManager.Save(achievementProgress);
        }
        
        private void UnlockIfKilled(AchievementProgress achievementProgress, string enemyType, string achievementId)
        {
            if (achievementProgress.EnemiesKilledByType.TryGetValue(enemyType, out var kills) && kills >= 1)
            {
                UnlockAchievement(achievementId);
            }
        }


        public void SetProgress(string statisticId, float progress)
        {
            if (!SteamManager.Initialized)
            {
                GameLogger.LogWarning("SteamManager not initialized. Cannot set progress for achievement: " +
                                      statisticId);
                return;
            }

            if (string.IsNullOrEmpty(statisticId))
            {
                GameLogger.LogWarning("Achievement ID is null or empty.");
                return;
            }

            bool success = SteamUserStats.SetStat(statisticId, progress);
            if (success)
            {
                SteamUserStats.StoreStats();
                GameLogger.Log($"Progress for achievement {statisticId} set to {progress}.");
            }

            else
            {
                GameLogger.LogError($"Failed to set progress for achievement: {statisticId}");
            }
        }

        public void SetProgress(string statisticId, int progress)
        {
            if (!SteamManager.Initialized)
            {
                GameLogger.LogWarning("SteamManager not initialized. Cannot set progress for achievement: " +
                                      statisticId);
                return;
            }

            if (string.IsNullOrEmpty(statisticId))
            {
                GameLogger.LogWarning("Achievement ID is null or empty.");
                return;
            }

            bool success = SteamUserStats.SetStat(statisticId, progress);
            if (success)
            {
                SteamUserStats.StoreStats();
                GameLogger.Log($"Progress for achievement {statisticId} set to {progress}.");
            }

            else
            {
                GameLogger.LogError($"Failed to set progress for achievement: {statisticId}");
            }
        }

        public void HandleInventoryAchievements(List<InventoryData> allInventories)
        {
            var allItems = allInventories
                .SelectMany(inventory => inventory.Items)
                .ToList();

            if (allItems.Any(x => x.Identifier == "Pickaxe" && _itemManager.GetModifierValue(x) >= 25))
            {
                UnlockAchievement(Achievements.AchievementMiscPickaxeUpgradeStrat);
            }

            if (allItems.Any(x => x.Identifier.ToLower().Trim() == "HolkSerum".ToLower().Trim()))
            {
                UnlockAchievement(Achievements.AchievementKillHulkLab);
            }
        }
    }
}