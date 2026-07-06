using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingUI : MonoBehaviour
{
    public static EndingUI Instance;

    [Header("Referencias")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Fade")]
    [SerializeField] private float fadeTime = 1.2f;

    [Header("Input")]
    [SerializeField] private KeyCode restartKey = KeyCode.R;

    private bool endingActive;
    private Coroutine fadeRoutine;

    public bool EndingActive => endingActive;

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
        if (!endingActive)
            return;

        if (Input.GetKeyDown(restartKey))
        {
            RestartScene();
        }
    }

    public void ShowEnding()
    {
        if (endingActive)
            return;

        endingActive = true;

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