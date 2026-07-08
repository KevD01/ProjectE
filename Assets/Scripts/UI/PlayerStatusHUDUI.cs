using TMPro;
using UnityEngine;

public class PlayerStatusHUDUI : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text ammoText;

    [Header("Referencias jugador")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerWeaponController playerWeapon;

    [Header("Opciones")]
    [SerializeField] private bool hideWhenPaused = true;
    [SerializeField] private bool hideWhenInventoryOpen = true;
    [SerializeField] private bool hideWhenReadingNote = true;

    private GameObject playerObject;

    private void Awake()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
    }

    private void Start()
    {
        FindPlayerReferences();
        RefreshHUD();
    }

    private void Update()
    {
        if (playerHealth == null || playerWeapon == null)
        {
            FindPlayerReferences();
        }

        bool shouldHide = ShouldHideHUD();

        if (shouldHide)
        {
            HideHUD();
            return;
        }

        ShowHUD();
        RefreshHUD();
    }

    private void FindPlayerReferences()
    {
        if (playerObject == null)
        {
            playerObject = GameObject.FindGameObjectWithTag("Player");
        }

        if (playerObject == null)
            return;

        if (playerHealth == null)
        {
            playerHealth = playerObject.GetComponent<PlayerHealth>();
        }

        if (playerWeapon == null)
        {
            playerWeapon = playerObject.GetComponent<PlayerWeaponController>();
        }
    }

    private bool ShouldHideHUD()
    {
        if (GameOverUI.Instance != null && GameOverUI.Instance.IsGameOver)
            return true;

        if (EndingUI.Instance != null && EndingUI.Instance.EndingActive)
            return true;

        if (hideWhenPaused && PauseMenuUI.Instance != null && PauseMenuUI.Instance.IsPaused)
            return true;

        if (hideWhenInventoryOpen && InventoryUI.Instance != null && InventoryUI.Instance.IsOpen)
            return true;

        if (hideWhenReadingNote && NoteUI.Instance != null && NoteUI.Instance.IsOpen)
            return true;

        if (hideWhenReadingNote && NoteArchiveUI.Instance != null && NoteArchiveUI.Instance.IsOpen)
            return true;

        return false;
    }

    private void RefreshHUD()
    {
        RefreshHealthText();
        RefreshAmmoText();
    }

    private void RefreshHealthText()
    {
        if (healthText == null)
            return;

        if (playerHealth == null)
        {
            healthText.text = "SALUD: -- / --\nESTADO: --";
            return;
        }

        healthText.text =
            "SALUD: " + playerHealth.CurrentHealth + " / " + playerHealth.MaxHealth +
            "\nESTADO: " + playerHealth.GetHealthState();
    }

    private void RefreshAmmoText()
    {
        if (ammoText == null)
            return;

        if (playerWeapon == null)
        {
            ammoText.text = "BALAS: -- / --\nRESERVA: --";
            return;
        }

        if (!playerWeapon.HasRequiredWeaponEquipped())
        {
            ammoText.text =
                "PISTOLA: No equipada\n" +
                "RESERVA: " + playerWeapon.GetReserveAmmo();
            return;
        }

        string reloadText = "";

        if (playerWeapon.IsReloading)
        {
            reloadText = "\nRECARGANDO...";
        }

        ammoText.text =
            "BALAS: " + playerWeapon.CurrentAmmoInClip + " / " + playerWeapon.MaxAmmoInClip +
            "\nRESERVA: " + playerWeapon.GetReserveAmmo() +
            reloadText;
    }

    private void ShowHUD()
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void HideHUD()
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}