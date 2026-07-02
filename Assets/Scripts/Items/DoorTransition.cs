using System.Collections;
using UnityEngine;

public class DoorTransition : MonoBehaviour
{
    [Header("Destino")]
    [SerializeField] private Transform destinationSpawnPoint;
    [SerializeField] private Transform destinationCameraPoint;

    [Header("Llave requerida")]
    [SerializeField] private bool requiresKey = false;
    [SerializeField] private ItemData requiredKey;
    [SerializeField] private string lockedMessage = "Está cerrada. Necesitas una llave.";

    [Header("Transición")]
    [SerializeField] private float fadeOutTime = 0.4f;
    [SerializeField] private float fadeInTime = 0.4f;
    [SerializeField] private float waitInBlackTime = 0.2f;

    [Header("Interacción")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float interactionDistance = 1.8f;
    [SerializeField] private string interactionMessage = "Presiona E para abrir la puerta";

    private GameObject playerObject;
    private bool playerInside;
    private bool isTransitioning;
    private bool promptIsVisible;
    private bool showingTemporaryMessage;

    private Coroutine temporaryMessageRoutine;

    private void Update()
    {
        UpdatePrompt();

        if (isTransitioning)
            return;

        if (NoteUI.Instance != null && NoteUI.Instance.IsOpen)
            return;

        if (NoteArchiveUI.Instance != null && NoteArchiveUI.Instance.IsOpen)
            return;

        if (InventoryUI.Instance != null && InventoryUI.Instance.IsOpen)
            return;

        if (playerObject == null)
            return;

        if (!playerInside)
            return;

        if (!IsPlayerCloseEnough())
            return;

        if (Input.GetKeyDown(interactKey))
        {
            TryUseDoor();
        }
    }

    private void TryUseDoor()
    {
        if (requiresKey)
        {
            if (requiredKey == null)
            {
                ShowTemporaryMessage("Esta puerta necesita llave, pero no tiene una asignada.");
                Debug.LogWarning(gameObject.name + " requiere llave, pero Required Key está vacío.");
                return;
            }

            if (PlayerInventory.Instance == null || !PlayerInventory.Instance.HasItem(requiredKey))
            {
                ShowTemporaryMessage(lockedMessage);
                return;
            }
        }

        StartCoroutine(UseDoor());
    }

    private void UpdatePrompt()
    {
        if (showingTemporaryMessage)
            return;

        bool shouldShowPrompt =
            !isTransitioning &&
            playerObject != null &&
            playerInside &&
            IsPlayerCloseEnough() &&
            (NoteUI.Instance == null || !NoteUI.Instance.IsOpen) &&
            (NoteArchiveUI.Instance == null || !NoteArchiveUI.Instance.IsOpen) &&
            (InventoryUI.Instance == null || !InventoryUI.Instance.IsOpen);

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

    private bool IsPlayerCloseEnough()
    {
        if (playerObject == null)
            return false;

        float distance = Vector3.Distance(transform.position, playerObject.transform.position);
        return distance <= interactionDistance;
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

        yield return new WaitForSeconds(1.5f);

        InteractionPromptUI.Instance?.Hide();

        showingTemporaryMessage = false;
    }

    private IEnumerator UseDoor()
    {
        if (playerObject == null || destinationSpawnPoint == null)
            yield break;

        isTransitioning = true;

        InteractionPromptUI.Instance?.Hide();
        promptIsVisible = false;
        showingTemporaryMessage = false;

        GameObject currentPlayer = playerObject;

        playerInside = false;
        playerObject = null;

        PlayerTankController playerMovement = currentPlayer.GetComponent<PlayerTankController>();

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
            playerMovement.ResetVerticalVelocity();
        }

        if (ScreenFader.Instance != null)
        {
            yield return ScreenFader.Instance.FadeOut(fadeOutTime);
        }

        yield return new WaitForSeconds(waitInBlackTime);

        if (playerMovement != null)
        {
            playerMovement.TeleportTo(destinationSpawnPoint);
        }
        else
        {
            currentPlayer.transform.SetPositionAndRotation(
                destinationSpawnPoint.position,
                destinationSpawnPoint.rotation
            );

            Physics.SyncTransforms();
        }

        if (FixedCameraManager.Instance != null)
        {
            FixedCameraManager.Instance.ClearActiveZones();

            if (destinationCameraPoint != null)
            {
                FixedCameraManager.Instance.ChangeCamera(destinationCameraPoint);
            }
        }

        yield return null;

        if (ScreenFader.Instance != null)
        {
            yield return ScreenFader.Instance.FadeIn(fadeInTime);
        }

        if (playerMovement != null)
        {
            playerMovement.ResetVerticalVelocity();
            playerMovement.enabled = true;
        }

        isTransitioning = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerObject = other.gameObject;
        playerInside = true;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerObject = other.gameObject;
        playerInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (playerObject == other.gameObject)
        {
            playerInside = false;
            playerObject = null;
        }

        InteractionPromptUI.Instance?.Hide();
        promptIsVisible = false;
        showingTemporaryMessage = false;
    }
}