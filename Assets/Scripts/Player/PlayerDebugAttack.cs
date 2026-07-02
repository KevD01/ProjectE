using UnityEngine;

public class PlayerDebugAttack : MonoBehaviour
{
    [Header("Ataque temporal")]
    [SerializeField] private KeyCode attackKey = KeyCode.F;
    [SerializeField] private int damage = 20;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackAngle = 70f;

    [Header("Debug")]
    [SerializeField] private bool showGizmos = true;

    private void Update()
    {
        if (IsGameplayPaused())
            return;

        if (Input.GetKeyDown(attackKey))
        {
            TryAttack();
        }
    }

    private void TryAttack()
    {
        EnemyHealth targetEnemy = FindEnemyInFront();

        if (targetEnemy == null)
        {
            Debug.Log("No hay enemigo en rango.");
            return;
        }

        targetEnemy.TakeDamage(damage);
        Debug.Log("Ataque de prueba conectado.");
    }

    private EnemyHealth FindEnemyInFront()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange);

        EnemyHealth closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            EnemyHealth enemyHealth = hit.GetComponentInParent<EnemyHealth>();

            if (enemyHealth == null)
                continue;

            if (enemyHealth.IsDead)
                continue;

            Vector3 directionToEnemy = enemyHealth.transform.position - transform.position;
            directionToEnemy.y = 0f;

            float distance = directionToEnemy.magnitude;

            if (distance <= 0.01f)
                continue;

            float angle = Vector3.Angle(transform.forward, directionToEnemy.normalized);

            if (angle > attackAngle * 0.5f)
                continue;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemyHealth;
            }
        }

        return closestEnemy;
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

        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}