using Combat;

namespace Items
{
    public static class HitChanceSettings
    {
        public static readonly HitChanceProfile Enemy = new HitChanceProfile(
            invert: false,
            sensitivity: 5f,
            minHitChance: 0.4f,
            modifier: 0.15f
        );

        public static readonly HitChanceProfile Obstacle = new HitChanceProfile(
            invert: true,
            sensitivity: 7f,
            minHitChance: 0.2f
        );

        public static HitChanceProfile Friendly => GameSettings.Instance.Preferences.FriendlyFire
            ? FriendlyForTrueMan
            : FriendlyForPussies;
        
        private static readonly HitChanceProfile FriendlyForPussies = new HitChanceProfile(
            invert: true,
            sensitivity: 12f,
            minHitChance: 1f,
            modifier: 0.1f
        );
        
        private static readonly HitChanceProfile FriendlyForTrueMan = new HitChanceProfile(
            invert: true,
            sensitivity: 9f,
            minHitChance: 0.75f,
            modifier: 0.1f
        );
    }
}