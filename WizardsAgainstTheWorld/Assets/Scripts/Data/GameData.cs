using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Services.MapGenerators;
using UnityEngine;

// ReSharper disable InconsistentNaming

[Serializable]
public class GameData
{
    public List<CreatureData> Creatures = new();
    public List<CrewUpgradeData> Upgrades = new();
    public string CurrentLocationId = string.Empty;
    public InventoryData Inventory = new();
    public InGameResources Resources = new();
}

public enum InGameResource
{
    Money,
    Fuel,
    Juice
}

[Serializable]
public class InGameResources
{
    public decimal Money = 0;
    public decimal Fuel = 0;
    public decimal Juice = 0;

    public event Action Changed;

    public void AddMoney(decimal amount)
    {
        Money += amount;
        Changed?.Invoke();
    }

    public void AddFuel(decimal amount)
    {
        Fuel += amount;
        Changed?.Invoke();
    }

    public void AddJuice(decimal amount)
    {
        Juice += amount;
        Changed?.Invoke();
    }
}


public enum LocationType
{
    Default,
    EndNode,
    StartNode,
    BossNode,
    ShopNode,
    EventNode
}

[Serializable]
public class GenerateMapSettingsData
{
    public int roomCount;
    public Vector2Int roomMinSize;
    public Vector2Int roomMaxSize;
    public Vector2Int gridSize;
    public float tileSize;
    public int seed;
    public GenerateMapSettings.CorridorWidth corridorWidth;

    public static GenerateMapSettingsData FromSettings(GenerateMapSettings settings)
    {
        return new GenerateMapSettingsData
        {
            roomCount = settings.roomCount,
            roomMinSize = settings.roomMinSize,
            roomMaxSize = settings.roomMaxSize,
            gridSize = settings.gridSize,
            tileSize = settings.tileSize,
            seed = settings.seed,
            corridorWidth = settings.corridorWidth
        };
    }

    public static GenerateMapSettings ToSettings(GenerateMapSettingsData levelDataMapSettings)
    {
        return new GenerateMapSettings
        {
            roomCount = levelDataMapSettings.roomCount,
            roomMinSize = levelDataMapSettings.roomMinSize,
            roomMaxSize = levelDataMapSettings.roomMaxSize,
            gridSize = levelDataMapSettings.gridSize,
            tileSize = levelDataMapSettings.tileSize,
            seed = levelDataMapSettings.seed,
            corridorWidth = levelDataMapSettings.corridorWidth
        };
    }
}


[Serializable]
public class CreatureData
{
    public string CreatureID;
    public string Name;
    public float SightRange;
    public InventoryData Inventory;
    public int ManaCost = 1;
    public bool Selected = false;
    public ColorData Color;
    public LevelData Level;


    public static CreatureData FromCreature(Creature creature)
    {
        Inventory inventory = creature.Inventory;
        if (inventory is null)
            inventory = new Inventory(creature.transform.Find("Inventory"),
                creature); // TODO: make this a const "Inventory"
        ILevelSystem levelSystem = creature.LevelingComponent;
        if (levelSystem is null)
            levelSystem = creature.GetComponent<ILevelSystem>();
        if (levelSystem is null)
            levelSystem = new DisabledLevelingSystem();

        return new CreatureData
        {
            CreatureID = creature.GetIdentifier(),
            Name = creature.name,
            SightRange = creature.SightRange,
            Inventory = InventoryData.FromInventory(inventory),
            ManaCost = creature.XpAmount,
            Color = new ColorData(creature.Color),
            Level = LevelData.FromComponent(levelSystem),
        };
    }
}

[Serializable]
public class LevelData
{
    public const int PointsPerLevel = 1;

    // Configuration
    private const int BaseXpLevelThreshold = 80; // XP required to go from level 0 -> 1
    private const int MaxLevel = 17;             // Highest attainable level

    // Thresholds[i] = total XP required to REACH level i.
    // thresholds[0] = 0 so level 0 progress never goes negative.
    private static readonly int[] LevelThresholds = BuildThresholds();

    private static int[] BuildThresholds()
    {
        var thresholds = new int[MaxLevel + 1];
        thresholds[0] = 0;
        for (int lvl = 1; lvl <= MaxLevel; lvl++)
        {
            // doubling curve: 80, 160, 320, ...
            thresholds[lvl] = checked(BaseXpLevelThreshold << (lvl - 1));
        }
        return thresholds;
    }

    public int XpAmount { get; private set; }
    public int CurrentLevel => CalculateLevelFromXp(XpAmount);
    public float LevelProgress => GetLevelProgress();

    // Points available from levels minus what you've spent (skills + characteristic increases).
    public int PointsToUse => CurrentLevel * PointsPerLevel - GetTotalPointsSpent();

    public Dictionary<Characteristics, int> CharacteristicsLevels { get; private set; } = new()
    {
        { Characteristics.Strength, 0 },
        { Characteristics.Dexterity, 0 },
        { Characteristics.Intelligence, 0 },
        { Characteristics.Constitution, 0 }
    };

    public ICollection<SkillData> AvailableSkills { get; set; } = new List<SkillData>();
    public ICollection<SkillData> Skills { get; set; } = new List<SkillData>();

    public LevelData(int xpAmount = 0)
    {
        if (xpAmount < 0) throw new ArgumentOutOfRangeException(nameof(xpAmount));
        XpAmount = xpAmount;
    }

    public static LevelData FromComponent(ILevelSystem creatureLevelingComponent)
    {
        if (creatureLevelingComponent == null) throw new ArgumentNullException(nameof(creatureLevelingComponent));
        return creatureLevelingComponent.GetData();
    }

    public static int CalculateLevelFromXp(int xp)
    {
        if (xp < 0) return 0;

        // Find the highest level L such that xp >= Thresholds[L].
        // Linear scan is fine given small MaxLevel; binary search if you want.
        int level = 0;
        for (int i = 1; i <= MaxLevel; i++)
        {
            if (xp >= LevelThresholds[i]) level = i;
            else break;
        }
        return level;
    }

    private float GetLevelProgress()
    {
        int level = CurrentLevel;
        if (level >= MaxLevel) return 1f;

        int lower = LevelThresholds[level];
        int upper = LevelThresholds[level + 1];
        if (upper <= lower) return 1f;

        float p = (XpAmount - lower) / (float)(upper - lower);
        return Mathf.Clamp01(p);
    }

    public void AddXp(int xp)
    {
        if (xp <= 0)
        {
            GameLogger.LogError($"XP gain must be positive! Instead is: {xp}");
            return;
        }
        XpAmount = checked(XpAmount + xp);
    }

    public void UpgradeCharacteristic(Characteristics characteristic)
    {
        if (!CharacteristicsLevels.ContainsKey(characteristic))
            throw new ArgumentException($"Unknown characteristic: {characteristic}", nameof(characteristic));

        if (PointsToUse <= 0)
            throw new InvalidOperationException("No points to spend");

        CharacteristicsLevels[characteristic] = CharacteristicsLevels[characteristic] + 1;
        // Points are implicitly spent because GetTotalPointsSpent() includes characteristic upgrades.
    }

    private int GetTotalPointsSpent()
    {
        // 1 point per characteristic increase
        int characteristicPoints = CharacteristicsLevels.Values.Sum();

        // plus total skill costs
        int skillPoints = 0;
        if (Skills != null)
        {
            foreach (var s in Skills) skillPoints = checked(skillPoints + s.Cost);
        }

        return checked(characteristicPoints + skillPoints);
    }

    public void BuySkill(SkillData toData)
    {
        if (toData == null) throw new ArgumentNullException(nameof(toData));
        if (Skills != null && Skills.Contains(toData))
            throw new InvalidOperationException("Skill already owned");

        if (PointsToUse < toData.Cost)
            throw new InvalidOperationException("Not enough points to buy skill");

        Skills.Add(toData);
        AvailableSkills?.Remove(toData);
    }

    public int XpIntoCurrentLevel()
    {
        int lvl = CurrentLevel;
        return XpAmount - LevelThresholds[lvl];
    }

    public int XpToNextLevel()
    {
        int lvl = CurrentLevel;
        if (lvl >= MaxLevel) return 0;
        return LevelThresholds[lvl + 1] - XpAmount;
    }
}

public class SkillData
{
    public string SkillID;
    public int Cost;

    [Obsolete("Use SkillDescriptor.ToData() instead")]
    public SkillData()
    {
    }
}

public struct ColorData
{
    public float r;
    public float g;
    public float b;
    public float a;

    public ColorData(Color color)
    {
        r = color.r;
        g = color.g;
        b = color.b;
        a = color.a;
    }

    public static ColorData FromColor(Color color)
    {
        return new ColorData(color);
    }

    public Color ToColor()
    {
        return new Color(r, g, b, a);
    }
}