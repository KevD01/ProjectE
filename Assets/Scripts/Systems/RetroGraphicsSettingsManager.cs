using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RetroGraphicsSettingsManager : MonoBehaviour
{
    public static RetroGraphicsSettingsManager Instance;

    private const string RetroResolutionKey =
        "Settings_RetroResolution";

    private const string WobbleKey =
        "Settings_RetroWobble";

    private const string FogKey =
        "Settings_RetroFog";

    private static bool isApplicationQuitting;

    public int SavedRetroResolution =>
        Mathf.Clamp(
            PlayerPrefs.GetInt(RetroResolutionKey, 1),
            0,
            3
        );

    public int SavedWobble =>
        Mathf.Clamp(
            PlayerPrefs.GetInt(WobbleKey, 1),
            0,
            2
        );

    public int SavedFog =>
        Mathf.Clamp(
            PlayerPrefs.GetInt(FogKey, 2),
            0,
            3
        );

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.BeforeSceneLoad
    )]
    private static void Bootstrap()
    {
        isApplicationQuitting = false;
        EnsureExists();
    }

    public static RetroGraphicsSettingsManager EnsureExists()
    {
        if (Instance != null)
            return Instance;

        if (isApplicationQuitting)
            return null;

        GameObject managerObject =
            new GameObject("RetroGraphicsSettingsManager");

        return managerObject.AddComponent<
            RetroGraphicsSettingsManager
        >();
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

        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private IEnumerator Start()
    {
        yield return null;
        ApplySavedSettingsToCurrentScene();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void OnApplicationQuit()
    {
        isApplicationQuitting = true;
    }

    private void HandleSceneLoaded(
        Scene loadedScene,
        LoadSceneMode loadMode
    )
    {
        StartCoroutine(ApplyAfterSceneLoad());
    }

    private IEnumerator ApplyAfterSceneLoad()
    {
        yield return null;
        ApplySavedSettingsToCurrentScene();
    }

    public void ApplyAndSave(
        int retroResolutionIndex,
        int wobbleIndex,
        int fogIndex
    )
    {
        retroResolutionIndex = Mathf.Clamp(
            retroResolutionIndex,
            0,
            3
        );

        wobbleIndex = Mathf.Clamp(
            wobbleIndex,
            0,
            2
        );

        fogIndex = Mathf.Clamp(
            fogIndex,
            0,
            3
        );

        PlayerPrefs.SetInt(
            RetroResolutionKey,
            retroResolutionIndex
        );

        PlayerPrefs.SetInt(
            WobbleKey,
            wobbleIndex
        );

        PlayerPrefs.SetInt(
            FogKey,
            fogIndex
        );

        PlayerPrefs.Save();

        ApplySavedSettingsToCurrentScene();

        Debug.Log(
            "Ajustes retro guardados. " +
            "Resolución: " + retroResolutionIndex +
            " | Wobble: " + wobbleIndex +
            " | Niebla: " + fogIndex
        );
    }

    public void ApplySavedSettingsToCurrentScene()
    {
        Camera targetCamera = Camera.main;

        if (targetCamera == null)
        {
            targetCamera =
                FindFirstObjectByType<Camera>();
        }

        if (targetCamera == null)
            return;

        RenderTexture targetTexture =
            targetCamera.targetTexture;

        // El menú principal no utiliza la Render Texture retro.
        // Así evitamos modificar su cámara o su niebla.
        if (targetTexture == null)
            return;

        ApplyRetroResolution(
            targetTexture,
            SavedRetroResolution
        );

        ApplyCameraWobble(
            targetCamera,
            SavedWobble
        );

        ApplyFog(SavedFog);
    }

    private void ApplyRetroResolution(
        RenderTexture renderTexture,
        int presetIndex
    )
    {
        Vector2Int targetSize =
            GetRetroResolution(presetIndex);

        if (renderTexture.width == targetSize.x &&
            renderTexture.height == targetSize.y)
        {
            renderTexture.filterMode =
                FilterMode.Point;

            return;
        }

        renderTexture.Release();

        renderTexture.width = targetSize.x;
        renderTexture.height = targetSize.y;
        renderTexture.antiAliasing = 1;
        renderTexture.filterMode = FilterMode.Point;
        renderTexture.wrapMode = TextureWrapMode.Clamp;
        renderTexture.useMipMap = false;
        renderTexture.autoGenerateMips = false;

        renderTexture.Create();

        Debug.Log(
            "Resolución retro aplicada: " +
            targetSize.x + "x" + targetSize.y
        );
    }

    private Vector2Int GetRetroResolution(int presetIndex)
    {
        switch (presetIndex)
        {
            case 0:
                return new Vector2Int(256, 144);

            case 1:
                return new Vector2Int(320, 180);

            case 2:
                return new Vector2Int(426, 240);

            case 3:
                return new Vector2Int(640, 360);

            default:
                return new Vector2Int(320, 180);
        }
    }

    private void ApplyCameraWobble(
        Camera targetCamera,
        int presetIndex
    )
    {
        RetroCameraJitter cameraJitter =
            targetCamera.GetComponent<RetroCameraJitter>();

        if (cameraJitter == null)
            return;

        cameraJitter.ApplyPreset(presetIndex);
    }

    private void ApplyFog(int presetIndex)
    {
        switch (presetIndex)
        {
            case 0:
                RenderSettings.fog = false;
                break;

            case 1:
                RenderSettings.fog = true;
                RenderSettings.fogMode =
                    FogMode.Exponential;

                RenderSettings.fogDensity = 0.015f;
                break;

            case 2:
                RenderSettings.fog = true;
                RenderSettings.fogMode =
                    FogMode.Exponential;

                RenderSettings.fogDensity = 0.025f;
                break;

            case 3:
                RenderSettings.fog = true;
                RenderSettings.fogMode =
                    FogMode.Exponential;

                RenderSettings.fogDensity = 0.035f;
                break;
        }
    }
}