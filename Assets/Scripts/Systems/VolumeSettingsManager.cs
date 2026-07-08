using UnityEngine;
using UnityEngine.Audio;

public class VolumeSettingsManager : MonoBehaviour
{
    public static VolumeSettingsManager Instance;

    private const string AudioMixerResourcePath = "Audio/AM_Main";

    private const string MasterVolumeParameter = "MasterVolume";
    private const string MusicVolumeParameter = "MusicVolume";
    private const string SFXVolumeParameter = "SFXVolume";
    private const string AmbienceVolumeParameter = "AmbienceVolume";

    private const string MasterVolumeKey = "Audio_MasterVolume";
    private const string MusicVolumeKey = "Audio_MusicVolume";
    private const string SFXVolumeKey = "Audio_SFXVolume";
    private const string AmbienceVolumeKey = "Audio_AmbienceVolume";
    private const string MutedKey = "Audio_Muted";

    [Header("Valores predeterminados")]
    [Range(0f, 1f)]
    [SerializeField] private float defaultMasterVolume = 0.8f;

    [Range(0f, 1f)]
    [SerializeField] private float defaultMusicVolume = 0.8f;

    [Range(0f, 1f)]
    [SerializeField] private float defaultSFXVolume = 0.8f;

    [Range(0f, 1f)]
    [SerializeField] private float defaultAmbienceVolume = 0.8f;

    private AudioMixer audioMixer;

    private float masterVolume;
    private float musicVolume;
    private float sfxVolume;
    private float ambienceVolume;
    private bool isMuted;

    private static bool isApplicationQuitting;

    public float MasterVolume => masterVolume;
    public float MusicVolume => musicVolume;
    public float SFXVolume => sfxVolume;
    public float AmbienceVolume => ambienceVolume;
    public bool IsMuted => isMuted;

    public int MasterVolumePercent =>
        Mathf.RoundToInt(masterVolume * 100f);

    public int MusicVolumePercent =>
        Mathf.RoundToInt(musicVolume * 100f);

    public int SFXVolumePercent =>
        Mathf.RoundToInt(sfxVolume * 100f);

    public int AmbienceVolumePercent =>
        Mathf.RoundToInt(ambienceVolume * 100f);

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.BeforeSceneLoad
    )]
    private static void Bootstrap()
    {
        isApplicationQuitting = false;
        EnsureExists();
    }

    public static VolumeSettingsManager EnsureExists()
    {
        if (Instance != null)
            return Instance;

        if (isApplicationQuitting)
            return null;

        GameObject managerObject =
            new GameObject("VolumeSettingsManager");

        return managerObject.AddComponent<VolumeSettingsManager>();
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

        audioMixer = Resources.Load<AudioMixer>(
            AudioMixerResourcePath
        );

        LoadSavedValues();
    }

    private void Start()
    {
        ApplyAllVolumes();
    }

    private void OnApplicationQuit()
    {
        isApplicationQuitting = true;
    }

    private void LoadSavedValues()
    {
        masterVolume = Mathf.Clamp01(
            PlayerPrefs.GetFloat(
                MasterVolumeKey,
                defaultMasterVolume
            )
        );

        musicVolume = Mathf.Clamp01(
            PlayerPrefs.GetFloat(
                MusicVolumeKey,
                defaultMusicVolume
            )
        );

        sfxVolume = Mathf.Clamp01(
            PlayerPrefs.GetFloat(
                SFXVolumeKey,
                defaultSFXVolume
            )
        );

        ambienceVolume = Mathf.Clamp01(
            PlayerPrefs.GetFloat(
                AmbienceVolumeKey,
                defaultAmbienceVolume
            )
        );

        isMuted =
            PlayerPrefs.GetInt(MutedKey, 0) == 1;
    }

    public void PreviewMasterVolume(float value)
    {
        masterVolume = Mathf.Clamp01(value);
        ApplyMasterVolume();
    }

    public void PreviewMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        ApplyMixerVolume(
            MusicVolumeParameter,
            musicVolume
        );
    }

    public void PreviewSFXVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        ApplyMixerVolume(
            SFXVolumeParameter,
            sfxVolume
        );
    }

    public void PreviewAmbienceVolume(float value)
    {
        ambienceVolume = Mathf.Clamp01(value);
        ApplyMixerVolume(
            AmbienceVolumeParameter,
            ambienceVolume
        );
    }

    public void PreviewMuted(bool muted)
    {
        isMuted = muted;
        ApplyMasterVolume();
    }

    public void SetAndSaveAll(
        float newMasterVolume,
        float newMusicVolume,
        float newSFXVolume,
        float newAmbienceVolume,
        bool muted
    )
    {
        masterVolume = Mathf.Clamp01(newMasterVolume);
        musicVolume = Mathf.Clamp01(newMusicVolume);
        sfxVolume = Mathf.Clamp01(newSFXVolume);
        ambienceVolume = Mathf.Clamp01(newAmbienceVolume);
        isMuted = muted;

        ApplyAllVolumes();
        SaveAllValues();
    }

    public void ReloadSavedValues()
    {
        LoadSavedValues();
        ApplyAllVolumes();
    }

    private void ApplyAllVolumes()
    {
        ApplyMasterVolume();

        ApplyMixerVolume(
            MusicVolumeParameter,
            musicVolume
        );

        ApplyMixerVolume(
            SFXVolumeParameter,
            sfxVolume
        );

        ApplyMixerVolume(
            AmbienceVolumeParameter,
            ambienceVolume
        );
    }

    private void ApplyMasterVolume()
    {
        float effectiveVolume =
            isMuted ? 0f : masterVolume;

        ApplyMixerVolume(
            MasterVolumeParameter,
            effectiveVolume
        );

        if (audioMixer == null)
        {
            AudioListener.volume = effectiveVolume;
        }
        else
        {
            AudioListener.volume = 1f;
        }
    }

    private void ApplyMixerVolume(
        string parameterName,
        float linearValue
    )
    {
        if (audioMixer == null)
        {
            Debug.LogWarning(
                "No se encontró AM_Main en Resources/Audio."
            );

            return;
        }

        float decibelValue =
            LinearToDecibels(linearValue);

        bool parameterFound =
            audioMixer.SetFloat(
                parameterName,
                decibelValue
            );

        if (!parameterFound)
        {
            Debug.LogWarning(
                "No se encontró el parámetro expuesto: " +
                parameterName
            );
        }
    }

    private float LinearToDecibels(float linearValue)
    {
        if (linearValue <= 0.0001f)
            return -80f;

        return Mathf.Log10(linearValue) * 20f;
    }

    private void SaveAllValues()
    {
        PlayerPrefs.SetFloat(
            MasterVolumeKey,
            masterVolume
        );

        PlayerPrefs.SetFloat(
            MusicVolumeKey,
            musicVolume
        );

        PlayerPrefs.SetFloat(
            SFXVolumeKey,
            sfxVolume
        );

        PlayerPrefs.SetFloat(
            AmbienceVolumeKey,
            ambienceVolume
        );

        PlayerPrefs.SetInt(
            MutedKey,
            isMuted ? 1 : 0
        );

        PlayerPrefs.Save();
    }

    // Compatibilidad con código anterior.

    public void SetVolume(float value)
    {
        PreviewMasterVolume(value);
        SaveAllValues();
    }

    public void PreviewVolume(float value)
    {
        PreviewMasterVolume(value);
    }

    public void IncreaseVolume()
    {
        SetVolume(masterVolume + 0.1f);
    }

    public void DecreaseVolume()
    {
        SetVolume(masterVolume - 0.1f);
    }

    public void SaveCurrentVolume()
    {
        SaveAllValues();
    }

    public void ReloadSavedVolume()
    {
        ReloadSavedValues();
    }
}