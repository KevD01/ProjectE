using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    public static GameOverUI Instance;

    [Header("Referencias")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Fade")]
    [SerializeField] private float fadeTime = 1f;

    [Header("Input")]
    [SerializeField] private KeyCode restartKey = KeyCode.R;

    private bool isGameOver;
    private Coroutine fadeRoutine;

    public bool IsGameOver => isGameOver;

    private void Awake()
    {
        Instance = this;

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        HideInstant();
    }

    private void Update()
    {
        if (!isGameOver)
            return;

        if (Input.GetKeyDown(restartKey))
        {
            RestartScene();
        }
    }

    public void ShowGameOver()
    {
        if (isGameOver)
            return;

        isGameOver = true;

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        fadeRoutine = StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        if (canvasGroup == null)
            yield break;

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        float startAlpha = canvasGroup.alpha;
        float timer = 0f;

        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            float t = timer / fadeTime;

            canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, t);

            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    private void HideInstant()
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void RestartScene()
    {
        Time.timeScale = 1f;

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}