using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Escena del juego")]
    [SerializeField] private string gameSceneName = "Sanatorio_Entrada";

    [Header("Referencias generales")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text loadingText;

    [Header("Paneles")]
    [SerializeField] private GameObject mainOptionsPanel;
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;

    [Header("Botones principales")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button controlsButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button quitButton;

    [Header("Botones para volver")]
    [SerializeField] private Button controlsBackButton;
    [SerializeField] private Button settingsBackButton;
    [SerializeField] private Button creditsBackButton;

    [Header("Transición")]
    [SerializeField] private float waitBeforeLoad = 0.6f;
    [SerializeField] private float fadeOutTime = 0.8f;

    private GameObject currentPanel;
    private bool isLoading;

    private void Awake()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        ConfigureButtons();

        if (loadingText != null)
        {
            loadingText.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        Time.timeScale = 1f;

        ShowCursor();
        RestoreCanvas();
        ShowMainOptions();
    }

    private void Update()
    {
        if (isLoading)
            return;

        if (!Input.GetKeyDown(KeyCode.Escape))
            return;

        if (currentPanel != mainOptionsPanel)
        {
            ShowMainOptions();
        }
    }

    private void ConfigureButtons()
    {
        if (startButton != null)
        {
            startButton.onClick.AddListener(StartGame);
        }

        if (controlsButton != null)
        {
            controlsButton.onClick.AddListener(ShowControls);
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(ShowSettings);
        }

        if (creditsButton != null)
        {
            creditsButton.onClick.AddListener(ShowCredits);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }

        if (controlsBackButton != null)
        {
            controlsBackButton.onClick.AddListener(ShowMainOptions);
        }

        if (settingsBackButton != null)
        {
            settingsBackButton.onClick.AddListener(ShowMainOptions);
        }

        if (creditsBackButton != null)
        {
            creditsBackButton.onClick.AddListener(ShowMainOptions);
        }
    }

    public void ShowMainOptions()
    {
        if (isLoading)
            return;

        SetActivePanel(mainOptionsPanel);
        SelectButton(startButton);
    }

    public void ShowControls()
    {
        if (isLoading)
            return;

        SetActivePanel(controlsPanel);
        SelectButton(controlsBackButton);
    }

    public void ShowSettings()
    {
        if (isLoading)
            return;

        SetActivePanel(settingsPanel);
        SelectButton(settingsBackButton);
    }

    public void ShowCredits()
    {
        if (isLoading)
            return;

        SetActivePanel(creditsPanel);
        SelectButton(creditsBackButton);
    }

    private void SetActivePanel(GameObject panelToShow)
    {
        if (mainOptionsPanel != null)
        {
            mainOptionsPanel.SetActive(panelToShow == mainOptionsPanel);
        }

        if (controlsPanel != null)
        {
            controlsPanel.SetActive(panelToShow == controlsPanel);
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(panelToShow == settingsPanel);
        }

        if (creditsPanel != null)
        {
            creditsPanel.SetActive(panelToShow == creditsPanel);
        }

        currentPanel = panelToShow;
    }

    private void SelectButton(Button buttonToSelect)
    {
        if (buttonToSelect == null)
            return;

        if (EventSystem.current == null)
        {
            Debug.LogWarning(
                "No existe un EventSystem en la escena MainMenu."
            );

            return;
        }

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(buttonToSelect.gameObject);
    }

    public void StartGame()
    {
        if (isLoading)
            return;

        StartCoroutine(StartGameRoutine());
    }

    private IEnumerator StartGameRoutine()
    {
        isLoading = true;

        DisableAllButtons();

        if (mainOptionsPanel != null)
        {
            mainOptionsPanel.SetActive(false);
        }

        if (controlsPanel != null)
        {
            controlsPanel.SetActive(false);
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        if (creditsPanel != null)
        {
            creditsPanel.SetActive(false);
        }

        if (loadingText != null)
        {
            loadingText.gameObject.SetActive(true);
            loadingText.text = "CARGANDO...";
        }

        yield return new WaitForSecondsRealtime(waitBeforeLoad);
        yield return FadeOutRoutine();

        SceneManager.LoadScene(gameSceneName);
    }

    private IEnumerator FadeOutRoutine()
    {
        if (canvasGroup == null)
            yield break;

        float startAlpha = canvasGroup.alpha;
        float timer = 0f;

        while (timer < fadeOutTime)
        {
            timer += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(timer / fadeOutTime);

            canvasGroup.alpha = Mathf.Lerp(
                startAlpha,
                0f,
                progress
            );

            yield return null;
        }

        canvasGroup.alpha = 0f;
    }

    private void DisableAllButtons()
    {
        SetButtonInteractable(startButton, false);
        SetButtonInteractable(controlsButton, false);
        SetButtonInteractable(settingsButton, false);
        SetButtonInteractable(creditsButton, false);
        SetButtonInteractable(quitButton, false);
        SetButtonInteractable(controlsBackButton, false);
        SetButtonInteractable(settingsBackButton, false);
        SetButtonInteractable(creditsBackButton, false);
    }

    private void SetButtonInteractable(Button button, bool interactable)
    {
        if (button != null)
        {
            button.interactable = interactable;
        }
    }

    private void RestoreCanvas()
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    private void ShowCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void QuitGame()
    {
        if (isLoading)
            return;

        Debug.Log("Saliendo del juego...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}