using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth = 100;

    [Header("Muerte")]
    [SerializeField] private bool disablePlayerOnDeath = true;

    [Header("Pruebas")]
    [SerializeField] private bool enableDebugKeys = true;
    [SerializeField] private KeyCode damageTestKey = KeyCode.K;

    private bool isDead;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => isDead;

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

        if (amount <= 0)
            return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log("Vida actual: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
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

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        Debug.Log("El jugador ha muerto.");

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