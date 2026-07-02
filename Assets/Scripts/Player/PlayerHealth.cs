using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth = 100;

    [Header("Pruebas")]
    [SerializeField] private bool enableDebugKeys = true;
    [SerializeField] private KeyCode damageTestKey = KeyCode.K;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    private void Start()
    {
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }

    private void Update()
    {
        if (!enableDebugKeys)
            return;

        if (Input.GetKeyDown(damageTestKey))
        {
            TakeDamage(25);
        }
    }

    public void TakeDamage(int amount)
    {
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
        Debug.Log("El jugador ha muerto.");
    }
}