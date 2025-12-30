using System.Collections.Generic;
using Utilities;

namespace DefaultNamespace.PersistentData
{
    public class CloudStatistics
    {
        public const string StatisticEnemiesKilled = "STAT_ENEMIES_KILLED";
        public const string StatisticPropsKilled = "STAT_PROPS_KILLED";
    }
    
    public class AchievementProgress : ISaveable<AchievementProgress>
    {
        public string GetFileName() => "achievement_progress.json";
        public static AchievementProgress Instance => SaveLoadManager.Load<AchievementProgress>();
        public static void Update(AchievementProgress instance) => SaveLoadManager.Update(instance);
        public static void Save() => SaveLoadManager.Save(Instance);

        public AchievementProgress CreateDefault()
        {
            return new AchievementProgress
            {
            };
        }
        
        public int EnemiesKilled { get; set; }
        public int BossesKilled { get; set; }
        public Dictionary<string, int> EnemiesKilledByType { get; set; } = new();
        
        
        public int LevelsCompleted { get; set; }
        public int PropsKilledWithGrenades { get; set; }
    }
}