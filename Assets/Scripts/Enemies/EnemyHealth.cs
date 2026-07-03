using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private int maxHealth = 60;
    [SerializeField] private int currentHealth = 60;

    [Header("Muerte")]
    [SerializeField] private bool destroyOnDeath = true;
    [SerializeField] private float destroyDelay = 0.6f;

    private bool isDead;
    private EnemyHitReaction hitReaction;

    public bool IsDead => isDead;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    private void Awake()
    {
        hitReaction = GetComponent<EnemyHitReaction>();
    }

    private void Start()
    {
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }

    public void TakeDamage(int damage)
    {
        TakeDamage(damage, transform.position - transform.forward);
    }

    public void TakeDamage(int damage, Vector3 damageSourcePosition)
    {
        if (isDead)
            return;

        if (damage <= 0)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log(gameObject.name + " recibió daño. Vida: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        if (hitReaction != null)
        {
            hitReaction.PlayHitReaction(damageSourcePosition);
        }
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        Debug.Log(gameObject.name + " murió.");

        if (hitReaction != null)
        {
            hitReaction.StopReaction();
        }

        BasicEnemy enemyMovement = GetComponent<BasicEnemy>();

        if (enemyMovement != null)
        {
            enemyMovement.enabled = false;
        }

        CharacterController characterController = GetComponent<CharacterController>();

        if (characterController != null)
        {
            characterController.enabled = false;
        }

        Collider enemyCollider = GetComponent<Collider>();

        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }

        transform.rotation = Quaternion.Euler(90f, transform.eulerAngles.y, 0f);
        transform.position += Vector3.down * 0.6f;

        if (destroyOnDeath)
        {
            Destroy(gameObject, destroyDelay);
        }
    }
}