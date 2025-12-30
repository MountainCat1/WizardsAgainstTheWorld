using UnityEngine;

namespace Combat
{
    public readonly struct HitChanceProfile
    {
        public bool Invert { get; }
        public float Sensitivity { get; }
        public float Modifier { get; }
        public float MinHitChance { get; }

        public HitChanceProfile(bool invert = false,
            float sensitivity = 0.2f,
            float modifier = 0f,
            float minHitChance = 0f
        )
        {
            Invert = invert;
            Sensitivity = sensitivity;
            Modifier = modifier;
            MinHitChance = minHitChance;
        }
    }

    public static class HitChanceCalculator
    {
        /// <summary>
        /// accuracy: 0.0 - 1.0 (float)
        /// </summary>
        public static bool ShouldMiss(HitChanceProfile hitChanceProfile, float accuracy)
        {
            var hitChance = GetHitChance(hitChanceProfile, accuracy);
            return UnityEngine.Random.value > hitChance;
        }

        /// <summary>
        /// accuracy: 0.0 - 1.0 (float)
        /// </summary>
        public static float GetHitChance(HitChanceProfile hitChanceProfile, float accuracy)
        {
            var hitChance = hitChanceProfile.Invert
                ? CalculateInvertedHitChance(
                    accuracy,
                    hitChanceProfile.Sensitivity,
                    hitChanceProfile.Modifier
                )
                : CalculateHitChance(accuracy, hitChanceProfile.Sensitivity, hitChanceProfile.Modifier);

            return hitChanceProfile.Invert
                ? Mathf.Min(hitChance, 1f - hitChanceProfile.MinHitChance)
                : Mathf.Max(hitChance, hitChanceProfile.MinHitChance);
        }

        /// <summary>
        /// accuracy: 0.0 - 1.0 (float)
        /// </summary>
        private static float CalculateInvertedHitChance(float accuracy, float sensitivity, float modifier = 0.0f)
        {
            var x = Mathf.Clamp01(accuracy);
            var chance = 1f - (sensitivity * x) / (sensitivity * x + 1.0f);
            return Mathf.Clamp01(chance - modifier);
        }

        /// <summary>
        /// accuracy: 0.0 - 1.0 (float)
        /// </summary>
        private static float CalculateHitChance(float accuracy, float sensitivity, float modifier = 0.0f)
        {
            var x = Mathf.Clamp01(accuracy);
            var chance = (sensitivity * x) / (sensitivity * x + 1.0f);
            return Mathf.Clamp01(chance + modifier);
        }
    }
}
