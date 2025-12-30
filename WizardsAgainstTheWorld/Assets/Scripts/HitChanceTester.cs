using System.Collections.Generic;
using Combat;
using Items;
using UnityEngine;

public class HitChanceTester : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       DisplayProfile(HitChanceSettings.Enemy, "Enemy");
       DisplayProfile(HitChanceSettings.Friendly, "Friendly");
       DisplayProfile(HitChanceSettings.Obstacle, "Obstacle");
    }

    private void DisplayProfile(HitChanceProfile profile, string name)
    {
        var testAccuracies = new float[]
        {
            0f, 1f, 5f, 25f, 50f, 75f, 90f, 99f, 100f,
        };

        Debug.LogError($"--- {name} ---");

        foreach (var accuracy in testAccuracies)
        {
            var hitChance = HitChanceCalculator.GetHitChance(profile, accuracy / 100f);
            Debug.LogError($"Accuracy {accuracy}% - {hitChance:F2}");
        }
    }
}
