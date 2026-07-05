using System.Collections;
using UnityEngine;

public class ItemPuzzleInteractable : MonoBehaviour
{
    [Header("Objeto requerido")]
    [SerializeField] private ItemData requiredItem;
    [SerializeField] private bool consumeItemOnUse = true;

    [Header("Estado")]
    [SerializeField] private bool startsSolved = false;
    [SerializeField] private bool canInteractAfterSolved = false;

    [Header("Mensajes")]
    [TextArea(2, 4)]
    [SerializeField] private string inspectMessage = "Parece que falta una pieza.";

    [SerializeField] private string missingItemMessage = "No tienes el objeto necesario.";
    [SerializeField] private string solvedMessage = "Usaste el objeto.";
    [SerializeField] private string alreadySolvedMessage = "Ya no hace falta hacer nada aquí.";
    [SerializeField] private float messageTime = 1.5f;

    [Header("Audio")]
    [SerializeField] private AudioClip successSound;
    [SerializeField] private AudioClip failSound;
    [SerializeField] private float successVolume = 0.8f;
    [SerializeField] private float failVolume = 0.7f;

    [Header("Efectos al resolver")]
    [SerializeField] private GameObject objectToActivate;
    [SerializeField] private GameObject objectToDisable;
    [SerializeField] private Light lightToEnable;
    [SerializeField] private FlickeringLight flickeringLightToStop;

    [Header("Puertas a desbloquear")]
    [SerializeField] private DoorTransition[] doorsToUnlock;

    [Header("Interacción")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float interactionDistance = 2.3f;
    [SerializeField] private string interactionMessage = "Presiona E para revisar";

    [Header("Debug")]
    [SerializeField] private bool showInteractionRange = true;

    private GameObject playerObject;
    private bool isSolved;
    private bool promptIsVisible;
    private bool showingTemporaryMessage;

    private Coroutine temporaryMessageRoutine;

    private void Awake()
    {
        isSolved = startsSolved;
    }

    private void Start()
    {
        FindPlayer();

        if (objectToActivate != null && !isSolved)
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
            TryInteract();
        }
    }

    private void FindPlayer()
    {
        playerObject = GameObject.FindGameObjectWithTag("Player");
    }

    private void UpdatePrompt()
    {
        if (showingTemporaryMessage)
            return;

        bool shouldShowPrompt =
            playerObject != null &&
            IsPlayerCloseEnough() &&
            !IsGameplayPaused() &&
            (!isSolved || canInteractAfterSolved);

        if (shouldShowPrompt)
        {
            if (!promptIsVisible)
            {
                InteractionPromptUI.Instance?.Show(interactionMessage);
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

    private void TryInteract()
    {
        if (isSolved)
        {
            ShowTemporaryMessage(alreadySolvedMessage);
            return;
        }

        if (requiredItem == null)
        {
            ShowTemporaryMessage(inspectMessage);
            return;
        }

        if (PlayerInventory.Instance == null || !PlayerInventory.Instance.HasItem(requiredItem))
        {
            GameAudioManager.Instance?.PlaySFXNoPitch(failSound, failVolume);
            ShowTemporaryMessage(missingItemMessage);
            return;
        }

        SolvePuzzle();
    }

    private void SolvePuzzle()
    {
        isSolved = true;

        if (consumeItemOnUse && PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.RemoveItem(requiredItem, 1);
        }

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

        UnlockDoors();

        GameAudioManager.Instance?.PlaySFXNoPitch(successSound, successVolume);

        ShowTemporaryMessage(solvedMessage);

        Debug.Log(gameObject.name + " resuelto.");
    }

    private void UnlockDoors()
    {
        if (doorsToUnlock == null)
            return;

        foreach (DoorTransition door in doorsToUnlock)
        {
            if (door != null)
            {
                door.UnlockFromPuzzle();
            }
        }
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