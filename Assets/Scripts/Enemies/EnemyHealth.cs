using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private int maxHealth = 60;
    [SerializeField] private int currentHealth = 60;

    [Header("Audio")]
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private float hitVolume = 0.8f;
    [SerializeField] private float deathVolume = 1f;

    [Header("Drop al morir")]
    [SerializeField] private GameObject dropPrefab;
    [SerializeField] private bool alwaysDrop = true;

    [Range(0f, 1f)]
    [SerializeField] private float dropChance = 0.5f;

    [SerializeField] private Vector3 dropOffset = new Vector3(0f, -0.75f, 0f);

    [Header("Muerte")]
    [SerializeField] private bool disableOnDeath = true;
    [SerializeField] private float disableDelay = 0.6f;

    private bool isDead;
    private EnemyHitReaction hitReaction;
    private Coroutine disableRoutine;

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

        GameAudioManager.Instance?.PlaySFX(hitSound, hitVolume);

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

        GameAudioManager.Instance?.PlaySFX(deathSound, deathVolume);

        Debug.Log(gameObject.name + " murió.");

        DropItem();

        if (hitReaction != null)
        {
            hitReaction.StopReaction();
        }

        SetEnemyComponentsEnabled(false);

        transform.rotation = Quaternion.Euler(90f, transform.eulerAngles.y, 0f);
        transform.position += Vector3.down * 0.6f;

        if (disableOnDeath)
        {
            disableRoutine = StartCoroutine(DisableAfterDeathRoutine());
        }
    }

    private IEnumerator DisableAfterDeathRoutine()
    {
        yield return new WaitForSeconds(disableDelay);

        gameObject.SetActive(false);

        disableRoutine = null;
    }

    private void DropItem()
    {
        if (dropPrefab == null)
            return;

        if (!alwaysDrop)
        {
            float randomValue = Random.Range(0f, 1f);

            if (randomValue > dropChance)
            {
                Debug.Log(gameObject.name + " no soltó objeto.");
                return;
            }
        }

        Vector3 dropPosition = transform.position + dropOffset;

        GameObject spawnedDrop = Instantiate(
            dropPrefab,
            dropPosition,
            Quaternion.identity
        );

        CheckpointSpawnedObject checkpointSpawnedObject =
            spawnedDrop.GetComponent<CheckpointSpawnedObject>();

        if (checkpointSpawnedObject == null)
        {
            checkpointSpawnedObject = spawnedDrop.AddComponent<CheckpointSpawnedObject>();
        }

        int version = 0;

        if (CheckpointManager.Instance != null)
        {
            version = CheckpointManager.Instance.CurrentCheckpointVersion;
        }

        checkpointSpawnedObject.MarkSpawned(version);

        Debug.Log(gameObject.name + " soltó: " + spawnedDrop.name);
    }

    public void RestoreFromCheckpoint(
        bool activeState,
        bool savedIsDead,
        int savedHealth,
        Vector3 savedPosition,
        Quaternion savedRotation
    )
    {
        if (disableRoutine != null)
        {
            StopCoroutine(disableRoutine);
            disableRoutine = null;
        }

        gameObject.SetActive(activeState);

        transform.SetPositionAndRotation(savedPosition, savedRotation);

        isDead = savedIsDead;
        currentHealth = Mathf.Clamp(savedHealth, 0, maxHealth);

        if (hitReaction != null)
        {
            hitReaction.StopReaction();
        }

        if (activeState && !isDead)
        {
            SetEnemyComponentsEnabled(true);
        }
        else
        {
            SetEnemyComponentsEnabled(false);
        }

        Debug.Log(gameObject.name + " restaurado desde checkpoint.");
    }

    private void SetEnemyComponentsEnabled(bool enabledState)
    {
        BasicEnemy enemyMovement = GetComponent<BasicEnemy>();

        if (enemyMovement != null)
        {
            enemyMovement.enabled = enabledState;
        }

        CharacterController characterController = GetComponent<CharacterController>();

        if (characterController != null)
        {
            characterController.enabled = enabledState;
        }

        Collider enemyCollider = GetComponent<Collider>();

        if (enemyCollider != null)
        {
            enemyCollider.enabled = enabledState;
        }
    }
}