using System.Collections;
using UnityEngine;

public class KeypadPuzzleInteractable : MonoBehaviour
{
    [Header("Código")]
    [SerializeField] private string correctCode = "1934";
    [SerializeField] private int codeLength = 4;
    [SerializeField] private bool hideCodeInput = false;

    [Header("Estado")]
    [SerializeField] private bool startsSolved = false;
    [SerializeField] private bool canInteractAfterSolved = true;

    [Header("Mensajes")]
    [TextArea(2, 4)]
    [SerializeField] private string inspectMessage = "Hay un teclado numérico.";

    [SerializeField] private string solvedMessage = "La caja fuerte se abrió.";
    [SerializeField] private string wrongCodeMessage = "Código incorrecto.";
    [SerializeField] private string incompleteCodeMessage = "El código está incompleto.";
    [SerializeField] private string alreadySolvedMessage = "La caja fuerte ya está abierta.";
    [SerializeField] private float messageTime = 1.2f;

    [Header("Audio")]
    [SerializeField] private AudioClip buttonSound;
    [SerializeField] private AudioClip successSound;
    [SerializeField] private AudioClip failSound;
    [SerializeField] private float buttonVolume = 0.5f;
    [SerializeField] private float successVolume = 0.8f;
    [SerializeField] private float failVolume = 0.7f;

    [Header("Efectos al resolver")]
    [SerializeField] private GameObject objectToActivate;
    [SerializeField] private GameObject objectToDisable;

    [Header("Puertas a desbloquear opcional")]
    [SerializeField] private DoorTransition[] doorsToUnlock;

    [Header("Interacción")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float interactionDistance = 2.3f;
    [SerializeField] private string interactionMessage = "Presiona E para usar el teclado";

    [Header("Debug")]
    [SerializeField] private bool showInteractionRange = true;

    private GameObject playerObject;
    private PlayerTankController playerMovement;

    private bool isSolved;
    private bool isInputMode;
    private bool promptIsVisible;
    private bool showingTemporaryMessage;

    private string currentInput = "";
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

        if (isInputMode)
        {
            HandleCodeInput();
            return;
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
            TryStartInputMode();
        }
    }

    private void FindPlayer()
    {
        playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            playerMovement = playerObject.GetComponent<PlayerTankController>();
        }
    }

    private void TryStartInputMode()
    {
        if (isSolved)
        {
            ShowTemporaryMessage(alreadySolvedMessage);
            return;
        }

        StartInputMode();
    }

    private void StartInputMode()
    {
        isInputMode = true;
        currentInput = "";
        promptIsVisible = false;

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        InteractionPromptUI.Instance?.Show(BuildInputMessage());
    }

    private void HandleCodeInput()
    {
        if (showingTemporaryMessage)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelInputMode();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            if (currentInput.Length > 0)
            {
                currentInput = currentInput.Substring(0, currentInput.Length - 1);
                GameAudioManager.Instance?.PlaySFXNoPitch(buttonSound, buttonVolume);
            }
        }

        int pressedDigit = GetPressedDigit();

        if (pressedDigit >= 0 && currentInput.Length < codeLength)
        {
            currentInput += pressedDigit.ToString();
            GameAudioManager.Instance?.PlaySFXNoPitch(buttonSound, buttonVolume);
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SubmitCode();
            return;
        }

        InteractionPromptUI.Instance?.Show(BuildInputMessage());
    }

    private int GetPressedDigit()
    {
        for (int i = 0; i <= 9; i++)
        {
            KeyCode alphaKey = (KeyCode)((int)KeyCode.Alpha0 + i);
            KeyCode keypadKey = (KeyCode)((int)KeyCode.Keypad0 + i);

            if (Input.GetKeyDown(alphaKey) || Input.GetKeyDown(keypadKey))
            {
                return i;
            }
        }

        return -1;
    }

    private void SubmitCode()
    {
        if (currentInput.Length < codeLength)
        {
            GameAudioManager.Instance?.PlaySFXNoPitch(failSound, failVolume);
            ShowTemporaryMessage(incompleteCodeMessage);
            return;
        }

        if (currentInput == correctCode)
        {
            SolvePuzzle();
            return;
        }

        currentInput = "";

        GameAudioManager.Instance?.PlaySFXNoPitch(failSound, failVolume);
        ShowTemporaryMessage(wrongCodeMessage);
    }

    private void SolvePuzzle()
    {
        isSolved = true;
        isInputMode = false;
        promptIsVisible = false;

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        if (objectToActivate != null)
        {
            objectToActivate.SetActive(true);
        }

        if (objectToDisable != null)
        {
            objectToDisable.SetActive(false);
        }

        UnlockDoors();

        GameAudioManager.Instance?.PlaySFXNoPitch(successSound, successVolume);

        ShowTemporaryMessage(solvedMessage);

        Debug.Log(gameObject.name + " resuelto con código.");
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

    private string BuildInputMessage()
    {
        string visibleInput = hideCodeInput ? new string('*', currentInput.Length) : currentInput;
        string blanks = new string('_', Mathf.Max(0, codeLength - currentInput.Length));

        return
            inspectMessage +
            "\nCódigo: " + visibleInput + blanks +
            "\nNúmeros: escribir     Enter: confirmar     Escape: salir";
    }

    private void CancelInputMode()
    {
        isInputMode = false;
        currentInput = "";
        promptIsVisible = false;

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        InteractionPromptUI.Instance?.Hide();
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

        showingTemporaryMessage = false;

        if (isInputMode)
        {
            InteractionPromptUI.Instance?.Show(BuildInputMessage());
        }
        else
        {
            InteractionPromptUI.Instance?.Hide();
        }
    }

    private void OnDisable()
    {
        if (isInputMode && playerMovement != null)
        {
            playerMovement.enabled = true;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!showInteractionRange)
            return;

        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}