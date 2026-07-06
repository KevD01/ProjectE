using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class BasicEnemy : MonoBehaviour
{
    private enum EnemyState
    {
        Patrol,
        Chase,
        Attack,
        ReturnToPatrol
    }

    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 1.2f;
    [SerializeField] private float patrolSpeed = 0.8f;
    [SerializeField] private float rotationSpeed = 6f;
    [SerializeField] private float gravity = -20f;

    [Header("Patrulla")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolPointReachDistance = 0.4f;
    [SerializeField] private float waitAtPatrolPointTime = 1f;

    [Header("Detección")]
    [SerializeField] private float detectionRange = 7f;
    [SerializeField] private float losePlayerRange = 10f;
    [SerializeField] private float attackRange = 1.3f;

    [Header("Visión")]
    [SerializeField] private bool useLineOfSight = true;
    [SerializeField] private float fieldOfViewAngle = 120f;
    [SerializeField] private float eyeHeight = 1.3f;
    [SerializeField] private float playerTargetHeight = 1.1f;
    [SerializeField] private float loseSightGraceTime = 1.5f;

    [Header("Ataque")]
    [SerializeField] private int damage = 15;
    [SerializeField] private float attackCooldown = 1.5f;

    [Header("Ataque - Timing")]
    [SerializeField] private float attackWindUpTime = 0.35f;
    [SerializeField] private float attackHitTime = 0.12f;
    [SerializeField] private float attackRecoveryTime = 0.45f;

    [Header("Ataque - Movimiento")]
    [SerializeField] private float attackLungeDistance = 0.45f;

    [Header("Audio")]
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip attackHitSound;
    [SerializeField] private float attackVolume = 0.8f;
    [SerializeField] private float attackHitVolume = 0.8f;

    [Header("Debug")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private bool showVisionDebugRay = true;

    private CharacterController characterController;
    private Transform playerTransform;
    private PlayerHealth playerHealth;

    private EnemyState currentState = EnemyState.Patrol;

    private Vector3 homePosition;
    private Vector3 lastKnownPlayerPosition;

    private float verticalVelocity;
    private float lastAttackTime;
    private float lastTimePlayerSeen;

    private bool isWaitingAtPatrolPoint;

    private int currentPatrolIndex;
    private Coroutine attackRoutine;
    private Coroutine patrolWaitRoutine;

    private Collider[] enemyColliders;
    private Collider[] playerColliders;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        homePosition = transform.position;
        lastKnownPlayerPosition = homePosition;

        enemyColliders = GetComponentsInChildren<Collider>();
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerTransform = player.transform;
            playerHealth = player.GetComponent<PlayerHealth>();
            playerColliders = player.GetComponentsInChildren<Collider>();
        }
    }

    private void Update()
    {
        if (IsGameplayPaused())
            return;

        if (playerTransform == null)
            return;

        HandleState();
    }

    private void HandleState()
    {
        if (playerHealth != null && playerHealth.IsDead)
        {
            ApplyGravity();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        bool canSeePlayer = CanSeePlayer(distanceToPlayer);

        if (canSeePlayer)
        {
            lastKnownPlayerPosition = playerTransform.position;
            lastTimePlayerSeen = Time.time;
        }

        if (currentState != EnemyState.Attack)
        {
            if (distanceToPlayer <= attackRange && canSeePlayer)
            {
                currentState = EnemyState.Attack;
                TryAttack();
                return;
            }

            if (canSeePlayer)
            {
                currentState = EnemyState.Chase;
            }
            else if (currentState == EnemyState.Chase)
            {
                bool tooFar = distanceToPlayer > losePlayerRange;
                bool lostSightTooLong = Time.time > lastTimePlayerSeen + loseSightGraceTime;

                if (tooFar || lostSightTooLong)
                {
                    currentState = EnemyState.ReturnToPatrol;
                }
            }
        }

        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol();
                break;

            case EnemyState.Chase:
                ChasePlayer(distanceToPlayer);
                break;

            case EnemyState.Attack:
                RotateTowardsPlayer();
                ApplyGravity();
                break;

            case EnemyState.ReturnToPatrol:
                ReturnToPatrolArea();
                break;
        }
    }

    private bool CanSeePlayer(float distanceToPlayer)
    {
        if (distanceToPlayer > detectionRange)
            return false;

        Vector3 enemyEyePosition = transform.position + Vector3.up * eyeHeight;
        Vector3 playerTargetPosition = playerTransform.position + Vector3.up * playerTargetHeight;

        Vector3 directionToPlayer = playerTargetPosition - enemyEyePosition;
        Vector3 flatDirectionToPlayer = directionToPlayer;
        flatDirectionToPlayer.y = 0f;

        if (flatDirectionToPlayer.sqrMagnitude <= 0.01f)
            return true;

        float angleToPlayer = Vector3.Angle(transform.forward, flatDirectionToPlayer.normalized);

        if (angleToPlayer > fieldOfViewAngle * 0.5f)
        {
            if (showVisionDebugRay)
            {
                Debug.DrawLine(enemyEyePosition, playerTargetPosition, Color.yellow);
            }

            return false;
        }

        if (!useLineOfSight)
        {
            if (showVisionDebugRay)
            {
                Debug.DrawLine(enemyEyePosition, playerTargetPosition, Color.green);
            }

            return true;
        }

        bool hasClearSight = HasClearLineOfSight(enemyEyePosition, playerTargetPosition);

        if (showVisionDebugRay)
        {
            Debug.DrawLine(
                enemyEyePosition,
                playerTargetPosition,
                hasClearSight ? Color.green : Color.red
            );
        }

        return hasClearSight;
    }

    private bool HasClearLineOfSight(Vector3 startPosition, Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - startPosition;
        float distance = direction.magnitude;

        if (distance <= 0.01f)
            return true;

        RaycastHit[] hits = Physics.RaycastAll(
            startPosition,
            direction.normalized,
            distance,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        if (hits.Length <= 0)
            return true;

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == null)
                continue;

            if (IsEnemyCollider(hit.collider))
                continue;

            if (IsPlayerCollider(hit.collider))
                return true;

            return false;
        }

        return true;
    }

    private bool IsEnemyCollider(Collider colliderToCheck)
    {
        if (enemyColliders == null)
            return false;

        foreach (Collider enemyCollider in enemyColliders)
        {
            if (enemyCollider == colliderToCheck)
                return true;
        }

        return false;
    }

    private bool IsPlayerCollider(Collider colliderToCheck)
    {
        if (playerColliders == null)
            return false;

        foreach (Collider playerCollider in playerColliders)
        {
            if (playerCollider == colliderToCheck)
                return true;
        }

        return false;
    }

    private void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length <= 0)
        {
            ApplyGravity();
            return;
        }

        if (isWaitingAtPatrolPoint)
        {
            ApplyGravity();
            return;
        }

        Transform targetPoint = patrolPoints[currentPatrolIndex];

        if (targetPoint == null)
        {
            GoToNextPatrolPoint();
            return;
        }

        MoveTowardsPosition(targetPoint.position, patrolSpeed);

        float distanceToPoint = Vector3.Distance(
            new Vector3(transform.position.x, 0f, transform.position.z),
            new Vector3(targetPoint.position.x, 0f, targetPoint.position.z)
        );

        if (distanceToPoint <= patrolPointReachDistance)
        {
            StartPatrolWait();
        }
    }

    private void StartPatrolWait()
    {
        if (patrolWaitRoutine != null)
        {
            StopCoroutine(patrolWaitRoutine);
        }

        patrolWaitRoutine = StartCoroutine(PatrolWaitRoutine());
    }

    private IEnumerator PatrolWaitRoutine()
    {
        isWaitingAtPatrolPoint = true;

        yield return new WaitForSeconds(waitAtPatrolPointTime);

        GoToNextPatrolPoint();

        isWaitingAtPatrolPoint = false;
        patrolWaitRoutine = null;
    }

    private void GoToNextPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length <= 0)
            return;

        currentPatrolIndex++;

        if (currentPatrolIndex >= patrolPoints.Length)
        {
            currentPatrolIndex = 0;
        }
    }

    private void ChasePlayer(float distanceToPlayer)
    {
        if (distanceToPlayer > losePlayerRange)
        {
            currentState = EnemyState.ReturnToPatrol;
            return;
        }

        MoveTowardsPosition(lastKnownPlayerPosition, moveSpeed);

        float distanceToLastKnown = Vector3.Distance(
            new Vector3(transform.position.x, 0f, transform.position.z),
            new Vector3(lastKnownPlayerPosition.x, 0f, lastKnownPlayerPosition.z)
        );

        if (distanceToLastKnown <= patrolPointReachDistance &&
            Time.time > lastTimePlayerSeen + loseSightGraceTime)
        {
            currentState = EnemyState.ReturnToPatrol;
        }
    }

    private void ReturnToPatrolArea()
    {
        Vector3 targetPosition = homePosition;

        if (patrolPoints != null && patrolPoints.Length > 0 && patrolPoints[0] != null)
        {
            targetPosition = patrolPoints[0].position;
        }

        MoveTowardsPosition(targetPosition, patrolSpeed);

        float distanceToHome = Vector3.Distance(
            new Vector3(transform.position.x, 0f, transform.position.z),
            new Vector3(targetPosition.x, 0f, targetPosition.z)
        );

        if (distanceToHome <= patrolPointReachDistance)
        {
            currentState = EnemyState.Patrol;
        }
    }

    private void MoveTowardsPosition(Vector3 targetPosition, float speed)
    {
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.01f)
        {
            ApplyGravity();
            return;
        }

        Vector3 moveDirection = direction.normalized;

        RotateTowardsDirection(moveDirection);

        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 movement = moveDirection * speed;
        movement.y = verticalVelocity;

        characterController.Move(movement * Time.deltaTime);
    }

    private void TryAttack()
    {
        if (attackRoutine != null)
            return;

        if (playerHealth != null && playerHealth.IsDead)
            return;

        if (Time.time < lastAttackTime + attackCooldown)
        {
            currentState = EnemyState.Chase;
            return;
        }

        lastAttackTime = Time.time;
        attackRoutine = StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        currentState = EnemyState.Attack;

        RotateTowardsPlayer();

        GameAudioManager.Instance?.PlaySFXNoPitch(attackSound, attackVolume);

        yield return new WaitForSeconds(attackWindUpTime);

        yield return LungeForwardRoutine();

        yield return new WaitForSeconds(attackHitTime);

        TryApplyAttackDamage();

        yield return new WaitForSeconds(attackRecoveryTime);

        currentState = EnemyState.Chase;
        attackRoutine = null;
    }

    private IEnumerator LungeForwardRoutine()
    {
        float lungeTime = 0.12f;
        float timer = 0f;

        Vector3 lungeDirection = transform.forward;
        lungeDirection.y = 0f;
        lungeDirection.Normalize();

        while (timer < lungeTime)
        {
            timer += Time.deltaTime;

            if (characterController != null && characterController.enabled)
            {
                Vector3 movement = lungeDirection * (attackLungeDistance / lungeTime);
                characterController.Move(movement * Time.deltaTime);
            }

            yield return null;
        }
    }

    private void TryApplyAttackDamage()
    {
        if (playerTransform == null || playerHealth == null)
            return;

        if (playerHealth.IsDead)
            return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer > attackRange + 0.4f)
        {
            Debug.Log("El enemigo atacó, pero falló.");
            return;
        }

        playerHealth.TakeDamage(damage);

        GameAudioManager.Instance?.PlaySFXNoPitch(attackHitSound, attackHitVolume);

        Debug.Log("El enemigo golpeó al jugador.");
    }

    private void RotateTowardsPlayer()
    {
        if (playerTransform == null)
            return;

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
        if (characterController == null || !characterController.enabled)
            return;

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
        if (GameOverUI.Instance != null && GameOverUI.Instance.IsGameOver)
            return true;

        if (NoteUI.Instance != null && NoteUI.Instance.IsOpen)
            return true;

        if (NoteArchiveUI.Instance != null && NoteArchiveUI.Instance.IsOpen)
            return true;

        if (InventoryUI.Instance != null && InventoryUI.Instance.IsOpen)
            return true;

        return false;
    }

    private void OnDisable()
    {
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }

        if (patrolWaitRoutine != null)
        {
            StopCoroutine(patrolWaitRoutine);
            patrolWaitRoutine = null;
        }

        isWaitingAtPatrolPoint = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos)
            return;

        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.DrawWireSphere(transform.position, losePlayerRange);
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Vector3 eyePosition = transform.position + Vector3.up * eyeHeight;

        Vector3 leftDirection = Quaternion.Euler(0f, -fieldOfViewAngle * 0.5f, 0f) * transform.forward;
        Vector3 rightDirection = Quaternion.Euler(0f, fieldOfViewAngle * 0.5f, 0f) * transform.forward;

        Gizmos.DrawRay(eyePosition, leftDirection * detectionRange);
        Gizmos.DrawRay(eyePosition, rightDirection * detectionRange);

        if (patrolPoints == null)
            return;

        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (patrolPoints[i] == null)
                continue;

            Gizmos.DrawWireSphere(patrolPoints[i].position, patrolPointReachDistance);

            int nextIndex = i + 1;

            if (nextIndex >= patrolPoints.Length)
            {
                nextIndex = 0;
            }

            if (patrolPoints[nextIndex] != null)
            {
                Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[nextIndex].position);
            }
        }
    }
}