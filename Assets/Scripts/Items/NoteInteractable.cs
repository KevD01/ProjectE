using UnityEngine;

public class NoteInteractable : MonoBehaviour
{
    [Header("Datos de la nota")]
    [SerializeField] private NoteData noteData;

    [Header("Interacción")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float interactionDistance = 1.8f;
    [SerializeField] private string interactionMessage = "Presiona E para leer la nota";

    private GameObject playerObject;
    private bool playerInside;
    private bool promptIsVisible;

    private void Update()
    {
        UpdatePrompt();

        if (NoteUI.Instance != null && NoteUI.Instance.IsOpen)
            return;

        if (NoteArchiveUI.Instance != null && NoteArchiveUI.Instance.IsOpen)
            return;

        if (playerObject == null)
            return;

        if (!playerInside)
            return;

        if (!IsPlayerCloseEnough())
            return;

        if (Input.GetKeyDown(interactKey))
        {
            ReadNote();
        }
    }

    private void UpdatePrompt()
    {
        bool shouldShowPrompt =
            playerObject != null &&
            playerInside &&
            IsPlayerCloseEnough() &&
            (NoteUI.Instance == null || !NoteUI.Instance.IsOpen) &&
            (NoteArchiveUI.Instance == null || !NoteArchiveUI.Instance.IsOpen);

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

    private void ReadNote()
    {
        if (noteData == null)
        {
            Debug.LogWarning("Esta nota no tiene NoteData asignado.");
            return;
        }

        PlayerTankController playerMovement = playerObject.GetComponent<PlayerTankController>();

        NoteArchive.Instance?.AddNote(noteData);

        if (NoteUI.Instance != null)
        {
            NoteUI.Instance.OpenNote(noteData.noteTitle, noteData.noteBody, playerMovement);
        }

        InteractionPromptUI.Instance?.Hide();
        promptIsVisible = false;
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
    }
}