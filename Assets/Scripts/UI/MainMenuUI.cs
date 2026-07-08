using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("Escena del juego")]
    [SerializeField] private string gameSceneName = "Sanatorio_Entrada";

    [Header("Referencias")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private TMP_Text loadingText;

    [Header("Input")]
    [SerializeField] private KeyCode startKey = KeyCode.Return;
    [SerializeField] private KeyCode controlsKey = KeyCode.C;
    [SerializeField] private KeyCode volumeKey = KeyCode.V;
    [SerializeField] private KeyCode quitKey = KeyCode.Q;
    [SerializeField] private KeyCode backKey = KeyCode.Escape;

    [Header("Transición")]
    [SerializeField] private float waitBeforeLoad = 0.6f;
    [SerializeField] private float fadeOutTime = 0.8f;

    private bool showingControls;
    private bool showingVolume;
    private bool isLoading;

    private VolumeSettingsManager volumeSettings;

    private void Awake()
    {
        volumeSettings = VolumeSettingsManager.EnsureExists();

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        if (loadingText != null)
        {
            loadingText.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        Time.timeScale = 1f;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        ShowMainMenuText();
    }

    private void Update()
    {
        if (isLoading)
            return;

        if (showingVolume)
        {
            HandleVolumeInput();
            return;
        }

        if (showingControls)
        {
            if (Input.GetKeyDown(backKey) || Input.GetKeyDown(controlsKey))
            {
                ShowMainMenuText();
            }

            return;
        }

        if (Input.GetKeyDown(startKey))
        {
            StartCoroutine(StartGameRoutine());
            return;
        }

        if (Input.GetKeyDown(controlsKey))
        {
            ShowControlsText();
            return;
        }

        if (Input.GetKeyDown(volumeKey))
        {
            ShowVolumeText();
            return;
        }

        if (Input.GetKeyDown(quitKey))
        {
            QuitGame();
            return;
        }
    }

    private void HandleVolumeInput()
    {
        if (Input.GetKeyDown(backKey) || Input.GetKeyDown(volumeKey))
        {
            ShowMainMenuText();
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

    private void ShowMainMenuText()
    {
        showingControls = false;
        showingVolume = false;

        if (bodyText == null)
            return;

        bodyText.text =
            "Enter - Iniciar demo\n\n" +
            "C - Ver controles\n\n" +
            "V - Volumen\n\n" +
            "Q - Salir";
    }

    private void ShowControlsText()
    {
        showingControls = true;
        showingVolume = false;

        if (bodyText == null)
            return;

        bodyText.text =
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
            "Escape - Volver";
    }

    private void ShowVolumeText()
    {
        showingControls = false;
        showingVolume = true;

        if (volumeSettings == null)
        {
            volumeSettings = VolumeSettingsManager.EnsureExists();
        }

        if (bodyText == null)
            return;

        bodyText.text =
            "VOLUMEN\n\n" +
            "Volumen actual: " + volumeSettings.MasterVolumePercent + "%\n\n" +
            "A / Flecha izquierda - Bajar\n" +
            "D / Flecha derecha - Subir\n\n" +
            "Escape o V - Volver";
    }

    private IEnumerator StartGameRoutine()
    {
        isLoading = true;
        showingControls = false;
        showingVolume = false;

        if (canvasGroup != null)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = true;
        }

        if (bodyText != null)
        {
            bodyText.text = "";
        }

        if (loadingText != null)
        {
            loadingText.gameObject.SetActive(true);
            loadingText.text = "Cargando...";
        }

        yield return new WaitForSeconds(waitBeforeLoad);

        yield return FadeOutTextRoutine();

        SceneManager.LoadScene(gameSceneName);
    }

    private IEnumerator FadeOutTextRoutine()
    {
        if (canvasGroup == null)
            yield break;

        float startAlpha = canvasGroup.alpha;
        float timer = 0f;

        while (timer < fadeOutTime)
        {
            timer += Time.deltaTime;
            float t = timer / fadeOutTime;

            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);

            yield return null;
        }

        canvasGroup.alpha = 0f;
    }

    private void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}