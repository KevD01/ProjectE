using System.Collections;
using UnityEngine;

public class SampleAnalyzerPuzzle : MonoBehaviour
{
    [Header("Objeto requerido")]
    [SerializeField] private ItemData requiredItem;
    [SerializeField] private bool consumeItemOnUse = true;

    [Header("Interacción")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float interactionDistance = 2.2f;
    [SerializeField] private string interactionMessage = "Presiona E para usar el analizador";

    [Header("Mensajes")]
    [SerializeField] private string missingItemMessage = "Necesitas una muestra para analizar.";
    [SerializeField] private string analyzingMessage = "Analizando muestra...";
    [SerializeField] private string solvedMessage = "Análisis completo. Algo se desbloqueó en el laboratorio.";
    [SerializeField] private string alreadySolvedMessage = "El análisis ya fue completado.";
    [SerializeField] private float analyzingTime = 2f;
    [SerializeField] private float messageTime = 1.5f;

    [Header("Eventos al completar")]
    [SerializeField] private GameObject[] objectsToActivateOnSolved;
    [SerializeField] private GameObject[] objectsToDisableOnSolved;
    [SerializeField] private DoorTransition[] doorsToUnlockOnSolved;

    [Header("Audio")]
    [SerializeField] private AudioClip analyzingSound;
    [SerializeField] private AudioClip solvedSound;
    [SerializeField] private AudioClip alarmSound;
    [SerializeField] private float analyzingVolume = 0.8f;
    [SerializeField] private float solvedVolume = 0.8f;
    [SerializeField] private float alarmVolume = 1f;

    [Header("Debug")]
    [SerializeField] private bool showDebug = true;

    private GameObject playerObject;
    private bool isSolved;
    private bool isWorking;
    private bool promptIsVisible;
    private bool showingTemporaryMessage;

    private void Start()
    {
        playerObject = GameObject.FindGameObjectWithTag("Player");
    }

    private void Update()
    {
        if (isWorking)
            return;

        if (IsGameplayPaused())
        {
            HidePrompt();
            return;
        }

        if (playerObject == null)
        {
            playerObject = GameObject.FindGameObjectWithTag("Player");
            return;
        }

        if (IsPlayerCloseEnough())
        {
            ShowPrompt();

            if (Input.GetKeyDown(interactKey))
            {
                TryUseAnalyzer();
            }
        }
        else
        {
            HidePrompt();
        }
    }

    private bool IsPlayerCloseEnough()
    {
        if (playerObject == null)
            return false;

        float distance = Vector3.Distance(transform.position, playerObject.transform.position);
        return distance <= interactionDistance;
    }

    private void ShowPrompt()
    {
        if (promptIsVisible || showingTemporaryMessage)
            return;

        if (isSolved)
        {
            InteractionPromptUI.Instance?.Show(alreadySolvedMessage);
        }
        else
        {
            InteractionPromptUI.Instance?.Show(interactionMessage);
        }

        promptIsVisible = true;
    }

    private void HidePrompt()
    {
        if (!promptIsVisible)
            return;

        InteractionPromptUI.Instance?.Hide();
        promptIsVisible = false;
    }

    private void TryUseAnalyzer()
    {
        if (isSolved)
        {
            StartCoroutine(TemporaryMessageRoutine(alreadySolvedMessage));
            return;
        }

        if (requiredItem == null)
        {
            StartCoroutine(TemporaryMessageRoutine("El analizador no tiene objeto requerido asignado."));
            Debug.LogWarning(gameObject.name + " no tiene Required Item asignado.");
            return;
        }

        if (PlayerInventory.Instance == null)
        {
            StartCoroutine(TemporaryMessageRoutine("No se encontró el inventario del jugador."));
            return;
        }

        if (!PlayerInventory.Instance.HasItem(requiredItem))
        {
            StartCoroutine(TemporaryMessageRoutine(missingItemMessage));
            return;
        }

        StartCoroutine(AnalyzeRoutine());
    }

    private IEnumerator AnalyzeRoutine()
    {
        isWorking = true;
        promptIsVisible = false;

        InteractionPromptUI.Instance?.Show(analyzingMessage);

        GameAudioManager.Instance?.PlaySFXNoPitch(analyzingSound, analyzingVolume);

        yield return new WaitForSeconds(analyzingTime);

        if (consumeItemOnUse && PlayerInventory.Instance != null && requiredItem != null)
        {
            PlayerInventory.Instance.RemoveItem(requiredItem, 1);
        }

        isSolved = true;

        ActivateObjects();
        DisableObjects();
        UnlockDoors();

        GameAudioManager.Instance?.PlaySFXNoPitch(solvedSound, solvedVolume);
        GameAudioManager.Instance?.PlaySFXNoPitch(alarmSound, alarmVolume);

        InteractionPromptUI.Instance?.Show(solvedMessage);

        if (showDebug)
        {
            Debug.Log(gameObject.name + " completó el análisis de muestra.");
        }

        yield return new WaitForSeconds(messageTime);

        InteractionPromptUI.Instance?.Hide();

        isWorking = false;
    }

    private void ActivateObjects()
    {
        if (objectsToActivateOnSolved == null)
            return;

        foreach (GameObject objectToActivate in objectsToActivateOnSolved)
        {
            if (objectToActivate != null)
            {
                objectToActivate.SetActive(true);
            }
        }
    }

    private void DisableObjects()
    {
        if (objectsToDisableOnSolved == null)
            return;

        foreach (GameObject objectToDisable in objectsToDisableOnSolved)
        {
            if (objectToDisable != null)
            {
                objectToDisable.SetActive(false);
            }
        }
    }

    private void UnlockDoors()
    {
        if (doorsToUnlockOnSolved == null)
            return;

        foreach (DoorTransition door in doorsToUnlockOnSolved)
        {
            if (door != null)
            {
                door.UnlockFromPuzzle();
            }
        }
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

    private bool IsGameplayPaused()
    {
        if (GameOverUI.Instance != null && GameOverUI.Instance.IsGameOver)
            return true;

        if (NoteUI.Instance != null && NoteUI.Instance.IsOpen)
            return true;

        if (NoteArchiveUI.Instance != null && NoteArchiveUI.Instance.IsOpen)
            return true;

        if (InventoryUI.Instance != null && InventoryUI.Instance.IsOpen)
            return true;

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}