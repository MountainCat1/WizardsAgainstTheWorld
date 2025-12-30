using UnityEngine;
using Utilities;

public class GameSettings : ISaveable<GameSettings>
{
    public static GameSettings Instance
    {
        get
        {
            var settings = SaveLoadManager.Load<GameSettings>();
            
            if(settings.DifficultySettings.OverrideWithDefault)
            {
                settings.DifficultySettings = new DifficultySettings();
            }

            return settings;
        }
    }

    public static void Update(GameSettings instance) => SaveLoadManager.Update(instance);
    public static void Save() => SaveLoadManager.Save(Instance);

    public string GetFileName() => "settings.json";

    public GameSettings CreateDefault()
    {
        Application.targetFrameRate = 144;

        return new GameSettings
        {
            LoadGameTutorial = true,
            DisplayLevelSelectorTutorial = true,
            SkipIntro = false,
            Language = "En",
            FallbackToDefaultLanguage = true,
            Difficulty = 0.4f,
            DisplayDifficultySelection = true,
            
            Visual = new GameSettingsVisual(),
            DifficultySettings = new DifficultySettings(),
            Sound = new SoundSettings()
        };
    }

    public bool FallbackToDefaultLanguage { get; set; }

    public string Language { get; set; }
    
    public float Difficulty { get; set; }
    public float EnemyDifficulty => Difficulty * DifficultySettings.EnemyDifficultyModifierFactor;
    
    public bool SkipIntro { get; set; }
    public bool LoadGameTutorial { get; set; }
    public bool DisplayLevelSelectorTutorial { get; set; }
    public bool DisplayDifficultySelection { get; set; }
    
    public GameSettingsVisual Visual { get; set; } = new();
    public DifficultySettings DifficultySettings { get; set; } = new();
    public SoundSettings Sound { get; set; } = new();
    public GameSettingsPreferences Preferences { get; set; } = new();
    public bool UsePixelArtFont { get; set; } = true;
}

public class GameSettingsPreferences
{
    public bool ShowHitChances { get; set; } = false;
    public bool FriendlyFire { get; set; } = false;
    public bool UseJuiceMechanic { get; set; } = false;
}

public class SoundSettings
{
    public float SpatialBlend { get; set; } = 0.649f; 
    public float MasterVolume { get; set; } = -0.299f;
    public float SfxVolume { get; set; } = -0.838f;
    public float MusicVolume { get; set; } = -0.259f;
    public float UiVolume { get ; set; } = -0.912f;
    public bool RandomPitch { get; set; } = false;
}

public class DifficultySettings
{
    public float MinBaseManaPerSecond { get; set; } = 0.8f;
    public float MaxBaseManaMultiplier { get; set; } = 2.3f;
    
    public float MinInitialMana { get; set; } = 550f;
    public float MaxInitialMana { get; set; } = 900f;
    
    public float MinManaGrowthRate { get; set; } = 0.02f;
    public float MaxManaGrowthRate { get; set; } = 0.0275f;

    public float EnemyDifficultyModifierFactor { get; set; } = 1f;
    public float SpawnRateDifficultyModifierFactor { get; set; } = 0.25f;
    
    public bool OverrideWithDefault { get; set; } = true;
}

public class GameSettingsVisual
{
    public CameraSettings CameraSettings { get; set; } = new CameraSettings();
}

public class CameraSettings
{
    public bool PixelPerfectEnabled { get; set; } = false;
    public float ShakeIntensity { get; set; } = 0.4f;
}