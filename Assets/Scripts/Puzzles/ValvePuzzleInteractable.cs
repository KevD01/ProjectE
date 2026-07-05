using System.Collections;
using UnityEngine;

public class ValvePuzzleInteractable : MonoBehaviour
{
    [Header("Objeto requerido")]
    [SerializeField] private ItemData requiredValveItem;
    [SerializeField] private bool consumeValveOnUse = true;

    [Header("Estado")]
    [SerializeField] private bool startsSolved = false;
    [SerializeField] private bool canInteractAfterSolved = true;

    [Header("Visual de válvula")]
    [SerializeField] private GameObject valveVisual;
    [SerializeField] private Transform valveToRotate;
    [SerializeField] private Vector3 solvedRotation = new Vector3(0f, 0f, 360f);
    [SerializeField] private float rotateTime = 1.2f;

    [Header("Efectos al resolver")]
    [SerializeField] private GameObject objectToDisable;
    [SerializeField] private GameObject objectToActivate;
    [SerializeField] private DoorTransition[] doorsToUnlock;

    [Header("Mensajes")]
    [TextArea(2, 4)]
    [SerializeField] private string inspectMessage = "Hay una tubería rota. Falta una válvula.";

    [SerializeField] private string missingValveMessage = "No tienes nada que encaje aquí.";
    [SerializeField] private string solvedMessage = "Colocaste la válvula y cerraste el paso del vapor.";
    [SerializeField] private string alreadySolvedMessage = "La válvula ya está instalada.";
    [SerializeField] private float messageTime = 1.5f;

    [Header("Audio")]
    [SerializeField] private AudioClip valveInstallSound;
    [SerializeField] private AudioClip valveTurnSound;
    [SerializeField] private AudioClip failSound;
    [SerializeField] private float installVolume = 0.8f;
    [SerializeField] private float turnVolume = 0.8f;
    [SerializeField] private float failVolume = 0.7f;

    [Header("Interacción")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float interactionDistance = 2.3f;
    [SerializeField] private string interactionMessage = "Presiona E para revisar la tubería";

    [Header("Debug")]
    [SerializeField] private bool showInteractionRange = true;

    private GameObject playerObject;
    private bool isSolved;
    private bool isSolving;
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

        if (valveVisual != null)
        {
            valveVisual.SetActive(isSolved);
        }

        if (objectToActivate != null && !isSolved)
        {
            objectToActivate.SetActive(false);
        }

        if (objectToDisable != null && isSolved)
        {
            objectToDisable.SetActive(false);
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
            !isSolving &&
            (!isSolved || canInteractAfterSolved);

        if (shouldShowPrompt)
        {
            if (!promptIsVisible)
            {
                string message = isSolved ? alreadySolvedMessage : interactionMessage;
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

    private void TryInteract()
    {
        if (isSolving)
            return;

        if (isSolved)
        {
            ShowTemporaryMessage(alreadySolvedMessage);
            return;
        }

        if (requiredValveItem == null)
        {
            ShowTemporaryMessage(inspectMessage);
            return;
        }

        if (PlayerInventory.Instance == null || !PlayerInventory.Instance.HasItem(requiredValveItem))
        {
            GameAudioManager.Instance?.PlaySFXNoPitch(failSound, failVolume);
            ShowTemporaryMessage(missingValveMessage);
            return;
        }

        StartCoroutine(SolveRoutine());
    }

    private IEnumerator SolveRoutine()
    {
        isSolving = true;
        promptIsVisible = false;

        InteractionPromptUI.Instance?.Hide();

        if (consumeValveOnUse && PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.RemoveItem(requiredValveItem, 1);
        }

        if (valveVisual != null)
        {
            valveVisual.SetActive(true);
        }

        GameAudioManager.Instance?.PlaySFXNoPitch(valveInstallSound, installVolume);

        yield return new WaitForSeconds(0.25f);

        GameAudioManager.Instance?.PlaySFXNoPitch(valveTurnSound, turnVolume);

        yield return RotateValveRoutine();

        ResolveEffects();

        isSolved = true;
        isSolving = false;

        ShowTemporaryMessage(solvedMessage);

        Debug.Log(gameObject.name + " resuelto con válvula.");
    }

    private IEnumerator RotateValveRoutine()
    {
        if (valveToRotate == null)
            yield break;

        Quaternion startRotation = valveToRotate.localRotation;
        Quaternion targetRotation = Quaternion.Euler(solvedRotation);

        float timer = 0f;

        while (timer < rotateTime)
        {
            timer += Time.deltaTime;
            float t = timer / rotateTime;

            valveToRotate.localRotation = Quaternion.Slerp(startRotation, targetRotation, t);

            yield return null;
        }

        valveToRotate.localRotation = targetRotation;
    }

    private void ResolveEffects()
    {
        if (objectToDisable != null)
        {
            objectToDisable.SetActive(false);
        }

        if (objectToActivate != null)
        {
            objectToActivate.SetActive(true);
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