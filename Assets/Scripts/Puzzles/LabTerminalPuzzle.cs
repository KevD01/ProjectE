using System.Collections;
using UnityEngine;

public class LabTerminalPuzzle : MonoBehaviour
{
    [Header("Código")]
    [SerializeField] private string correctCode = "2741";
    [SerializeField] private int maxCodeLength = 4;

    [Header("Interacción")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float interactionDistance = 2.2f;
    [SerializeField] private string interactionMessage = "Presiona E para usar el terminal";

    [Header("Mensajes")]
    [SerializeField] private string terminalTitle = "TERMINAL DEL LABORATORIO";
    [SerializeField] private string solvedMessage = "Acceso concedido. El gabinete se abrió.";
    [SerializeField] private string wrongCodeMessage = "Código incorrecto.";
    [SerializeField] private string alreadySolvedMessage = "Terminal desbloqueado. El gabinete ya está abierto.";
    [SerializeField] private float messageTime = 1.3f;

    [Header("Eventos al resolver")]
    [SerializeField] private GameObject[] objectsToActivateOnSolved;
    [SerializeField] private GameObject[] objectsToDisableOnSolved;
    [SerializeField] private DoorTransition[] doorsToUnlockOnSolved;

    [Header("Audio")]
    [SerializeField] private AudioClip typingSound;
    [SerializeField] private AudioClip correctSound;
    [SerializeField] private AudioClip wrongSound;
    [SerializeField] private float typingVolume = 0.4f;
    [SerializeField] private float correctVolume = 0.8f;
    [SerializeField] private float wrongVolume = 0.7f;

    [Header("Debug")]
    [SerializeField] private bool showDebug = true;

    private GameObject playerObject;
    private PlayerTankController playerMovement;

    private bool playerClose;
    private bool promptIsVisible;
    private bool isUsingTerminal;
    private bool isSolved;
    private bool showingTemporaryMessage;

    private string currentInput = "";
    private Coroutine messageRoutine;

    private void Start()
    {
        FindPlayer();
    }

    private void Update()
    {
        if (isUsingTerminal)
        {
            HandleTerminalInput();
            return;
        }

        if (IsGameplayPaused())
        {
            HidePrompt();
            return;
        }

        if (playerObject == null)
        {
            FindPlayer();
            return;
        }

        playerClose = IsPlayerCloseEnough();

        if (playerClose)
        {
            ShowPrompt();

            if (Input.GetKeyDown(interactKey))
            {
                TryUseTerminal();
            }
        }
        else
        {
            HidePrompt();
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

    private void TryUseTerminal()
    {
        if (isSolved)
        {
            ShowTemporaryMessage(alreadySolvedMessage);
            return;
        }

        OpenTerminal();
    }

    private void OpenTerminal()
    {
        isUsingTerminal = true;
        currentInput = "";

        HidePrompt();

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        RefreshTerminalText();
    }

    private void CloseTerminal()
    {
        isUsingTerminal = false;
        currentInput = "";

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        InteractionPromptUI.Instance?.Hide();
    }

    private void HandleTerminalInput()
    {
        if (GameOverUI.Instance != null && GameOverUI.Instance.IsGameOver)
        {
            CloseTerminal();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseTerminal();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            if (currentInput.Length > 0)
            {
                currentInput = currentInput.Substring(0, currentInput.Length - 1);
                GameAudioManager.Instance?.PlaySFXNoPitch(typingSound, typingVolume);
                RefreshTerminalText();
            }

            return;
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            CheckCode();
            return;
        }

        string input = Input.inputString;

        if (string.IsNullOrEmpty(input))
            return;

        foreach (char character in input)
        {
            if (!char.IsDigit(character))
                continue;

            if (currentInput.Length >= maxCodeLength)
                continue;

            currentInput += character;
            GameAudioManager.Instance?.PlaySFXNoPitch(typingSound, typingVolume);
            RefreshTerminalText();
        }
    }

    private void RefreshTerminalText()
    {
        string hiddenCode = currentInput;

        while (hiddenCode.Length < maxCodeLength)
        {
            hiddenCode += "_";
        }

        string message =
            terminalTitle +
            "\n\nCódigo: " + hiddenCode +
            "\n\nNúmeros para escribir" +
            "\nEnter para confirmar" +
            "\nBackspace para borrar" +
            "\nEscape para salir";

        InteractionPromptUI.Instance?.Show(message);
    }

    private void CheckCode()
    {
        if (currentInput == correctCode)
        {
            SolvePuzzle();
            return;
        }

        GameAudioManager.Instance?.PlaySFXNoPitch(wrongSound, wrongVolume);

        if (messageRoutine != null)
        {
            StopCoroutine(messageRoutine);
        }

        messageRoutine = StartCoroutine(WrongCodeRoutine());
    }

    private IEnumerator WrongCodeRoutine()
    {
        InteractionPromptUI.Instance?.Show(wrongCodeMessage);

        yield return new WaitForSeconds(messageTime);

        currentInput = "";
        RefreshTerminalText();

        messageRoutine = null;
    }

    private void SolvePuzzle()
    {
        if (isSolved)
            return;

        isSolved = true;
        isUsingTerminal = false;

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        ActivateObjects();
        DisableObjects();
        UnlockDoors();

        GameAudioManager.Instance?.PlaySFXNoPitch(correctSound, correctVolume);

        ShowTemporaryMessage(solvedMessage);

        if (showDebug)
        {
            Debug.Log(gameObject.name + " fue resuelto con el código: " + correctCode);
        }
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

    private void ShowTemporaryMessage(string message)
    {
        if (messageRoutine != null)
        {
            StopCoroutine(messageRoutine);
        }

        messageRoutine = StartCoroutine(TemporaryMessageRoutine(message));
    }

    private IEnumerator TemporaryMessageRoutine(string message)
    {
        showingTemporaryMessage = true;
        promptIsVisible = false;

        InteractionPromptUI.Instance?.Show(message);

        yield return new WaitForSeconds(messageTime);

        InteractionPromptUI.Instance?.Hide();

        showingTemporaryMessage = false;
        messageRoutine = null;
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

    private void OnDisable()
    {
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}