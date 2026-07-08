using System.Collections.Generic;
using UnityEngine;

public class DisplaySettingsManager : MonoBehaviour
{
    public static DisplaySettingsManager Instance;

    private const string ResolutionWidthKey = "Settings_ResolutionWidth";
    private const string ResolutionHeightKey = "Settings_ResolutionHeight";
    private const string FullscreenKey = "Settings_Fullscreen";
    private const string VSyncKey = "Settings_VSync";

    private static bool isApplicationQuitting;

    public int SavedWidth =>
        PlayerPrefs.GetInt(ResolutionWidthKey, Screen.width);

    public int SavedHeight =>
        PlayerPrefs.GetInt(ResolutionHeightKey, Screen.height);

    public bool SavedFullscreen =>
        PlayerPrefs.GetInt(
            FullscreenKey,
            Screen.fullScreen ? 1 : 0
        ) == 1;

    public bool SavedVSync =>
        PlayerPrefs.GetInt(
            VSyncKey,
            QualitySettings.vSyncCount > 0 ? 1 : 0
        ) == 1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        isApplicationQuitting = false;
        EnsureExists();
    }

    public static DisplaySettingsManager EnsureExists()
    {
        if (Instance != null)
            return Instance;

        if (isApplicationQuitting)
            return null;

        GameObject managerObject =
            new GameObject("DisplaySettingsManager");

        return managerObject.AddComponent<DisplaySettingsManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        ApplySavedSettingsIfAvailable();
    }

    private void OnApplicationQuit()
    {
        isApplicationQuitting = true;
    }

    private void ApplySavedSettingsIfAvailable()
    {
        bool hasResolution =
            PlayerPrefs.HasKey(ResolutionWidthKey) &&
            PlayerPrefs.HasKey(ResolutionHeightKey);

        bool hasFullscreen =
            PlayerPrefs.HasKey(FullscreenKey);

        bool hasVSync =
            PlayerPrefs.HasKey(VSyncKey);

        if (hasVSync)
        {
            QualitySettings.vSyncCount = SavedVSync ? 1 : 0;
        }

        if (!hasResolution && !hasFullscreen)
            return;

        FullScreenMode screenMode = SavedFullscreen
            ? FullScreenMode.FullScreenWindow
            : FullScreenMode.Windowed;

        Screen.SetResolution(
            SavedWidth,
            SavedHeight,
            screenMode
        );
    }

    public List<Vector2Int> GetAvailableResolutions()
    {
        List<Vector2Int> availableResolutions =
            new List<Vector2Int>();

        HashSet<string> addedResolutions =
            new HashSet<string>();

        Resolution[] resolutions = Screen.resolutions;

        foreach (Resolution resolution in resolutions)
        {
            string resolutionKey =
                resolution.width + "x" + resolution.height;

            if (addedResolutions.Contains(resolutionKey))
                continue;

            addedResolutions.Add(resolutionKey);

            availableResolutions.Add(
                new Vector2Int(
                    resolution.width,
                    resolution.height
                )
            );
        }

        Vector2Int currentResolution =
            new Vector2Int(Screen.width, Screen.height);

        string currentKey =
            currentResolution.x + "x" + currentResolution.y;

        if (!addedResolutions.Contains(currentKey))
        {
            availableResolutions.Add(currentResolution);
        }

        availableResolutions.Sort(
            (first, second) =>
            {
                int firstPixels = first.x * first.y;
                int secondPixels = second.x * second.y;

                return secondPixels.CompareTo(firstPixels);
            }
        );

        return availableResolutions;
    }

    public void ApplyAndSave(
        int width,
        int height,
        bool fullscreen,
        bool useVSync
    )
    {
        width = Mathf.Max(640, width);
        height = Mathf.Max(360, height);

        QualitySettings.vSyncCount = useVSync ? 1 : 0;

        FullScreenMode screenMode = fullscreen
            ? FullScreenMode.FullScreenWindow
            : FullScreenMode.Windowed;

        Screen.SetResolution(
            width,
            height,
            screenMode
        );

        PlayerPrefs.SetInt(ResolutionWidthKey, width);
        PlayerPrefs.SetInt(ResolutionHeightKey, height);
        PlayerPrefs.SetInt(
            FullscreenKey,
            fullscreen ? 1 : 0
        );
        PlayerPrefs.SetInt(
            VSyncKey,
            useVSync ? 1 : 0
        );

        PlayerPrefs.Save();

        Debug.Log(
            "Ajustes de pantalla guardados: " +
            width + "x" + height +
            " | Pantalla completa: " + fullscreen +
            " | VSync: " + useVSync
        );
    }
}