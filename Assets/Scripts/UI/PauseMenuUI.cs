using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
    public static PauseMenuUI Instance;

    [Header("Referencias")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text menuBodyText;

    [Header("Input")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] private KeyCode controlsKey = KeyCode.C;
    [SerializeField] private KeyCode restartKey = KeyCode.R;
    [SerializeField] private KeyCode quitKey = KeyCode.Q;

    private bool isPaused;
    private bool showingControls;

    public bool IsPaused => isPaused;

    private void Awake()
    {
        Instance = this;

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        HidePauseMenuInstant();
    }

    private void Update()
    {
        if (GameOverUI.Instance != null && GameOverUI.Instance.IsGameOver)
            return;

        if (EndingUI.Instance != null && EndingUI.Instance.EndingActive)
            return;

        if (Input.GetKeyDown(pauseKey))
        {
            HandlePauseKey();
            return;
        }

        if (!isPaused)
            return;

        if (Input.GetKeyDown(controlsKey))
        {
            ToggleControlsScreen();
            return;
        }

        if (showingControls)
            return;

        if (Input.GetKeyDown(restartKey))
        {
            RestartScene();
            return;
        }

        if (Input.GetKeyDown(quitKey))
        {
            QuitGame();
            return;
        }
    }

    private void HandlePauseKey()
    {
        if (isPaused)
        {
            if (showingControls)
            {
                ShowMainPauseText();
                return;
            }

            ResumeGame();
            return;
        }

        if (IsAnotherUIOpen())
            return;

        PauseGame();
    }

    public void PauseGame()
    {
        if (isPaused)
            return;

        isPaused = true;
        showingControls = false;

        Time.timeScale = 0f;

        InteractionPromptUI.Instance?.Hide();

        ShowPauseMenu();
        ShowMainPauseText();
    }

    public void ResumeGame()
    {
        if (!isPaused)
            return;

        isPaused = false;
        showingControls = false;

        Time.timeScale = 1f;

        HidePauseMenuInstant();
    }

    private void ToggleControlsScreen()
    {
        if (showingControls)
        {
            ShowMainPauseText();
        }
        else
        {
            ShowControlsText();
        }
    }

    private void ShowMainPauseText()
    {
        showingControls = false;

        if (menuBodyText == null)
            return;

        menuBodyText.text =
            "Escape - Continuar\n\n" +
            "C - Ver controles\n\n" +
            "R - Reiniciar\n\n" +
            "Q - Salir del juego";
    }

    private void ShowControlsText()
    {
        showingControls = true;

        if (menuBodyText == null)
            return;

        menuBodyText.text =
            "CONTROLES\n\n" +
            "W / S - Caminar adelante / atrás\n" +
            "A / D - Girar\n" +
            "Shift - Correr\n\n" +
            "E - Interactuar / recoger / usar\n" +
            "I - Inventario\n" +
            "J - Archivo de notas\n\n" +
            "Click derecho - Apuntar\n" +
            "Click izquierdo - Disparar\n" +
            "R - Recargar\n\n" +
            "Escape - Volver\n" +
            "C - Volver al menú de pausa";
    }

    private void ShowPauseMenu()
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    private void HidePauseMenuInstant()
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private bool IsAnotherUIOpen()
    {
        if (NoteUI.Instance != null && NoteUI.Instance.IsOpen)
            return true;

        if (NoteArchiveUI.Instance != null && NoteArchiveUI.Instance.IsOpen)
            return true;

        if (InventoryUI.Instance != null && InventoryUI.Instance.IsOpen)
            return true;

        return false;
    }

    private void RestartScene()
    {
        Time.timeScale = 1f;

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    private void QuitGame()
    {
        Time.timeScale = 1f;

        Debug.Log("Saliendo del juego...");

        Application.Quit();
    }

    private void OnDisable()
    {
        if (isPaused)
        {
            Time.timeScale = 1f;
        }
    }
}