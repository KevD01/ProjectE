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
    [SerializeField] private KeyCode volumeKey = KeyCode.V;
    [SerializeField] private KeyCode restartKey = KeyCode.R;
    [SerializeField] private KeyCode quitKey = KeyCode.Q;

    private bool isPaused;
    private bool showingControls;
    private bool showingVolume;

    private VolumeSettingsManager volumeSettings;

    public bool IsPaused => isPaused;

    private void Awake()
    {
        Instance = this;

        volumeSettings = VolumeSettingsManager.EnsureExists();

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

        if (showingVolume)
        {
            HandleVolumeInput();
            return;
        }

        if (Input.GetKeyDown(controlsKey))
        {
            ToggleControlsScreen();
            return;
        }

        if (showingControls)
            return;

        if (Input.GetKeyDown(volumeKey))
        {
            ShowVolumeText();
            return;
        }

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
            if (showingControls || showingVolume)
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

    private void HandleVolumeInput()
    {
        if (Input.GetKeyDown(volumeKey))
        {
            ShowMainPauseText();
            return;
        }

        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            volumeSettings.DecreaseVolume();
            ShowVolumeText();
            return;
        }

        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            volumeSettings.IncreaseVolume();
            ShowVolumeText();
            return;
        }
    }

    public void PauseGame()
    {
        if (isPaused)
            return;

        isPaused = true;
        showingControls = false;
        showingVolume = false;

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
        showingVolume = false;

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
        showingVolume = false;

        if (menuBodyText == null)
            return;

        menuBodyText.text =
            "Escape - Continuar\n\n" +
            "C - Ver controles\n\n" +
            "V - Volumen\n\n" +
            "R - Reiniciar escena\n\n" +
            "M - Menú principal\n\n" +
            "Q - Salir del juego";
    }

    private void ShowControlsText()
    {
        showingControls = true;
        showingVolume = false;

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
            "C - Volver al menú de pausa\n" +
            "V - Volumen\n" +
            "M - Menú principal";
    }

    private void ShowVolumeText()
    {
        showingControls = false;
        showingVolume = true;

        if (volumeSettings == null)
        {
            volumeSettings = VolumeSettingsManager.EnsureExists();
        }

        if (menuBodyText == null)
            return;

        menuBodyText.text =
            "VOLUMEN\n\n" +
            "Volumen actual: " + volumeSettings.MasterVolumePercent + "%\n\n" +
            "A / Flecha izquierda - Bajar\n" +
            "D / Flecha derecha - Subir\n\n" +
            "Escape o V - Volver";
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