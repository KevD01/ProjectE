using UnityEngine;

public class GameAudioManager : MonoBehaviour
{
    public static GameAudioManager Instance;

    [Header("Fuentes de audio")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource ambienceSource;

    [Header("Ambiente")]
    [SerializeField] private AudioClip startingAmbienceClip;
    [SerializeField] private float ambienceVolume = 0.35f;

    [Header("Pitch aleatorio")]
    [SerializeField] private float minPitch = 0.95f;
    [SerializeField] private float maxPitch = 1.05f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (startingAmbienceClip != null)
        {
            PlayAmbience(startingAmbienceClip, ambienceVolume);
        }
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null || sfxSource == null)
            return;

        sfxSource.pitch = Random.Range(minPitch, maxPitch);
        sfxSource.PlayOneShot(clip, volume);
    }

    public void PlaySFXNoPitch(AudioClip clip, float volume = 1f)
    {
        if (clip == null || sfxSource == null)
            return;

        sfxSource.pitch = 1f;
        sfxSource.PlayOneShot(clip, volume);
    }

    public void PlayAmbience(AudioClip clip, float volume = 0.35f)
    {
        if (clip == null || ambienceSource == null)
            return;

        ambienceSource.clip = clip;
        ambienceSource.volume = volume;
        ambienceSource.loop = true;
        ambienceSource.Play();
    }

    public void StopAmbience()
    {
        if (ambienceSource == null)
            return;

        ambienceSource.Stop();
    }
}