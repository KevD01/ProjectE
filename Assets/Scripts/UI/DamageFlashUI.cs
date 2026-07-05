using System.Collections;
using UnityEngine;

public class DamageFlashUI : MonoBehaviour
{
    public static DamageFlashUI Instance;

    [Header("Referencias")]
    [SerializeField] private CanvasGroup canvasGroup;

    private Coroutine flashRoutine;

    private void Awake()
    {
        Instance = this;

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        HideInstant();
    }

    public void Flash(float maxAlpha, float duration)
    {
        if (canvasGroup == null)
            return;

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }

        flashRoutine = StartCoroutine(FlashRoutine(maxAlpha, duration));
    }

    private IEnumerator FlashRoutine(float maxAlpha, float duration)
    {
        canvasGroup.alpha = maxAlpha;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            canvasGroup.alpha = Mathf.Lerp(maxAlpha, 0f, t);

            yield return null;
        }

        canvasGroup.alpha = 0f;
        flashRoutine = null;
    }

    private void HideInstant()
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}