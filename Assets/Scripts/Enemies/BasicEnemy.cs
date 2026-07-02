using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class BasicEnemy : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 1.2f;
    [SerializeField] private float rotationSpeed = 6f;
    [SerializeField] private float gravity = -20f;

    [Header("Detección")]
    [SerializeField] private float detectionRange = 7f;
    [SerializeField] private float attackRange = 1.3f;

    [Header("Ataque")]
    [SerializeField] private int damage = 15;
    [SerializeField] private float attackCooldown = 1.5f;

    [Header("Debug")]
    [SerializeField] private bool showGizmos = true;

    private CharacterController characterController;
    private Transform playerTransform;
    private PlayerHealth playerHealth;

    private float verticalVelocity;
    private float lastAttackTime;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerTransform = player.transform;
            playerHealth = player.GetComponent<PlayerHealth>();
        }
    }

    private void Update()
    {
        if (IsGameplayPaused())
            return;

        if (playerTransform == null)
            return;

        HandleEnemyBehavior();
    }

    private void HandleEnemyBehavior()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= attackRange)
        {
            RotateTowardsPlayer();
            ApplyGravity();
            TryAttack();
            return;
        }

        if (distanceToPlayer <= detectionRange)
        {
            ChasePlayer();
            return;
        }

        ApplyGravity();
    }

    private void ChasePlayer()
    {
        Vector3 directionToPlayer = playerTransform.position - transform.position;
        directionToPlayer.y = 0f;

        if (directionToPlayer.sqrMagnitude <= 0.01f)
            return;

        Vector3 moveDirection = directionToPlayer.normalized;

        RotateTowardsDirection(moveDirection);

        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 movement = moveDirection * moveSpeed;
        movement.y = verticalVelocity;

        characterController.Move(movement * Time.deltaTime);
    }

    private void TryAttack()
    {
        if (Time.time < lastAttackTime + attackCooldown)
            return;

        lastAttackTime = Time.time;

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            Debug.Log("El enemigo atacó al jugador.");
        }
    }

    private void RotateTowardsPlayer()
    {
        Vector3 directionToPlayer = playerTransform.position - transform.position;
        directionToPlayer.y = 0f;

        if (directionToPlayer.sqrMagnitude <= 0.01f)
            return;

        RotateTowardsDirection(directionToPlayer.normalized);
    }

    private void RotateTowardsDirection(Vector3 direction)
    {
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private void ApplyGravity()
    {
        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 movement = Vector3.up * verticalVelocity;
        characterController.Move(movement * Time.deltaTime);
    }

    private bool IsGameplayPaused()
    {
        if (NoteUI.Instance != null && NoteUI.Instance.IsOpen)
            return true;

        if (NoteArchiveUI.Instance != null && NoteArchiveUI.Instance.IsOpen)
            return true;

        if (InventoryUI.Instance != null && InventoryUI.Instance.IsOpen)
            return true;

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos)
            return;

        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}