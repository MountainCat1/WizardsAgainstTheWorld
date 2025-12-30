using System;
using System.Collections.Generic;
using UI;
using UnityEngine;
using Zenject;

[Serializable]
public enum Characteristics
{
    Strength,
    Dexterity,
    Intelligence,
    Constitution
}

public static class CharacteristicsConsts
{
    public const float SpeedAdditiveMultiplierPerDexterity = 0.2f;
    public const float AttackSpeedAdditiveMultiplierPerDexterity = 0.1f;

    public const float MaxHealthPerConstitution = 10f;
    public const float DragReductionPerConstitution = 0.1f;

    public const float DamageAdditiveMultiplierPerStrength = 0.4f;
    public const float PushForceAdditiveMultiplierPerStrength = 0.4f;
    
    public const float AccuracyAdditiveMultiplierPerIntelligence = 0.2f;
}

public interface ILevelSystem
{
    event Action ChangedXp;
    event Action ChangedLevel;
    event Action CharacteristicsChanged;
    public int Xp { get; }
    public float LevelProgress { get; }
    public int PointsToUse { get; }
    void UpgradeCharacteristic(Characteristics characteristic);
    Dictionary<Characteristics, int> CharacteristicsLevels { get; }
    void AddXp(int amount);
    void SetData(LevelData levelData);
    LevelData GetData();
}

public class DisabledLevelingSystem : ILevelSystem
{
    public event Action ChangedXp;
    public event Action ChangedLevel;
    public event Action CharacteristicsChanged;

    public int Level => 0;
    public int Xp => 0;
    public float LevelProgress => 0;
    public int PointsToUse => 0;

    public Dictionary<Characteristics, int> CharacteristicsLevels { get; } = new()
    {
        { Characteristics.Strength, 0 },
        { Characteristics.Dexterity, 0 },
        { Characteristics.Intelligence, 0 },
        { Characteristics.Constitution, 0 }
    };

    public void AddXp(int amount)
    {
    }

    public void SetData(LevelData levelData)
    {
    }

    public LevelData GetData()
    {
        return new LevelData();
    }

    public void UpgradeCharacteristic(Characteristics characteristic)
    {
        GameLogger.LogError("No leveling system");
    }
}

public class LevelingComponent : MonoBehaviour, ILevelSystem
{
    public event Action ChangedXp;
    public event Action ChangedLevel;
    public event Action CharacteristicsChanged;

    [Inject] IFloatingTextManager _floatingTextService;

    private LevelData LevelData { get; set; } = new();

    public int Level => LevelData.CurrentLevel;
    public int Xp => LevelData.XpAmount;
    public float LevelProgress => LevelData.LevelProgress;
    public int PointsToUse => LevelData.PointsToUse;
    public Dictionary<Characteristics, int> CharacteristicsLevels => LevelData.CharacteristicsLevels;
    
    private const string LevelUpKey = "Game.FloatingText.LevelUp";
    private const string XpGainKey = "Game.FloatingText.XpGain";

    public void AddXp(int xp)
    {
        int previousLevel = Level;
        LevelData.AddXp(xp);
        _floatingTextService.SpawnFloatingText(transform.position, XpGainKey, FloatingTextType.XpGain, xp);
        ChangedXp?.Invoke();

        if (Level > previousLevel)
        {
            ChangedLevel?.Invoke();
            _floatingTextService.SpawnFloatingText(transform.position, LevelUpKey, FloatingTextType.LevelUp);
            GameLogger.Log($"Level up! New level: {Level}");
        }
    }

    public void SetData(LevelData levelData)
    {
        LevelData = levelData;
        ChangedXp?.Invoke();
        GameLogger.Log($"Set data: {levelData.XpAmount}");
    }

    public LevelData GetData()
    {
        return LevelData;
    }

    public void UpgradeCharacteristic(Characteristics characteristic)
    {
        try
        {
            LevelData.UpgradeCharacteristic(characteristic);
            CharacteristicsChanged?.Invoke();
            GameLogger.Log($"Upgraded {characteristic} to {CharacteristicsLevels[characteristic]}");
        }
        catch (InvalidOperationException e)
        {
            GameLogger.LogError(e.Message);
        }
    }
}
