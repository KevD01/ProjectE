using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToMainMenuManager : MonoBehaviour
{
    [Header("Escena del menú principal")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Input")]
    [SerializeField] private KeyCode returnToMenuKey = KeyCode.M;

    private void Update()
    {
        if (!Input.GetKeyDown(returnToMenuKey))
            return;

        if (!CanReturnToMainMenu())
            return;

        ReturnToMainMenu();
    }

    private bool CanReturnToMainMenu()
    {
        if (PauseMenuUI.Instance != null && PauseMenuUI.Instance.IsPaused)
            return true;

        if (GameOverUI.Instance != null && GameOverUI.Instance.IsGameOver)
            return true;

        if (EndingUI.Instance != null && EndingUI.Instance.EndingActive)
            return true;

        return false;
    }

    private void ReturnToMainMenu()
    {
        Time.timeScale = 1f;

        InteractionPromptUI.Instance?.Hide();

        SceneManager.LoadScene(mainMenuSceneName);
    }
}