using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
    public static PauseMenuUI Instance;

    [Header("Escenas")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Referencias generales")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text pauseTitleText;

    [Header("Paneles")]
    [SerializeField] private GameObject mainPausePanel;
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Botones principales")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button controlsButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;

    [Header("Botones para volver")]
    [SerializeField] private Button controlsBackButton;
    [SerializeField] private Button settingsBackButton;

    [Header("Selección del panel de ajustes")]
    [SerializeField] private Selectable settingsFirstSelectable;

    [Header("Input")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

    private bool isPaused;
    private bool isChangingScene;

    private GameObject currentPanel;

    private CursorLockMode previousCursorLockMode;
    private bool previousCursorVisible;

    public bool IsPaused => isPaused;

    private void Awake()
    {
        Instance = this;

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        ConfigureButtons();
        HidePauseMenuInstant();
    }

    private void Update()
    {
        if (IntroCinematicUI.Instance != null &&
            IntroCinematicUI.Instance.IsPlaying)
        {
            return;
        }

        if (isChangingScene)
            return;

        if (GameOverUI.Instance != null &&
            GameOverUI.Instance.IsGameOver)
        {
            return;
        }

        if (EndingUI.Instance != null &&
            EndingUI.Instance.EndingActive)
        {
            return;
        }

        if (!Input.GetKeyDown(pauseKey))
            return;

        if (!isPaused)
        {
            if (IsAnotherUIOpen())
                return;

            PauseGame();
            return;
        }

        if (currentPanel != mainPausePanel)
        {
            ShowMainPausePanel();
            return;
        }

        ResumeGame();
    }

    private void ConfigureButtons()
    {
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(ResumeGame);
        }

        if (controlsButton != null)
        {
            controlsButton.onClick.AddListener(
                ShowControlsPanel
            );
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(
                ShowSettingsPanel
            );
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(
                RestartScene
            );
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(
                ReturnToMainMenu
            );
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }

        if (controlsBackButton != null)
        {
            controlsBackButton.onClick.AddListener(
                ShowMainPausePanel
            );
        }

        if (settingsBackButton != null)
        {
            settingsBackButton.onClick.AddListener(
                ShowMainPausePanel
            );
        }
    }

    public void PauseGame()
    {
        if (isPaused || isChangingScene)
            return;

        if (IntroCinematicUI.Instance != null &&
            IntroCinematicUI.Instance.IsPlaying)
        {
            return;
        }

        isPaused = true;

        SaveCurrentCursorState();

        Time.timeScale = 0f;

        InteractionPromptUI.Instance?.Hide();

        ShowPauseCanvas();
        ShowCursorForMenu();
        ShowMainPausePanel();
    }

    public void ResumeGame()
    {
        if (!isPaused || isChangingScene)
            return;

        isPaused = false;

        Time.timeScale = 1f;

        HidePauseMenuInstant();
        RestorePreviousCursorState();
    }

    public void ShowMainPausePanel()
    {
        if (!isPaused)
            return;

        SetActivePanel(mainPausePanel);

        if (pauseTitleText != null)
        {
            pauseTitleText.gameObject.SetActive(true);
            pauseTitleText.text = "PAUSA";
        }

        SelectUIElement(resumeButton);
    }

    public void ShowControlsPanel()
    {
        if (!isPaused)
            return;

        SetActivePanel(controlsPanel);

        if (pauseTitleText != null)
        {
            pauseTitleText.gameObject.SetActive(false);
        }

        SelectUIElement(controlsBackButton);
    }

    public void ShowSettingsPanel()
    {
        if (!isPaused)
            return;

        SetActivePanel(settingsPanel);

        if (pauseTitleText != null)
        {
            pauseTitleText.gameObject.SetActive(false);
        }

        if (settingsFirstSelectable != null)
        {
            SelectUIElement(settingsFirstSelectable);
        }
        else
        {
            SelectUIElement(settingsBackButton);
        }
    }

    private void SetActivePanel(GameObject panelToShow)
    {
        if (mainPausePanel != null)
        {
            mainPausePanel.SetActive(
                panelToShow == mainPausePanel
            );
        }

        if (controlsPanel != null)
        {
            controlsPanel.SetActive(
                panelToShow == controlsPanel
            );
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(
                panelToShow == settingsPanel
            );
        }

        currentPanel = panelToShow;
    }

    private void ShowPauseCanvas()
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    private void HidePauseMenuInstant()
    {
        if (mainPausePanel != null)
        {
            mainPausePanel.SetActive(false);
        }

        if (controlsPanel != null)
        {
            controlsPanel.SetActive(false);
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        currentPanel = null;

        if (pauseTitleText != null)
        {
            pauseTitleText.gameObject.SetActive(false);
        }

        if (canvasGroup == null)
            return;

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void SelectUIElement(Selectable selectable)
    {
        if (selectable == null)
            return;

        if (EventSystem.current == null)
        {
            Debug.LogWarning(
                "No existe EventSystem en Sanatorio_Entrada."
            );

            return;
        }

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(
            selectable.gameObject
        );
    }

    private bool IsAnotherUIOpen()
    {
        if (NoteUI.Instance != null &&
            NoteUI.Instance.IsOpen)
        {
            return true;
        }

        if (NoteArchiveUI.Instance != null &&
            NoteArchiveUI.Instance.IsOpen)
        {
            return true;
        }

        if (InventoryUI.Instance != null &&
            InventoryUI.Instance.IsOpen)
        {
            return true;
        }

        return false;
    }

    private void RestartScene()
    {
        if (isChangingScene)
            return;

        isChangingScene = true;
        isPaused = false;

        Time.timeScale = 1f;

        Scene currentScene =
            SceneManager.GetActiveScene();

        SceneManager.LoadScene(currentScene.name);
    }

    private void ReturnToMainMenu()
    {
        if (isChangingScene)
            return;

        isChangingScene = true;
        isPaused = false;

        Time.timeScale = 1f;

        InteractionPromptUI.Instance?.Hide();

        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void QuitGame()
    {
        if (isChangingScene)
            return;

        isChangingScene = true;
        isPaused = false;

        Time.timeScale = 1f;

        Debug.Log("Saliendo del juego...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void SaveCurrentCursorState()
    {
        previousCursorLockMode = Cursor.lockState;
        previousCursorVisible = Cursor.visible;
    }

    private void ShowCursorForMenu()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void RestorePreviousCursorState()
    {
        Cursor.lockState = previousCursorLockMode;
        Cursor.visible = previousCursorVisible;
    }

    private void OnDisable()
    {
        if (!isPaused || isChangingScene)
            return;

        isPaused = false;
        Time.timeScale = 1f;

        RestorePreviousCursorState();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}