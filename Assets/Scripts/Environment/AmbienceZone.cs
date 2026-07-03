using UnityEngine;

public class AmbienceZone : MonoBehaviour
{
    [Header("Ambiente")]
    [SerializeField] private AudioClip ambienceClip;
    [SerializeField] private float volume = 0.35f;
    [SerializeField] private float fadeTime = 1.5f;

    [Header("Configuración")]
    [SerializeField] private bool onlyPlayer = true;

    private void OnTriggerEnter(Collider other)
    {
        if (onlyPlayer && !other.CompareTag("Player"))
            return;

        if (ambienceClip == null)
        {
            Debug.LogWarning(gameObject.name + " no tiene Ambience Clip asignado.");
            return;
        }

        if (GameAudioManager.Instance != null)
        {
            GameAudioManager.Instance.ChangeAmbience(ambienceClip, volume, fadeTime);
        }
    }
}