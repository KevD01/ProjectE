using System.Collections;
using TMPro;
using UnityEngine;

public class IntroCinematicUI : MonoBehaviour
{
    public static IntroCinematicUI Instance;

    private static bool playedThisSession;

    [Header("Referencias UI")]
    [SerializeField] private CanvasGroup introCanvasGroup;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private TMP_Text skipText;

    [Header("Objetos ocultos durante la introducción")]
    [SerializeField] private GameObject[] objectsToHideDuringIntro;

    [Header("Texto")]
    [SerializeField] private string gameTitle = "ECOS DEL SILENCIO";

    [TextArea(2, 5)]
    [SerializeField] private string[] introMessages =
    {
        "Hace tres días, una señal de auxilio\ncomenzó a transmitirse desde el sanatorio.",
        "Nadie de los equipos enviados\nha regresado.",
        "Tu hermano fue uno de ellos."
    };

    [Header("Tiempos")]
    [SerializeField] private float initialBlackTime = 0.7f;
    [SerializeField] private float titleFadeTime = 0.8f;
    [SerializeField] private float titleHoldTime = 1.5f;
    [SerializeField] private float messageFadeTime = 0.6f;
    [SerializeField] private float messageHoldTime = 2.5f;
    [SerializeField] private float finalFadeTime = 1.2f;

    [Header("Omitir")]
    [SerializeField] private KeyCode skipKey = KeyCode.Space;

    [Header("Audio opcional")]
    [SerializeField] private AudioSource introAudioSource;
    [SerializeField] private AudioClip introAudioClip;
    [SerializeField] private float introAudioVolume = 0.7f;

    private PlayerTankController playerMovement;
    private PlayerWeaponController playerWeapon;

    private bool previousMovementEnabled;
    private bool previousWeaponEnabled;

    private bool[] previousObjectStates;

    private bool isPlaying;
    private bool isCompleting;

    private float previousTimeScale = 1f;
    private Coroutine introRoutine;

    public bool IsPlaying => isPlaying;

    private void Awake()
    {
        Instance = this;

        if (introCanvasGroup == null)
        {
            introCanvasGroup = GetComponent<CanvasGroup>();
        }
    }

    private void Start()
    {
        if (playedThisSession)
        {
            HideInstant();
            return;
        }

        introRoutine = StartCoroutine(PlayIntroRoutine());
    }

    private void Update()
    {
        if (!isPlaying || isCompleting)
            return;

        if (Input.GetKeyDown(skipKey))
        {
            SkipIntro();
        }
    }

    public static void ResetForNewGame()
    {
        playedThisSession = false;
    }

    private IEnumerator PlayIntroRoutine()
    {
        playedThisSession = true;
        isPlaying = true;
        isCompleting = false;

        PrepareGameplayForIntro();
        PrepareUI();

        yield return new WaitForSecondsRealtime(initialBlackTime);

        titleText.text = gameTitle;

        yield return FadeText(
            titleText,
            0f,
            1f,
            titleFadeTime
        );

        yield return new WaitForSecondsRealtime(titleHoldTime);

        yield return FadeText(
            titleText,
            1f,
            0f,
            titleFadeTime
        );

        titleText.gameObject.SetActive(false);
        bodyText.gameObject.SetActive(true);

        foreach (string message in introMessages)
        {
            if (string.IsNullOrWhiteSpace(message))
                continue;

            bodyText.text = message;

            yield return FadeText(
                bodyText,
                0f,
                1f,
                messageFadeTime
            );

            yield return new WaitForSecondsRealtime(
                messageHoldTime
            );

            yield return FadeText(
                bodyText,
                1f,
                0f,
                messageFadeTime
            );
        }

        yield return FadeCanvas(
            1f,
            0f,
            finalFadeTime
        );

        CompleteIntro();
    }

    private void PrepareGameplayForIntro()
    {
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerMovement =
                player.GetComponent<PlayerTankController>();

            playerWeapon =
                player.GetComponent<PlayerWeaponController>();
        }

        if (playerMovement != null)
        {
            previousMovementEnabled =
                playerMovement.enabled;

            playerMovement.enabled = false;
        }

        if (playerWeapon != null)
        {
            previousWeaponEnabled =
                playerWeapon.enabled;

            playerWeapon.enabled = false;
        }

        HideGameplayObjects();

        InteractionPromptUI.Instance?.Hide();
    }

    private void PrepareUI()
    {
        if (introCanvasGroup != null)
        {
            introCanvasGroup.alpha = 1f;
            introCanvasGroup.interactable = true;
            introCanvasGroup.blocksRaycasts = true;
        }

        if (titleText != null)
        {
            titleText.gameObject.SetActive(true);
            titleText.alpha = 0f;
            titleText.text = gameTitle;
        }

        if (bodyText != null)
        {
            bodyText.gameObject.SetActive(false);
            bodyText.alpha = 0f;
            bodyText.text = "";
        }

        if (skipText != null)
        {
            skipText.gameObject.SetActive(true);
            skipText.alpha = 0.65f;
            skipText.text = "ESPACIO — OMITIR";
        }

        if (introAudioSource != null &&
            introAudioClip != null)
        {
            introAudioSource.clip = introAudioClip;
            introAudioSource.volume = introAudioVolume;
            introAudioSource.loop = false;
            introAudioSource.Play();
        }
    }

    private void HideGameplayObjects()
    {
        if (objectsToHideDuringIntro == null)
            return;

        previousObjectStates =
            new bool[objectsToHideDuringIntro.Length];

        for (int i = 0;
             i < objectsToHideDuringIntro.Length;
             i++)
        {
            GameObject target =
                objectsToHideDuringIntro[i];

            if (target == null)
                continue;

            previousObjectStates[i] = target.activeSelf;
            target.SetActive(false);
        }
    }

    private void RestoreGameplayObjects()
    {
        if (objectsToHideDuringIntro == null ||
            previousObjectStates == null)
        {
            return;
        }

        int count = Mathf.Min(
            objectsToHideDuringIntro.Length,
            previousObjectStates.Length
        );

        for (int i = 0; i < count; i++)
        {
            GameObject target =
                objectsToHideDuringIntro[i];

            if (target != null)
            {
                target.SetActive(
                    previousObjectStates[i]
                );
            }
        }
    }

    private IEnumerator FadeText(
        TMP_Text targetText,
        float startAlpha,
        float endAlpha,
        float duration
    )
    {
        if (targetText == null)
            yield break;

        if (duration <= 0f)
        {
            targetText.alpha = endAlpha;
            yield break;
        }

        float timer = 0f;
        targetText.alpha = startAlpha;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;

            float progress =
                Mathf.Clamp01(timer / duration);

            targetText.alpha = Mathf.Lerp(
                startAlpha,
                endAlpha,
                progress
            );

            yield return null;
        }

        targetText.alpha = endAlpha;
    }

    private IEnumerator FadeCanvas(
        float startAlpha,
        float endAlpha,
        float duration
    )
    {
        if (introCanvasGroup == null)
            yield break;

        if (duration <= 0f)
        {
            introCanvasGroup.alpha = endAlpha;
            yield break;
        }

        float timer = 0f;
        introCanvasGroup.alpha = startAlpha;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;

            float progress =
                Mathf.Clamp01(timer / duration);

            introCanvasGroup.alpha = Mathf.Lerp(
                startAlpha,
                endAlpha,
                progress
            );

            yield return null;
        }

        introCanvasGroup.alpha = endAlpha;
    }

    private void SkipIntro()
    {
        if (isCompleting)
            return;

        isCompleting = true;

        if (introRoutine != null)
        {
            StopCoroutine(introRoutine);
            introRoutine = null;
        }

        CompleteIntro();
    }

    private void CompleteIntro()
    {
        if (!isPlaying && !isCompleting)
            return;

        isPlaying = false;
        isCompleting = false;

        if (introAudioSource != null &&
            introAudioSource.isPlaying)
        {
            introAudioSource.Stop();
        }

        Time.timeScale =
            previousTimeScale <= 0f
                ? 1f
                : previousTimeScale;

        if (playerMovement != null)
        {
            playerMovement.enabled =
                previousMovementEnabled;
        }

        if (playerWeapon != null)
        {
            playerWeapon.enabled =
                previousWeaponEnabled;
        }

        RestoreGameplayObjects();
        HideInstant();
    }

    private void HideInstant()
    {
        if (introCanvasGroup != null)
        {
            introCanvasGroup.alpha = 0f;
            introCanvasGroup.interactable = false;
            introCanvasGroup.blocksRaycasts = false;
        }

        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        if (!isPlaying)
            return;

        Time.timeScale = 1f;

        if (playerMovement != null)
        {
            playerMovement.enabled =
                previousMovementEnabled;
        }

        if (playerWeapon != null)
        {
            playerWeapon.enabled =
                previousWeaponEnabled;
        }

        RestoreGameplayObjects();

        isPlaying = false;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}