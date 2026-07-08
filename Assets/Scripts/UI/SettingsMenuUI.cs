using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuUI : MonoBehaviour
{
    [Header("Volumen")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private TMP_Text masterVolumeValueText;

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

    [Header("Valores predeterminados")]
    [Range(0f, 1f)]
    [SerializeField] private float defaultVolume = 0.8f;

    [SerializeField] private int defaultWidth = 1280;
    [SerializeField] private int defaultHeight = 720;
    [SerializeField] private bool defaultFullscreen = false;
    [SerializeField] private bool defaultVSync = false;

    [SerializeField] private int defaultRetroResolution = 1;
    [SerializeField] private int defaultWobble = 1;
    [SerializeField] private int defaultFog = 2;

    private VolumeSettingsManager volumeManager;
    private DisplaySettingsManager displayManager;
    private RetroGraphicsSettingsManager retroManager;

    private List<Vector2Int> availableResolutions =
        new List<Vector2Int>();

    private float committedVolume = 0.8f;
    private bool isLoadingValues;

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
        LoadCurrentSettingsIntoUI();
    }

    private void OnDisable()
    {
        if (volumeManager != null)
        {
            volumeManager.PreviewVolume(
                committedVolume
            );
        }
    }

    private void ConfigureUIEvents()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.AddListener(
                HandleVolumeSliderChanged
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

        PopulateResolutionDropdown();

        if (volumeManager != null)
        {
            committedVolume =
                volumeManager.MasterVolume;

            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.SetValueWithoutNotify(
                    committedVolume
                );
            }

            RefreshVolumeValueText(
                committedVolume
            );
        }

        if (displayManager != null)
        {
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

        if (retroManager != null)
        {
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

        SetStatusMessage("");

        isLoadingValues = false;
    }

    private void SetDropdownValue(
        TMP_Dropdown dropdown,
        int value
    )
    {
        if (dropdown == null)
            return;

        int maximumIndex =
            Mathf.Max(0, dropdown.options.Count - 1);

        dropdown.SetValueWithoutNotify(
            Mathf.Clamp(value, 0, maximumIndex)
        );

        dropdown.RefreshShownValue();
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

        int resolutionIndex =
            FindResolutionIndex(width, height);

        resolutionDropdown.SetValueWithoutNotify(
            resolutionIndex
        );

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
        int targetWidth,
        int targetHeight
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
                Mathf.Abs(
                    resolution.x - targetWidth
                ) +
                Mathf.Abs(
                    resolution.y - targetHeight
                );

            if (difference < smallestDifference)
            {
                smallestDifference = difference;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

    private void HandleVolumeSliderChanged(float value)
    {
        if (isLoadingValues)
            return;

        if (volumeManager != null)
        {
            volumeManager.PreviewVolume(value);
        }

        RefreshVolumeValueText(value);

        SetStatusMessage(
            "PULSA APLICAR PARA GUARDAR"
        );
    }

    private void HandlePendingSettingChanged(
        int unusedValue
    )
    {
        if (isLoadingValues)
            return;

        SetStatusMessage(
            "PULSA APLICAR PARA GUARDAR"
        );
    }

    private void HandlePendingSettingChanged(
        bool unusedValue
    )
    {
        if (isLoadingValues)
            return;

        SetStatusMessage(
            "PULSA APLICAR PARA GUARDAR"
        );
    }

    private void RefreshVolumeValueText(float value)
    {
        if (masterVolumeValueText == null)
            return;

        int percentage =
            Mathf.RoundToInt(value * 100f);

        masterVolumeValueText.text =
            percentage + "%";
    }

    private void ApplySettings()
    {
        ApplyVolumeSettings();
        ApplyDisplaySettings();
        ApplyRetroSettings();

        SetStatusMessage(
            "CAMBIOS APLICADOS"
        );
    }

    private void ApplyVolumeSettings()
    {
        if (volumeManager == null ||
            masterVolumeSlider == null)
        {
            return;
        }

        volumeManager.SetVolume(
            masterVolumeSlider.value
        );

        committedVolume =
            volumeManager.MasterVolume;
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

        bool useFullscreen =
            fullscreenToggle != null &&
            fullscreenToggle.isOn;

        bool useVSync =
            vSyncToggle != null &&
            vSyncToggle.isOn;

        displayManager.ApplyAndSave(
            selectedResolution.x,
            selectedResolution.y,
            useFullscreen,
            useVSync
        );
    }

    private void ApplyRetroSettings()
    {
        if (retroManager == null)
            return;

        int retroResolutionValue =
            retroResolutionDropdown != null
                ? retroResolutionDropdown.value
                : defaultRetroResolution;

        int wobbleValue =
            wobbleDropdown != null
                ? wobbleDropdown.value
                : defaultWobble;

        int fogValue =
            fogDropdown != null
                ? fogDropdown.value
                : defaultFog;

        retroManager.ApplyAndSave(
            retroResolutionValue,
            wobbleValue,
            fogValue
        );
    }

    private void LoadDefaultValues()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.SetValueWithoutNotify(
                defaultVolume
            );

            RefreshVolumeValueText(
                defaultVolume
            );

            if (volumeManager != null)
            {
                volumeManager.PreviewVolume(
                    defaultVolume
                );
            }
        }

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

        SetStatusMessage(
            "VALORES PREDETERMINADOS CARGADOS. PULSA APLICAR."
        );
    }

    private void SetStatusMessage(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }
}