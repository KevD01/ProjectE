using System.Collections;
using UnityEngine;

public class LeverInteractable : MonoBehaviour
{
    [Header("Estado")]
    [SerializeField] private bool startsActivated = false;
    [SerializeField] private bool canUseOnlyOnce = true;

    [Header("Visual de palanca")]
    [SerializeField] private Transform leverHandle;
    [SerializeField] private Vector3 inactiveRotation = new Vector3(0f, 0f, 0f);
    [SerializeField] private Vector3 activeRotation = new Vector3(-60f, 0f, 0f);
    [SerializeField] private float leverMoveTime = 0.35f;

    [Header("Mensajes")]
    [SerializeField] private string interactionMessage = "Presiona E para bajar la palanca";
    [SerializeField] private string activatedMessage = "La energía vuelve a circular.";
    [SerializeField] private string alreadyActivatedMessage = "La palanca ya fue activada.";
    [SerializeField] private float messageTime = 1.4f;

    [Header("Audio")]
    [SerializeField] private AudioClip leverSound;
    [SerializeField] private AudioClip alreadyActivatedSound;
    [SerializeField] private float leverVolume = 0.8f;
    [SerializeField] private float alreadyActivatedVolume = 0.6f;

    [Header("Efectos al activar")]
    [SerializeField] private GameObject objectToActivate;
    [SerializeField] private GameObject objectToDisable;
    [SerializeField] private Light lightToEnable;
    [SerializeField] private FlickeringLight flickeringLightToStop;
    [SerializeField] private DoorTransition[] doorsToUnlock;

    [Header("Interacción")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float interactionDistance = 2.3f;

    [Header("Debug")]
    [SerializeField] private bool showInteractionRange = true;

    private GameObject playerObject;
    private bool isActivated;
    private bool isMoving;
    private bool promptIsVisible;
    private bool showingTemporaryMessage;

    private Coroutine temporaryMessageRoutine;

    private void Awake()
    {
        isActivated = startsActivated;
    }

    private void Start()
    {
        FindPlayer();
        ApplyInitialVisualState();

        if (objectToActivate != null && !isActivated)
        {
            objectToActivate.SetActive(false);
        }
    }

    private void Update()
    {
        if (playerObject == null)
        {
            FindPlayer();
        }

        UpdatePrompt();

        if (IsGameplayPaused())
            return;

        if (playerObject == null)
            return;

        if (!IsPlayerCloseEnough())
            return;

        if (Input.GetKeyDown(interactKey))
        {
            TryUseLever();
        }
    }

    private void FindPlayer()
    {
        playerObject = GameObject.FindGameObjectWithTag("Player");
    }

    private void ApplyInitialVisualState()
    {
        if (leverHandle == null)
            return;

        leverHandle.localEulerAngles = isActivated ? activeRotation : inactiveRotation;
    }

    private void UpdatePrompt()
    {
        if (showingTemporaryMessage)
            return;

        bool shouldShowPrompt =
            playerObject != null &&
            IsPlayerCloseEnough() &&
            !IsGameplayPaused();

        if (shouldShowPrompt)
        {
            if (!promptIsVisible)
            {
                string message = isActivated && canUseOnlyOnce
                    ? alreadyActivatedMessage
                    : interactionMessage;

                InteractionPromptUI.Instance?.Show(message);
                promptIsVisible = true;
            }
        }
        else
        {
            if (promptIsVisible)
            {
                InteractionPromptUI.Instance?.Hide();
                promptIsVisible = false;
            }
        }
    }

    private void TryUseLever()
    {
        if (isMoving)
            return;

        if (isActivated && canUseOnlyOnce)
        {
            GameAudioManager.Instance?.PlaySFXNoPitch(alreadyActivatedSound, alreadyActivatedVolume);
            ShowTemporaryMessage(alreadyActivatedMessage);
            return;
        }

        StartCoroutine(ActivateLeverRoutine());
    }

    private IEnumerator ActivateLeverRoutine()
    {
        isMoving = true;
        isActivated = true;

        promptIsVisible = false;
        InteractionPromptUI.Instance?.Hide();

        GameAudioManager.Instance?.PlaySFXNoPitch(leverSound, leverVolume);

        yield return MoveLeverRoutine();

        ActivateEffects();

        ShowTemporaryMessage(activatedMessage);

        isMoving = false;
    }

    private IEnumerator MoveLeverRoutine()
    {
        if (leverHandle == null)
            yield break;

        Quaternion startRotation = leverHandle.localRotation;
        Quaternion targetRotation = Quaternion.Euler(activeRotation);

        float timer = 0f;

        while (timer < leverMoveTime)
        {
            timer += Time.deltaTime;
            float t = timer / leverMoveTime;

            leverHandle.localRotation = Quaternion.Slerp(startRotation, targetRotation, t);

            yield return null;
        }

        leverHandle.localRotation = targetRotation;
    }

    private void ActivateEffects()
    {
        if (objectToActivate != null)
        {
            objectToActivate.SetActive(true);
        }

        if (objectToDisable != null)
        {
            objectToDisable.SetActive(false);
        }

        if (lightToEnable != null)
        {
            lightToEnable.enabled = true;
        }

        if (flickeringLightToStop != null)
        {
            flickeringLightToStop.StopFlicker();
        }

        if (doorsToUnlock != null)
        {
            foreach (DoorTransition door in doorsToUnlock)
            {
                if (door != null)
                {
                    door.UnlockFromPuzzle();
                }
            }
        }

        Debug.Log(gameObject.name + " activó sus efectos.");
    }

    private bool IsPlayerCloseEnough()
    {
        if (playerObject == null)
            return false;

        float distance = Vector3.Distance(transform.position, playerObject.transform.position);
        return distance <= interactionDistance;
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

    private void ShowTemporaryMessage(string message)
    {
        if (temporaryMessageRoutine != null)
        {
            StopCoroutine(temporaryMessageRoutine);
        }

        temporaryMessageRoutine = StartCoroutine(TemporaryMessageRoutine(message));
    }

    private IEnumerator TemporaryMessageRoutine(string message)
    {
        showingTemporaryMessage = true;
        promptIsVisible = false;

        InteractionPromptUI.Instance?.Show(message);

        yield return new WaitForSeconds(messageTime);

        InteractionPromptUI.Instance?.Hide();

        showingTemporaryMessage = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (!showInteractionRange)
            return;

        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}