using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntroCinematicUI : MonoBehaviour
{
    public static IntroCinematicUI Instance;

    private static bool playedThisSession;

    [Header("Referencias UI")]
    [SerializeField] private CanvasGroup introCanvasGroup;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private TMP_Text locationText;
    [SerializeField] private TMP_Text skipText;

    [Header("Progreso para omitir")]
    [SerializeField] private Image skipProgressFillImage;

    [Header("Objetos ocultos durante la introducción")]
    [SerializeField]
    private GameObject[] objectsToHideDuringIntro;

    [Header("Texto")]
    [SerializeField]
    private string gameTitle = "ECOS DEL SILENCIO";

    [TextArea(2, 5)]
    [SerializeField]
    private string[] introMessages =
    {
        "Hace tres días, una señal de auxilio\n" +
        "comenzó a transmitirse desde el sanatorio.",

        "Nadie de los equipos enviados\n" +
        "ha regresado.",

        "Tu hermano fue uno de ellos."
    };

    [SerializeField]
    private string locationMessage =
        "SANATORIO — 02:17 A. M.";

    [Header("Tiempos de textos")]
    [Min(0f)]
    [SerializeField]
    private float initialBlackTime = 0.8f;

    [Min(0f)]
    [SerializeField]
    private float titleFadeTime = 0.8f;

    [Min(0f)]
    [SerializeField]
    private float titleHoldTime = 1.6f;

    [Min(0f)]
    [SerializeField]
    private float messageFadeTime = 0.65f;

    [Min(0f)]
    [SerializeField]
    private float messageHoldTime = 2.5f;

    [Min(0f)]
    [SerializeField]
    private float locationFadeTime = 0.7f;

    [Min(0f)]
    [SerializeField]
    private float locationHoldTime = 1.7f;

    [Header("Transición final")]
    [Min(0f)]
    [SerializeField]
    private float finalBlackHoldTime = 0.35f;

    [Min(0f)]
    [SerializeField]
    private float finalStingLeadTime = 0.15f;

    [Min(0f)]
    [SerializeField]
    private float finalFadeTime = 1.6f;

    [Min(0f)]
    [SerializeField]
    private float skipFadeTime = 0.35f;

    [Header("Mantener para omitir")]
    [SerializeField]
    private KeyCode skipKey = KeyCode.Space;

    [Min(0.1f)]
    [SerializeField]
    private float skipHoldDuration = 1.5f;

    [Min(0f)]
    [SerializeField]
    private float skipResetSpeed = 2.5f;

    [SerializeField]
    private string skipIdleMessage =
        "MANTÉN ESPACIO PARA OMITIR";

    [SerializeField]
    private string skipHoldingMessage =
        "SIGUE MANTENIENDO";

    [Header("Ambiente de introducción")]
    [SerializeField]
    private AudioSource introAmbienceSource;

    [SerializeField]
    private AudioClip introAmbienceClip;

    [Range(0f, 1f)]
    [SerializeField]
    private float introAmbienceVolume = 0.55f;

    [Header("Golpe sonoro final")]
    [SerializeField]
    private AudioSource finalStingSource;

    [SerializeField]
    private AudioClip finalStingClip;

    [Range(0f, 1f)]
    [SerializeField]
    private float finalStingVolume = 0.8f;

    private PlayerTankController playerMovement;
    private PlayerWeaponController playerWeapon;

    private bool previousMovementEnabled;
    private bool previousWeaponEnabled;

    private bool[] previousObjectStates;

    private CursorLockMode previousCursorLockMode;
    private bool previousCursorVisible;

    private float previousTimeScale = 1f;
    private float skipHoldTimer;

    private bool isPlaying;
    private bool isCompleting;

    private Coroutine introRoutine;
    private Coroutine skipRoutine;

    public bool IsPlaying => isPlaying;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration
    )]
    private static void ResetStaticState()
    {
        Instance = null;
        playedThisSession = false;
    }

    private void Awake()
    {
        Instance = this;

        if (introCanvasGroup == null)
        {
            introCanvasGroup =
                GetComponent<CanvasGroup>();
        }

        ResetSkipProgress();
    }

    private void Start()
    {
        if (playedThisSession)
        {
            HideInstant();
            return;
        }

        introRoutine =
            StartCoroutine(PlayIntroRoutine());
    }

    private void Update()
    {
        if (!isPlaying || isCompleting)
            return;

        UpdateHoldToSkip();
    }

    public static void ResetForNewGame()
    {
        playedThisSession = false;
    }

    private void UpdateHoldToSkip()
    {
        bool isHolding =
            Input.GetKey(skipKey);

        if (isHolding)
        {
            skipHoldTimer +=
                Time.unscaledDeltaTime;
        }
        else
        {
            skipHoldTimer = Mathf.MoveTowards(
                skipHoldTimer,
                0f,
                skipResetSpeed *
                Time.unscaledDeltaTime
            );
        }

        float progress = Mathf.Clamp01(
            skipHoldTimer / skipHoldDuration
        );

        UpdateSkipVisuals(
            progress,
            isHolding
        );

        if (skipHoldTimer >= skipHoldDuration)
        {
            BeginSkipTransition();
        }
    }

    private void UpdateSkipVisuals(
        float progress,
        bool isHolding
    )
    {
        if (skipProgressFillImage != null)
        {
            skipProgressFillImage.fillAmount =
                progress;
        }

        if (skipText == null)
            return;

        skipText.alpha =
            isHolding || progress > 0f
                ? 1f
                : 0.65f;

        if (!isHolding && progress <= 0f)
        {
            skipText.text = skipIdleMessage;
            return;
        }

        int percentage =
            Mathf.RoundToInt(progress * 100f);

        skipText.text =
            skipHoldingMessage +
            " — " +
            percentage +
            "%";
    }

    private void ResetSkipProgress()
    {
        skipHoldTimer = 0f;

        if (skipProgressFillImage != null)
        {
            skipProgressFillImage.fillAmount = 0f;
        }

        if (skipText != null)
        {
            skipText.text = skipIdleMessage;
            skipText.alpha = 0.65f;
        }
    }

    private IEnumerator PlayIntroRoutine()
    {
        playedThisSession = true;
        isPlaying = true;
        isCompleting = false;

        PrepareGameplayForIntro();
        PrepareUI();
        StartAmbienceAudio();

        yield return new WaitForSecondsRealtime(
            initialBlackTime
        );

        yield return PlayTitleSequence();
        yield return PlayMessageSequence();
        yield return PlayLocationSequence();

        HideSkipInterface();

        if (finalBlackHoldTime > 0f)
        {
            yield return new WaitForSecondsRealtime(
                finalBlackHoldTime
            );
        }

        PlayFinalSting();

        if (finalStingLeadTime > 0f)
        {
            yield return new WaitForSecondsRealtime(
                finalStingLeadTime
            );
        }

        yield return FadeCanvasAndAmbience(
            0f,
            finalFadeTime
        );

        CompleteIntro();
    }

    private IEnumerator PlayTitleSequence()
    {
        if (titleText == null)
            yield break;

        titleText.gameObject.SetActive(true);
        titleText.text = gameTitle;

        yield return FadeText(
            titleText,
            0f,
            1f,
            titleFadeTime
        );

        yield return new WaitForSecondsRealtime(
            titleHoldTime
        );

        yield return FadeText(
            titleText,
            1f,
            0f,
            titleFadeTime
        );

        titleText.gameObject.SetActive(false);
    }

    private IEnumerator PlayMessageSequence()
    {
        if (bodyText == null)
            yield break;

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

        bodyText.gameObject.SetActive(false);
    }

    private IEnumerator PlayLocationSequence()
    {
        if (locationText == null ||
            string.IsNullOrWhiteSpace(
                locationMessage
            ))
        {
            yield break;
        }

        locationText.gameObject.SetActive(true);
        locationText.text = locationMessage;

        yield return FadeText(
            locationText,
            0f,
            1f,
            locationFadeTime
        );

        yield return new WaitForSecondsRealtime(
            locationHoldTime
        );

        yield return FadeText(
            locationText,
            1f,
            0f,
            locationFadeTime
        );

        locationText.gameObject.SetActive(false);
    }

    private void PrepareGameplayForIntro()
    {
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        previousCursorLockMode =
            Cursor.lockState;

        previousCursorVisible =
            Cursor.visible;

        Cursor.lockState =
            CursorLockMode.None;

        Cursor.visible = false;

        GameObject player =
            GameObject.FindGameObjectWithTag(
                "Player"
            );

        if (player != null)
        {
            playerMovement =
                player.GetComponent<
                    PlayerTankController
                >();

            playerWeapon =
                player.GetComponent<
                    PlayerWeaponController
                >();
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

        if (locationText != null)
        {
            locationText.gameObject.SetActive(false);
            locationText.alpha = 0f;
            locationText.text = locationMessage;
        }

        if (skipText != null)
        {
            skipText.gameObject.SetActive(true);
        }

        SetSkipBarActive(true);
        ResetSkipProgress();
    }

    private void StartAmbienceAudio()
    {
        if (introAmbienceSource == null ||
            introAmbienceClip == null)
        {
            return;
        }

        introAmbienceSource.Stop();

        introAmbienceSource.clip =
            introAmbienceClip;

        introAmbienceSource.volume =
            introAmbienceVolume;

        introAmbienceSource.loop = true;
        introAmbienceSource.Play();
    }

    private void PlayFinalSting()
    {
        if (finalStingSource == null ||
            finalStingClip == null)
        {
            return;
        }

        finalStingSource.PlayOneShot(
            finalStingClip,
            finalStingVolume
        );
    }

    private void BeginSkipTransition()
    {
        if (isCompleting)
            return;

        isCompleting = true;

        if (introRoutine != null)
        {
            StopCoroutine(introRoutine);
            introRoutine = null;
        }

        HideSkipInterface();
        PlayFinalSting();

        skipRoutine =
            StartCoroutine(
                SkipTransitionRoutine()
            );
    }

    private IEnumerator SkipTransitionRoutine()
    {
        yield return FadeCanvasAndAmbience(
            0f,
            skipFadeTime
        );

        CompleteIntro();
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
                Mathf.Clamp01(
                    timer / duration
                );

            targetText.alpha = Mathf.Lerp(
                startAlpha,
                endAlpha,
                progress
            );

            yield return null;
        }

        targetText.alpha = endAlpha;
    }

    private IEnumerator FadeCanvasAndAmbience(
        float targetCanvasAlpha,
        float duration
    )
    {
        float startCanvasAlpha =
            introCanvasGroup != null
                ? introCanvasGroup.alpha
                : 1f;

        float startAmbienceVolume =
            introAmbienceSource != null
                ? introAmbienceSource.volume
                : 0f;

        if (duration <= 0f)
        {
            if (introCanvasGroup != null)
            {
                introCanvasGroup.alpha =
                    targetCanvasAlpha;
            }

            StopAmbienceAudio();
            yield break;
        }

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;

            float progress =
                Mathf.Clamp01(
                    timer / duration
                );

            if (introCanvasGroup != null)
            {
                introCanvasGroup.alpha =
                    Mathf.Lerp(
                        startCanvasAlpha,
                        targetCanvasAlpha,
                        progress
                    );
            }

            if (introAmbienceSource != null &&
                introAmbienceSource.isPlaying)
            {
                introAmbienceSource.volume =
                    Mathf.Lerp(
                        startAmbienceVolume,
                        0f,
                        progress
                    );
            }

            yield return null;
        }

        if (introCanvasGroup != null)
        {
            introCanvasGroup.alpha =
                targetCanvasAlpha;
        }

        StopAmbienceAudio();
    }

    private void StopAmbienceAudio()
    {
        if (introAmbienceSource == null)
            return;

        introAmbienceSource.Stop();

        introAmbienceSource.volume =
            introAmbienceVolume;
    }

    private void HideSkipInterface()
    {
        if (skipText != null)
        {
            skipText.gameObject.SetActive(false);
        }

        SetSkipBarActive(false);
    }

    private void SetSkipBarActive(bool active)
    {
        if (skipProgressFillImage == null)
            return;

        Transform parent =
            skipProgressFillImage.transform.parent;

        if (parent != null)
        {
            parent.gameObject.SetActive(active);
        }
    }

    private void HideGameplayObjects()
    {
        if (objectsToHideDuringIntro == null)
            return;

        previousObjectStates =
            new bool[
                objectsToHideDuringIntro.Length
            ];

        for (int i = 0;
             i < objectsToHideDuringIntro.Length;
             i++)
        {
            GameObject target =
                objectsToHideDuringIntro[i];

            if (target == null)
                continue;

            previousObjectStates[i] =
                target.activeSelf;

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

    private void CompleteIntro()
    {
        if (!isPlaying && !isCompleting)
            return;

        isPlaying = false;
        isCompleting = false;

        introRoutine = null;
        skipRoutine = null;

        ResetSkipProgress();
        StopAmbienceAudio();

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

        Cursor.lockState =
            previousCursorLockMode;

        Cursor.visible =
            previousCursorVisible;

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

    private void RestoreGameplayImmediately()
    {
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

        Cursor.lockState =
            previousCursorLockMode;

        Cursor.visible =
            previousCursorVisible;

        StopAmbienceAudio();

        if (finalStingSource != null)
        {
            finalStingSource.Stop();
        }
    }

    private void OnDisable()
    {
        if (!isPlaying)
            return;

        RestoreGameplayImmediately();

        isPlaying = false;
        isCompleting = false;

        ResetSkipProgress();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}