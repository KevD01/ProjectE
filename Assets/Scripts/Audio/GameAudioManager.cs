using System.Collections;
using UnityEngine;

public class GameAudioManager : MonoBehaviour
{
    public static GameAudioManager Instance;

    [Header("Fuentes de audio")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource ambienceSource;

    [Header("Ambiente inicial")]
    [SerializeField] private AudioClip startingAmbienceClip;
    [SerializeField] private float ambienceVolume = 0.35f;

    [Header("Fade de ambiente")]
    [SerializeField] private float defaultFadeTime = 1.5f;

    [Header("Pitch aleatorio SFX")]
    [SerializeField] private float minPitch = 0.95f;
    [SerializeField] private float maxPitch = 1.05f;

    private AudioClip currentAmbienceClip;
    private Coroutine ambienceFadeRoutine;

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

        currentAmbienceClip = clip;

        ambienceSource.clip = clip;
        ambienceSource.volume = volume;
        ambienceSource.loop = true;
        ambienceSource.Play();
    }

    public void ChangeAmbience(AudioClip newClip, float targetVolume)
    {
        ChangeAmbience(newClip, targetVolume, defaultFadeTime);
    }

    public void ChangeAmbience(AudioClip newClip, float targetVolume, float fadeTime)
    {
        if (newClip == null || ambienceSource == null)
            return;

        if (currentAmbienceClip == newClip && ambienceSource.isPlaying)
            return;

        if (ambienceFadeRoutine != null)
        {
            StopCoroutine(ambienceFadeRoutine);
        }

        ambienceFadeRoutine = StartCoroutine(ChangeAmbienceRoutine(newClip, targetVolume, fadeTime));
    }

    private IEnumerator ChangeAmbienceRoutine(AudioClip newClip, float targetVolume, float fadeTime)
    {
        float startVolume = ambienceSource.volume;
        float timer = 0f;

        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            float t = timer / fadeTime;

            ambienceSource.volume = Mathf.Lerp(startVolume, 0f, t);

            yield return null;
        }

        ambienceSource.Stop();

        currentAmbienceClip = newClip;
        ambienceSource.clip = newClip;
        ambienceSource.loop = true;
        ambienceSource.Play();

        timer = 0f;

        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            float t = timer / fadeTime;

            ambienceSource.volume = Mathf.Lerp(0f, targetVolume, t);

            yield return null;
        }

        ambienceSource.volume = targetVolume;
        ambienceFadeRoutine = null;
    }

    public void StopAmbience()
    {
        if (ambienceSource == null)
            return;

        ambienceSource.Stop();
        currentAmbienceClip = null;
    }
}