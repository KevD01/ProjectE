using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
    public static PauseMenuUI Instance;

    [Header("Referencias")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Input")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] private KeyCode restartKey = KeyCode.R;
    [SerializeField] private KeyCode quitKey = KeyCode.Q;

    private bool isPaused;

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
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                if (IsAnotherUIOpen())
                    return;

                PauseGame();
            }

            return;
        }

        if (!isPaused)
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

    public void PauseGame()
    {
        if (isPaused)
            return;

        isPaused = true;

        Time.timeScale = 0f;

        InteractionPromptUI.Instance?.Hide();

        ShowPauseMenu();
    }

    public void ResumeGame()
    {
        if (!isPaused)
            return;

        isPaused = false;

        Time.timeScale = 1f;

        HidePauseMenuInstant();
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