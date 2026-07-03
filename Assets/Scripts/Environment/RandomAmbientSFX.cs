using System.Collections;
using UnityEngine;

public class RandomAmbientSFX : MonoBehaviour
{
    [Header("Sonidos aleatorios")]
    [SerializeField] private AudioClip[] randomClips;

    [Header("Tiempo")]
    [SerializeField] private float minTimeBetweenSounds = 8f;
    [SerializeField] private float maxTimeBetweenSounds = 20f;

    [Header("Audio")]
    [SerializeField] private float volume = 0.7f;
    [SerializeField] private bool playOnStart = true;

    private Coroutine ambientRoutine;

    private void Start()
    {
        if (playOnStart)
        {
            StartRandomSounds();
        }
    }

    public void StartRandomSounds()
    {
        if (ambientRoutine != null)
            return;

        ambientRoutine = StartCoroutine(RandomSoundLoop());
    }

    public void StopRandomSounds()
    {
        if (ambientRoutine != null)
        {
            StopCoroutine(ambientRoutine);
            ambientRoutine = null;
        }
    }

    private IEnumerator RandomSoundLoop()
    {
        while (true)
        {
            float waitTime = Random.Range(minTimeBetweenSounds, maxTimeBetweenSounds);
            yield return new WaitForSeconds(waitTime);

            if (IsGameplayPaused())
                continue;

            PlayRandomClip();
        }
    }

    private void PlayRandomClip()
    {
        if (randomClips == null || randomClips.Length <= 0)
            return;

        AudioClip clip = randomClips[Random.Range(0, randomClips.Length)];

        if (clip == null)
            return;

        GameAudioManager.Instance?.PlaySFX(clip, volume);
    }

    private bool IsGameplayPaused()
    {
        if (NoteUI.Instance != null && NoteUI.Instance.IsOpen)
            return true;

        if (NoteArchiveUI.Instance != null && NoteArchiveUI.Instance.IsOpen)
            return true;

        if (InventoryUI.Instance != null && InventoryUI.Instance.IsOpen)
            return true;

        return false;
    }
}