using UnityEngine;

public class VolumeSettingsManager : MonoBehaviour
{
    public static VolumeSettingsManager Instance;

    private const string MasterVolumeKey = "MasterVolume";

    [Header("Volumen")]
    [Range(0f, 1f)]
    [SerializeField] private float defaultVolume = 0.8f;

    [Range(0.01f, 0.5f)]
    [SerializeField] private float volumeStep = 0.1f;

    private float masterVolume;

    public float MasterVolume => masterVolume;
    public int MasterVolumePercent => Mathf.RoundToInt(masterVolume * 100f);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        LoadVolume();
    }

    public static VolumeSettingsManager EnsureExists()
    {
        if (Instance != null)
            return Instance;

        GameObject managerObject = new GameObject("VolumeSettingsManager");
        return managerObject.AddComponent<VolumeSettingsManager>();
    }

    private void LoadVolume()
    {
        masterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, defaultVolume);
        masterVolume = Mathf.Clamp01(masterVolume);

        ApplyVolume();
    }

    public void IncreaseVolume()
    {
        SetVolume(masterVolume + volumeStep);
    }

    public void DecreaseVolume()
    {
        SetVolume(masterVolume - volumeStep);
    }

    public void SetVolume(float value)
    {
        masterVolume = Mathf.Clamp01(value);

        ApplyVolume();
        SaveVolume();
    }

    private void ApplyVolume()
    {
        AudioListener.volume = masterVolume;
    }

    private void SaveVolume()
    {
        PlayerPrefs.SetFloat(MasterVolumeKey, masterVolume);
        PlayerPrefs.Save();
    }
}