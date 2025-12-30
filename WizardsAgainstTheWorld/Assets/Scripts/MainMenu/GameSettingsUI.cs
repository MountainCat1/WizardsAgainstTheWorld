using System;
using System.Collections.Generic;
using System.Linq;
using Managers;
using TMPro;
using UI.Abstractions;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

[RequireComponent(typeof(PageUI))]
public class GameSettingsUI : MonoBehaviour
{
    [Inject] private ISoundManager _soundManager;

    private GameSettings _gameSettings;

    [Header("Graphics Settings")] [SerializeField]
    private Toggle pixelPerfectToggle;

    [SerializeField] private Slider shakeSlider;

    [Header("Gameplay Settings")] [SerializeField]
    private Slider difficulty;
    [SerializeField] private Toggle useJuiceMechanicToggle;

    [SerializeField] private TMP_Dropdown languageDropdown;
    [SerializeField] private Toggle usePixelArtFontToggle;
    [SerializeField] private Toggle fallbackToDefaultLanguageToggle;
    [SerializeField] private Toggle displayLevelSelectorTutorialToggle;
    [SerializeField] private Toggle showHitChancesTutorialToggle;
    [SerializeField] private Toggle friendlyFireToggle;

    [Header("Audio Settings")] [SerializeField]
    private Slider sfxVolume;

    [SerializeField] private Slider musicVolume;
    [SerializeField] private Slider uiVolume;
    [SerializeField] private Slider masterVolume;
    [SerializeField] private Slider spatialAudioVolume;

    private void Awake()
    {
        _gameSettings = GameSettings.Instance;
        
        GetComponent<PageUI>().OnShow += () =>
        {
            // Reload settings when the page is shown
            _gameSettings = GameSettings.Instance;
            Start();
        };
    }

    private void Start()
    {
        _gameSettings = GameSettings.Instance;


        // General
        SetupSlider(difficulty, value => _gameSettings.Difficulty = value, _gameSettings.Difficulty, 3f, 0.1f);
        SetupToggle(useJuiceMechanicToggle,
            value => _gameSettings.Preferences.UseJuiceMechanic = value,
            _gameSettings.Preferences.UseJuiceMechanic);
        
        SetupDropdown(languageDropdown,
            value =>
            {
                _gameSettings.Language = value;
                CsvLocalizationManager.Instance.LoadLanguage(
                    languageCode: _gameSettings.Language,
                    fallbackToDefault: _gameSettings.FallbackToDefaultLanguage
                );
            },
            _gameSettings.Language,
            CsvLocalizationManager.Instance.AvailableLanguages.ToList());
        
        SetupToggle(usePixelArtFontToggle,
            value =>
            {
                CsvLocalizationManager.Instance.SetFont(value ? AvailableFont.PixelArt : AvailableFont.Normal);
                _gameSettings.UsePixelArtFont = value;
            },
            _gameSettings.UsePixelArtFont);

        SetupToggle(fallbackToDefaultLanguageToggle,
            value =>
            {
                _gameSettings.FallbackToDefaultLanguage = value;
                CsvLocalizationManager.Instance.LoadLanguage(
                    languageCode: _gameSettings.Language,
                    fallbackToDefault: _gameSettings.FallbackToDefaultLanguage
                );
            },
            _gameSettings.FallbackToDefaultLanguage);

        SetupToggle(displayLevelSelectorTutorialToggle,
            value => _gameSettings.DisplayLevelSelectorTutorial = value,
            _gameSettings.DisplayLevelSelectorTutorial);
        
        SetupToggle(showHitChancesTutorialToggle,
            value => _gameSettings.Preferences.ShowHitChances = value,
            _gameSettings.Preferences.ShowHitChances);
        
        SetupToggle(friendlyFireToggle,
            value => _gameSettings.Preferences.FriendlyFire = value,
            _gameSettings.Preferences.FriendlyFire);

        // Graphics
        SetupToggle(pixelPerfectToggle,
            value => _gameSettings.Visual.CameraSettings.PixelPerfectEnabled = value,
            _gameSettings.Visual.CameraSettings.PixelPerfectEnabled);
        SetupSlider(shakeSlider,
            value => _gameSettings.Visual.CameraSettings.ShakeIntensity = value,
            _gameSettings.Visual.CameraSettings.ShakeIntensity, 2f, 0f);

        // Audio
        SetupLogSlider(masterVolume,
            value => _gameSettings.Sound.MasterVolume = value,
            _gameSettings.Sound.MasterVolume);

        SetupLogSlider(sfxVolume,
            value => _gameSettings.Sound.SfxVolume = value,
            _gameSettings.Sound.SfxVolume);

        SetupLogSlider(musicVolume,
            value => _gameSettings.Sound.MusicVolume = value,
            _gameSettings.Sound.MusicVolume);

        SetupLogSlider(uiVolume,
            value => _gameSettings.Sound.UiVolume = value,
            _gameSettings.Sound.UiVolume);
        
        SetupSlider(spatialAudioVolume,
            value => _gameSettings.Sound.SpatialBlend = value,
            _gameSettings.Sound.SpatialBlend);
    }

    private void OnDestroy()
    {
        GameSettings.Update(_gameSettings);
        GameSettings.Save();
    }

    private void OnDisable()
    {
        GameSettings.Update(_gameSettings);
        GameSettings.Save();
    }


    private void SetupSlider(Slider slider, Action<float> setValue, float defaultValue, float maxValue = 1f,
        float minValue = 0f)
    {
        slider.minValue = minValue;
        slider.maxValue = maxValue;
        slider.value = Mathf.Clamp(defaultValue, minValue, maxValue);

        setValue(slider.value);
        slider.onValueChanged.AddListener(f =>
        {
            setValue.Invoke(f);
            ApplySettings();
        });
    }

    public void SetupToggle(Toggle toggle, Action<bool> setValue, bool defaultValue)
    {
        toggle.isOn = defaultValue;
        setValue(toggle.isOn);
        toggle.onValueChanged.AddListener(f =>
        {
            setValue.Invoke(f);
            ApplySettings();
        });
    }

    private void SetupLogSlider(Slider slider, Action<float> setLogValue, float storedLogValue, float maxValue = 1f,
        float minValue = 0.0001f)
    {
        slider.minValue = minValue;
        slider.maxValue = maxValue;

        float linearValue = Mathf.Pow(10f, storedLogValue);
        slider.value = Mathf.Clamp(linearValue, minValue, maxValue);

        slider.onValueChanged.AddListener(linear =>
        {
            float clamped = Mathf.Clamp(linear, minValue, maxValue);
            float logValue = Mathf.Log10(clamped);
            setLogValue(logValue);
            ApplySettings();
        });

        // Apply initial setting
        float initialLog = Mathf.Log10(slider.value);
        setLogValue(initialLog);
    }

    private void SetupDropdown<T>(
        TMP_Dropdown dropdown,
        Action<T> setValue,
        T currentValue,
        IList<T> options = null
    )
    {
        options ??= Enum.GetValues(typeof(T)) as T[];

        dropdown.ClearOptions();
        var optionLabels = new List<string>();
        foreach (var opt in options)
            optionLabels.Add(opt.ToString()); // You could localize here if needed

        dropdown.AddOptions(optionLabels);

        int currentIndex = options.IndexOf(currentValue);
        if (currentIndex >= 0)
            dropdown.value = currentIndex;

        setValue(options[dropdown.value]);

        dropdown.onValueChanged.AddListener(index =>
        {
            if (index >= 0 && index < options.Count)
            {
                setValue(options[index]);
                ApplySettings();
            }
        });
    }
    
    private void ApplySettings()
    {
        _soundManager.UpdateVolumes();

        GameSettings.Update(_gameSettings);
    }
}