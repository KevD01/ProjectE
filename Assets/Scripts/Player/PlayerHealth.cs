using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth = 100;

    [Header("Daño")]
    [SerializeField] private bool useInvulnerabilityAfterHit = true;
    [SerializeField] private float invulnerabilityTime = 0.6f;

    [Header("Feedback de daño")]
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private float hurtVolume = 0.8f;
    [SerializeField] private float deathVolume = 1f;
    [SerializeField] private float damageFlashAlpha = 0.65f;
    [SerializeField] private float damageFlashTime = 0.35f;

    [Header("Muerte")]
    [SerializeField] private bool disablePlayerOnDeath = true;

    [Header("Pruebas")]
    [SerializeField] private bool enableDebugKeys = true;
    [SerializeField] private KeyCode damageTestKey = KeyCode.K;

    private bool isDead;
    private bool isInvulnerable;
    private Coroutine invulnerabilityRoutine;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => isDead;
    public bool IsInvulnerable => isInvulnerable;

    private void Start()
    {
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }

    private void Update()
    {
        if (isDead)
            return;

        if (!enableDebugKeys)
            return;

        if (Input.GetKeyDown(damageTestKey))
        {
            TakeDamage(25);
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead)
            return;

        if (useInvulnerabilityAfterHit && isInvulnerable)
            return;

        if (amount <= 0)
            return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log("Vida actual: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        PlayDamageFeedback();

        if (useInvulnerabilityAfterHit)
        {
            StartInvulnerability();
        }
    }

    public void Heal(int amount)
    {
        if (isDead)
            return;

        if (amount <= 0)
            return;

        if (currentHealth >= maxHealth)
        {
            Debug.Log("La vida ya está al máximo.");
            return;
        }

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log("Jugador curado. Vida actual: " + currentHealth);
    }

    public bool IsFullHealth()
    {
        return currentHealth >= maxHealth;
    }

    public string GetHealthState()
    {
        float healthPercent = (float)currentHealth / maxHealth;

        if (healthPercent > 0.6f)
            return "Bien";

        if (healthPercent > 0.3f)
            return "Cuidado";

        if (currentHealth > 0)
            return "Peligro";

        return "Muerto";
    }

    private void PlayDamageFeedback()
    {
        GameAudioManager.Instance?.PlaySFXNoPitch(hurtSound, hurtVolume);

        if (DamageFlashUI.Instance != null)
        {
            DamageFlashUI.Instance.Flash(damageFlashAlpha, damageFlashTime);
        }
    }

    private void StartInvulnerability()
    {
        if (invulnerabilityRoutine != null)
        {
            StopCoroutine(invulnerabilityRoutine);
        }

        invulnerabilityRoutine = StartCoroutine(InvulnerabilityRoutine());
    }

    private IEnumerator InvulnerabilityRoutine()
    {
        isInvulnerable = true;

        yield return new WaitForSeconds(invulnerabilityTime);

        isInvulnerable = false;
        invulnerabilityRoutine = null;
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        Debug.Log("El jugador ha muerto.");

        GameAudioManager.Instance?.PlaySFXNoPitch(deathSound, deathVolume);

        if (DamageFlashUI.Instance != null)
        {
            DamageFlashUI.Instance.Flash(0.85f, 0.6f);
        }

        InteractionPromptUI.Instance?.Hide();

        CloseOpenUIs();

        if (disablePlayerOnDeath)
        {
            DisablePlayerControls();
        }

        if (GameOverUI.Instance != null)
        {
            GameOverUI.Instance.ShowGameOver();
        }
    }

    private void CloseOpenUIs()
    {
        if (InventoryUI.Instance != null && InventoryUI.Instance.IsOpen)
        {
            InventoryUI.Instance.ForceClose();
        }

        if (NoteArchiveUI.Instance != null && NoteArchiveUI.Instance.IsOpen)
        {
            NoteArchiveUI.Instance.ForceClose();
        }

        if (NoteUI.Instance != null && NoteUI.Instance.IsOpen)
        {
            NoteUI.Instance.ForceClose();
        }
    }

    private void DisablePlayerControls()
    {
        PlayerTankController movement = GetComponent<PlayerTankController>();

        if (movement != null)
        {
            movement.enabled = false;
        }

        PlayerWeaponController weapon = GetComponent<PlayerWeaponController>();

        if (weapon != null)
        {
            weapon.enabled = false;
        }

        CharacterController characterController = GetComponent<CharacterController>();

        if (characterController != null)
        {
            characterController.enabled = false;
        }
    }
}