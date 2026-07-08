using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuUI : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private TMP_Text masterVolumeValueText;

    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private TMP_Text musicVolumeValueText;

    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private TMP_Text sfxVolumeValueText;

    [SerializeField] private Slider ambienceVolumeSlider;
    [SerializeField] private TMP_Text ambienceVolumeValueText;

    [SerializeField] private Toggle muteAllToggle;

    [Header("Pantalla")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Toggle vSyncToggle;

    [Header("Gráficos retro")]
    [SerializeField] private TMP_Dropdown retroResolutionDropdown;
    [SerializeField] private TMP_Dropdown wobbleDropdown;
    [SerializeField] private TMP_Dropdown fogDropdown;

    [Header("Botones")]
    [SerializeField] private Button applyButton;
    [SerializeField] private Button defaultsButton;

    [Header("Mensajes")]
    [SerializeField] private TMP_Text statusText;

    [Header("Audio predeterminado")]
    [Range(0f, 1f)]
    [SerializeField] private float defaultMasterVolume = 0.8f;

    [Range(0f, 1f)]
    [SerializeField] private float defaultMusicVolume = 0.8f;

    [Range(0f, 1f)]
    [SerializeField] private float defaultSFXVolume = 0.8f;

    [Range(0f, 1f)]
    [SerializeField] private float defaultAmbienceVolume = 0.8f;

    [SerializeField] private bool defaultMuted = false;

    [Header("Pantalla predeterminada")]
    [SerializeField] private int defaultWidth = 1280;
    [SerializeField] private int defaultHeight = 720;
    [SerializeField] private bool defaultFullscreen = false;
    [SerializeField] private bool defaultVSync = false;

    [Header("Gráficos retro predeterminados")]
    [SerializeField] private int defaultRetroResolution = 1;
    [SerializeField] private int defaultWobble = 1;
    [SerializeField] private int defaultFog = 2;

    private VolumeSettingsManager volumeManager;
    private DisplaySettingsManager displayManager;
    private RetroGraphicsSettingsManager retroManager;

    private List<Vector2Int> availableResolutions =
        new List<Vector2Int>();

    private float committedMasterVolume;
    private float committedMusicVolume;
    private float committedSFXVolume;
    private float committedAmbienceVolume;
    private bool committedMuted;

    private bool isLoadingValues;
    private bool valuesWereApplied;

    private void Awake()
    {
        volumeManager =
            VolumeSettingsManager.EnsureExists();

        displayManager =
            DisplaySettingsManager.EnsureExists();

        retroManager =
            RetroGraphicsSettingsManager.EnsureExists();

        ConfigureUIEvents();
        PopulateRetroDropdowns();
    }

    private void OnEnable()
    {
        valuesWereApplied = false;
        LoadCurrentSettingsIntoUI();
    }

    private void OnDisable()
    {
        if (valuesWereApplied)
            return;

        RestoreCommittedAudioPreview();
    }

    private void ConfigureUIEvents()
    {
        AddSliderListener(
            masterVolumeSlider,
            HandleMasterVolumeChanged
        );

        AddSliderListener(
            musicVolumeSlider,
            HandleMusicVolumeChanged
        );

        AddSliderListener(
            sfxVolumeSlider,
            HandleSFXVolumeChanged
        );

        AddSliderListener(
            ambienceVolumeSlider,
            HandleAmbienceVolumeChanged
        );

        if (muteAllToggle != null)
        {
            muteAllToggle.onValueChanged.AddListener(
                HandleMuteChanged
            );
        }

        if (resolutionDropdown != null)
        {
            resolutionDropdown.onValueChanged.AddListener(
                HandlePendingSettingChanged
            );
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.onValueChanged.AddListener(
                HandlePendingSettingChanged
            );
        }

        if (vSyncToggle != null)
        {
            vSyncToggle.onValueChanged.AddListener(
                HandlePendingSettingChanged
            );
        }

        if (retroResolutionDropdown != null)
        {
            retroResolutionDropdown.onValueChanged.AddListener(
                HandlePendingSettingChanged
            );
        }

        if (wobbleDropdown != null)
        {
            wobbleDropdown.onValueChanged.AddListener(
                HandlePendingSettingChanged
            );
        }

        if (fogDropdown != null)
        {
            fogDropdown.onValueChanged.AddListener(
                HandlePendingSettingChanged
            );
        }

        if (applyButton != null)
        {
            applyButton.onClick.AddListener(
                ApplySettings
            );
        }

        if (defaultsButton != null)
        {
            defaultsButton.onClick.AddListener(
                LoadDefaultValues
            );
        }
    }

    private void AddSliderListener(
        Slider slider,
        UnityEngine.Events.UnityAction<float> action
    )
    {
        if (slider != null)
        {
            slider.onValueChanged.AddListener(action);
        }
    }

    private void PopulateRetroDropdowns()
    {
        PopulateDropdown(
            retroResolutionDropdown,
            new List<string>
            {
                "256 x 144 - PS1 FUERTE",
                "320 x 180 - EQUILIBRADO",
                "426 x 240 - RETRO LIMPIO",
                "640 x 360 - PS2 LIMPIO"
            }
        );

        PopulateDropdown(
            wobbleDropdown,
            new List<string>
            {
                "DESACTIVADO",
                "SUAVE",
                "NORMAL"
            }
        );

        PopulateDropdown(
            fogDropdown,
            new List<string>
            {
                "DESACTIVADA",
                "BAJA",
                "MEDIA",
                "ALTA"
            }
        );
    }

    private void PopulateDropdown(
        TMP_Dropdown dropdown,
        List<string> options
    )
    {
        if (dropdown == null)
            return;

        dropdown.ClearOptions();
        dropdown.AddOptions(options);
        dropdown.RefreshShownValue();
    }

    private void LoadCurrentSettingsIntoUI()
    {
        isLoadingValues = true;

        EnsureManagersExist();
        PopulateResolutionDropdown();

        LoadAudioValues();
        LoadDisplayValues();
        LoadRetroValues();

        SetStatusMessage("");

        isLoadingValues = false;
    }

    private void EnsureManagersExist()
    {
        if (volumeManager == null)
        {
            volumeManager =
                VolumeSettingsManager.EnsureExists();
        }

        if (displayManager == null)
        {
            displayManager =
                DisplaySettingsManager.EnsureExists();
        }

        if (retroManager == null)
        {
            retroManager =
                RetroGraphicsSettingsManager.EnsureExists();
        }
    }

    private void LoadAudioValues()
    {
        if (volumeManager == null)
            return;

        committedMasterVolume =
            volumeManager.MasterVolume;

        committedMusicVolume =
            volumeManager.MusicVolume;

        committedSFXVolume =
            volumeManager.SFXVolume;

        committedAmbienceVolume =
            volumeManager.AmbienceVolume;

        committedMuted =
            volumeManager.IsMuted;

        SetSliderValue(
            masterVolumeSlider,
            committedMasterVolume
        );

        SetSliderValue(
            musicVolumeSlider,
            committedMusicVolume
        );

        SetSliderValue(
            sfxVolumeSlider,
            committedSFXVolume
        );

        SetSliderValue(
            ambienceVolumeSlider,
            committedAmbienceVolume
        );

        if (muteAllToggle != null)
        {
            muteAllToggle.SetIsOnWithoutNotify(
                committedMuted
            );
        }

        RefreshAllAudioTexts();
    }

    private void LoadDisplayValues()
    {
        if (displayManager == null)
            return;

        SelectResolutionInDropdown(
            displayManager.SavedWidth,
            displayManager.SavedHeight
        );

        if (fullscreenToggle != null)
        {
            fullscreenToggle.SetIsOnWithoutNotify(
                displayManager.SavedFullscreen
            );
        }

        if (vSyncToggle != null)
        {
            vSyncToggle.SetIsOnWithoutNotify(
                displayManager.SavedVSync
            );
        }
    }

    private void LoadRetroValues()
    {
        if (retroManager == null)
            return;

        SetDropdownValue(
            retroResolutionDropdown,
            retroManager.SavedRetroResolution
        );

        SetDropdownValue(
            wobbleDropdown,
            retroManager.SavedWobble
        );

        SetDropdownValue(
            fogDropdown,
            retroManager.SavedFog
        );
    }

    private void SetSliderValue(
        Slider slider,
        float value
    )
    {
        if (slider != null)
        {
            slider.SetValueWithoutNotify(value);
        }
    }

    private void HandleMasterVolumeChanged(float value)
    {
        if (isLoadingValues)
            return;

        volumeManager?.PreviewMasterVolume(value);

        RefreshPercentageText(
            masterVolumeValueText,
            value
        );

        MarkPendingChanges();
    }

    private void HandleMusicVolumeChanged(float value)
    {
        if (isLoadingValues)
            return;

        volumeManager?.PreviewMusicVolume(value);

        RefreshPercentageText(
            musicVolumeValueText,
            value
        );

        MarkPendingChanges();
    }

    private void HandleSFXVolumeChanged(float value)
    {
        if (isLoadingValues)
            return;

        volumeManager?.PreviewSFXVolume(value);

        RefreshPercentageText(
            sfxVolumeValueText,
            value
        );

        MarkPendingChanges();
    }

    private void HandleAmbienceVolumeChanged(float value)
    {
        if (isLoadingValues)
            return;

        volumeManager?.PreviewAmbienceVolume(value);

        RefreshPercentageText(
            ambienceVolumeValueText,
            value
        );

        MarkPendingChanges();
    }

    private void HandleMuteChanged(bool muted)
    {
        if (isLoadingValues)
            return;

        volumeManager?.PreviewMuted(muted);
        MarkPendingChanges();
    }

    private void MarkPendingChanges()
    {
        valuesWereApplied = false;

        SetStatusMessage(
            "PULSA APLICAR PARA GUARDAR"
        );
    }

    private void HandlePendingSettingChanged(int unusedValue)
    {
        if (isLoadingValues)
            return;

        MarkPendingChanges();
    }

    private void HandlePendingSettingChanged(bool unusedValue)
    {
        if (isLoadingValues)
            return;

        MarkPendingChanges();
    }

    private void RefreshAllAudioTexts()
    {
        RefreshPercentageText(
            masterVolumeValueText,
            committedMasterVolume
        );

        RefreshPercentageText(
            musicVolumeValueText,
            committedMusicVolume
        );

        RefreshPercentageText(
            sfxVolumeValueText,
            committedSFXVolume
        );

        RefreshPercentageText(
            ambienceVolumeValueText,
            committedAmbienceVolume
        );
    }

    private void RefreshPercentageText(
        TMP_Text targetText,
        float value
    )
    {
        if (targetText == null)
            return;

        int percentage =
            Mathf.RoundToInt(value * 100f);

        targetText.text = percentage + "%";
    }

    private void ApplySettings()
    {
        EnsureManagersExist();

        ApplyAudioSettings();
        ApplyDisplaySettings();
        ApplyRetroSettings();

        valuesWereApplied = true;

        SetStatusMessage(
            "CAMBIOS APLICADOS"
        );
    }

    private void ApplyAudioSettings()
    {
        if (volumeManager == null)
            return;

        float masterValue =
            GetSliderValue(
                masterVolumeSlider,
                defaultMasterVolume
            );

        float musicValue =
            GetSliderValue(
                musicVolumeSlider,
                defaultMusicVolume
            );

        float sfxValue =
            GetSliderValue(
                sfxVolumeSlider,
                defaultSFXVolume
            );

        float ambienceValue =
            GetSliderValue(
                ambienceVolumeSlider,
                defaultAmbienceVolume
            );

        bool muted =
            muteAllToggle != null &&
            muteAllToggle.isOn;

        volumeManager.SetAndSaveAll(
            masterValue,
            musicValue,
            sfxValue,
            ambienceValue,
            muted
        );

        committedMasterVolume = masterValue;
        committedMusicVolume = musicValue;
        committedSFXVolume = sfxValue;
        committedAmbienceVolume = ambienceValue;
        committedMuted = muted;
    }

    private float GetSliderValue(
        Slider slider,
        float fallback
    )
    {
        return slider != null
            ? slider.value
            : fallback;
    }

    private void RestoreCommittedAudioPreview()
    {
        if (volumeManager == null)
            return;

        volumeManager.PreviewMasterVolume(
            committedMasterVolume
        );

        volumeManager.PreviewMusicVolume(
            committedMusicVolume
        );

        volumeManager.PreviewSFXVolume(
            committedSFXVolume
        );

        volumeManager.PreviewAmbienceVolume(
            committedAmbienceVolume
        );

        volumeManager.PreviewMuted(
            committedMuted
        );
    }

    private void ApplyDisplaySettings()
    {
        if (displayManager == null ||
            availableResolutions.Count <= 0)
        {
            return;
        }

        int selectedIndex = 0;

        if (resolutionDropdown != null)
        {
            selectedIndex = Mathf.Clamp(
                resolutionDropdown.value,
                0,
                availableResolutions.Count - 1
            );
        }

        Vector2Int selectedResolution =
            availableResolutions[selectedIndex];

        bool fullscreen =
            fullscreenToggle != null &&
            fullscreenToggle.isOn;

        bool vSync =
            vSyncToggle != null &&
            vSyncToggle.isOn;

        displayManager.ApplyAndSave(
            selectedResolution.x,
            selectedResolution.y,
            fullscreen,
            vSync
        );
    }

    private void ApplyRetroSettings()
    {
        if (retroManager == null)
            return;

        int retroResolution =
            retroResolutionDropdown != null
                ? retroResolutionDropdown.value
                : defaultRetroResolution;

        int wobble =
            wobbleDropdown != null
                ? wobbleDropdown.value
                : defaultWobble;

        int fog =
            fogDropdown != null
                ? fogDropdown.value
                : defaultFog;

        retroManager.ApplyAndSave(
            retroResolution,
            wobble,
            fog
        );
    }

    private void LoadDefaultValues()
    {
        isLoadingValues = true;

        SetSliderValue(
            masterVolumeSlider,
            defaultMasterVolume
        );

        SetSliderValue(
            musicVolumeSlider,
            defaultMusicVolume
        );

        SetSliderValue(
            sfxVolumeSlider,
            defaultSFXVolume
        );

        SetSliderValue(
            ambienceVolumeSlider,
            defaultAmbienceVolume
        );

        if (muteAllToggle != null)
        {
            muteAllToggle.SetIsOnWithoutNotify(
                defaultMuted
            );
        }

        volumeManager?.PreviewMasterVolume(
            defaultMasterVolume
        );

        volumeManager?.PreviewMusicVolume(
            defaultMusicVolume
        );

        volumeManager?.PreviewSFXVolume(
            defaultSFXVolume
        );

        volumeManager?.PreviewAmbienceVolume(
            defaultAmbienceVolume
        );

        volumeManager?.PreviewMuted(defaultMuted);

        RefreshPercentageText(
            masterVolumeValueText,
            defaultMasterVolume
        );

        RefreshPercentageText(
            musicVolumeValueText,
            defaultMusicVolume
        );

        RefreshPercentageText(
            sfxVolumeValueText,
            defaultSFXVolume
        );

        RefreshPercentageText(
            ambienceVolumeValueText,
            defaultAmbienceVolume
        );

        SelectResolutionInDropdown(
            defaultWidth,
            defaultHeight
        );

        if (fullscreenToggle != null)
        {
            fullscreenToggle.SetIsOnWithoutNotify(
                defaultFullscreen
            );
        }

        if (vSyncToggle != null)
        {
            vSyncToggle.SetIsOnWithoutNotify(
                defaultVSync
            );
        }

        SetDropdownValue(
            retroResolutionDropdown,
            defaultRetroResolution
        );

        SetDropdownValue(
            wobbleDropdown,
            defaultWobble
        );

        SetDropdownValue(
            fogDropdown,
            defaultFog
        );

        isLoadingValues = false;
        valuesWereApplied = false;

        SetStatusMessage(
            "VALORES PREDETERMINADOS CARGADOS. PULSA APLICAR."
        );
    }

    private void PopulateResolutionDropdown()
    {
        if (resolutionDropdown == null)
            return;

        availableResolutions.Clear();

        if (displayManager != null)
        {
            availableResolutions =
                displayManager.GetAvailableResolutions();
        }

        if (availableResolutions.Count <= 0)
        {
            availableResolutions.Add(
                new Vector2Int(
                    Screen.width,
                    Screen.height
                )
            );
        }

        List<string> labels =
            new List<string>();

        foreach (Vector2Int resolution in availableResolutions)
        {
            labels.Add(
                resolution.x + " x " + resolution.y
            );
        }

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(labels);
        resolutionDropdown.RefreshShownValue();
    }

    private void SelectResolutionInDropdown(
        int width,
        int height
    )
    {
        if (resolutionDropdown == null)
            return;

        int index =
            FindResolutionIndex(width, height);

        resolutionDropdown.SetValueWithoutNotify(index);
        resolutionDropdown.RefreshShownValue();
    }

    private int FindResolutionIndex(
        int width,
        int height
    )
    {
        for (int i = 0;
             i < availableResolutions.Count;
             i++)
        {
            Vector2Int resolution =
                availableResolutions[i];

            if (resolution.x == width &&
                resolution.y == height)
            {
                return i;
            }
        }

        return FindClosestResolutionIndex(
            width,
            height
        );
    }

    private int FindClosestResolutionIndex(
        int width,
        int height
    )
    {
        if (availableResolutions.Count <= 0)
            return 0;

        int closestIndex = 0;
        int smallestDifference = int.MaxValue;

        for (int i = 0;
             i < availableResolutions.Count;
             i++)
        {
            Vector2Int resolution =
                availableResolutions[i];

            int difference =
                Mathf.Abs(resolution.x - width) +
                Mathf.Abs(resolution.y - height);

            if (difference < smallestDifference)
            {
                smallestDifference = difference;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

    private void SetDropdownValue(
        TMP_Dropdown dropdown,
        int value
    )
    {
        if (dropdown == null)
            return;

        int maximum =
            Mathf.Max(0, dropdown.options.Count - 1);

        dropdown.SetValueWithoutNotify(
            Mathf.Clamp(value, 0, maximum)
        );

        dropdown.RefreshShownValue();
    }

    private void SetStatusMessage(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }
}