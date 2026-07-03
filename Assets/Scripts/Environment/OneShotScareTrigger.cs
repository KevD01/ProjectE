using UnityEngine;

public class OneShotScareTrigger : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip scareSound;
    [SerializeField] private float scareSoundVolume = 1f;

    [Header("Luces")]
    [SerializeField] private FlickeringLight[] lightsToFlicker;
    [SerializeField] private float flickerDuration = 1.5f;

    [Header("Objeto opcional")]
    [SerializeField] private GameObject objectToActivate;
    [SerializeField] private GameObject objectToDisable;

    [Header("Configuración")]
    [SerializeField] private bool onlyOnce = true;

    private bool hasTriggered;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (onlyOnce && hasTriggered)
            return;

        TriggerScare();
    }

    private void TriggerScare()
    {
        hasTriggered = true;

        if (scareSound != null)
        {
            GameAudioManager.Instance?.PlaySFXNoPitch(scareSound, scareSoundVolume);
        }

        if (lightsToFlicker != null)
        {
            foreach (FlickeringLight flickeringLight in lightsToFlicker)
            {
                if (flickeringLight != null)
                {
                    flickeringLight.FlickerForSeconds(flickerDuration);
                }
            }
        }

        if (objectToActivate != null)
        {
            objectToActivate.SetActive(true);
        }

        if (objectToDisable != null)
        {
            objectToDisable.SetActive(false);
        }

        Debug.Log("Susto ambiental activado: " + gameObject.name);
    }
}