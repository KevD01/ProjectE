using System.Collections;
using UnityEngine;

public class EnemyHitReaction : MonoBehaviour
{
    [Header("Stun")]
    [SerializeField] private float stunTime = 0.25f;

    [Header("Retroceso")]
    [SerializeField] private float knockbackDistance = 0.35f;
    [SerializeField] private float knockbackTime = 0.12f;

    [Header("Flash de daño")]
    [SerializeField] private Renderer enemyRenderer;
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float flashTime = 0.08f;

    private BasicEnemy basicEnemy;
    private CharacterController characterController;
    private Coroutine reactionRoutine;
    private Color originalColor;
    private bool hasOriginalColor;

    private void Awake()
    {
        basicEnemy = GetComponent<BasicEnemy>();
        characterController = GetComponent<CharacterController>();

        if (enemyRenderer == null)
        {
            enemyRenderer = GetComponentInChildren<Renderer>();
        }

        if (enemyRenderer != null && enemyRenderer.material != null)
        {
            originalColor = enemyRenderer.material.color;
            hasOriginalColor = true;
        }
    }

    public void PlayHitReaction(Vector3 damageSourcePosition)
    {
        if (reactionRoutine != null)
        {
            StopCoroutine(reactionRoutine);
        }

        reactionRoutine = StartCoroutine(HitReactionRoutine(damageSourcePosition));
    }

    private IEnumerator HitReactionRoutine(Vector3 damageSourcePosition)
    {
        if (basicEnemy != null)
        {
            basicEnemy.enabled = false;
        }

        if (enemyRenderer != null)
        {
            enemyRenderer.material.color = hitColor;
        }

        Vector3 knockbackDirection = transform.position - damageSourcePosition;
        knockbackDirection.y = 0f;

        if (knockbackDirection.sqrMagnitude < 0.01f)
        {
            knockbackDirection = -transform.forward;
        }

        knockbackDirection.Normalize();

        float timer = 0f;

        while (timer < knockbackTime)
        {
            timer += Time.deltaTime;

            if (characterController != null && characterController.enabled)
            {
                Vector3 movement = knockbackDirection * (knockbackDistance / knockbackTime);
                characterController.Move(movement * Time.deltaTime);
            }

            yield return null;
        }

        yield return new WaitForSeconds(stunTime);

        if (enemyRenderer != null && hasOriginalColor)
        {
            enemyRenderer.material.color = originalColor;
        }

        if (basicEnemy != null)
        {
            basicEnemy.enabled = true;
        }

        reactionRoutine = null;
    }

    public void StopReaction()
    {
        if (reactionRoutine != null)
        {
            StopCoroutine(reactionRoutine);
            reactionRoutine = null;
        }

        if (enemyRenderer != null && hasOriginalColor)
        {
            enemyRenderer.material.color = originalColor;
        }
    }
}