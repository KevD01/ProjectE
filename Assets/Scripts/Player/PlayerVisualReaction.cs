using System.Collections;
using UnityEngine;

public class PlayerVisualReaction : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform visualRoot;
    [SerializeField] private Renderer[] renderersToAffect;

    [Header("Flash de daño")]
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float hitFlashTime = 0.12f;

    [Header("Parpadeo de invulnerabilidad")]
    [SerializeField] private bool blinkDuringInvulnerability = true;
    [SerializeField] private float blinkInterval = 0.08f;

    [Header("Muerte visual")]
    [SerializeField] private bool rotateOnDeath = true;
    [SerializeField] private Vector3 deathRotation = new Vector3(90f, 0f, 0f);
    [SerializeField] private float deathRotateTime = 0.45f;

    private Color[] originalColors;
    private Quaternion originalLocalRotation;

    private Coroutine hitRoutine;
    private Coroutine blinkRoutine;
    private Coroutine deathRoutine;
    private bool isDead;

    private void Awake()
    {
        if (visualRoot == null)
        {
            visualRoot = transform;
        }

        originalLocalRotation = visualRoot.localRotation;

        if (renderersToAffect == null || renderersToAffect.Length <= 0)
        {
            renderersToAffect = GetComponentsInChildren<Renderer>();
        }

        SaveOriginalColors();
    }

    private void SaveOriginalColors()
    {
        if (renderersToAffect == null)
            return;

        originalColors = new Color[renderersToAffect.Length];

        for (int i = 0; i < renderersToAffect.Length; i++)
        {
            if (renderersToAffect[i] != null && renderersToAffect[i].material != null)
            {
                originalColors[i] = renderersToAffect[i].material.color;
            }
        }
    }

    public void PlayHitFlash()
    {
        if (isDead)
            return;

        if (hitRoutine != null)
        {
            StopCoroutine(hitRoutine);
        }

        hitRoutine = StartCoroutine(HitFlashRoutine());
    }

    private IEnumerator HitFlashRoutine()
    {
        SetRenderersColor(hitColor);

        yield return new WaitForSeconds(hitFlashTime);

        RestoreOriginalColors();

        hitRoutine = null;
    }

    public void StartInvulnerabilityBlink(float duration)
    {
        if (isDead)
            return;

        if (!blinkDuringInvulnerability)
            return;

        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
        }

        blinkRoutine = StartCoroutine(InvulnerabilityBlinkRoutine(duration));
    }

    private IEnumerator InvulnerabilityBlinkRoutine(float duration)
    {
        float timer = 0f;
        bool visible = true;

        while (timer < duration)
        {
            timer += blinkInterval;
            visible = !visible;

            SetRenderersVisible(visible);

            yield return new WaitForSeconds(blinkInterval);
        }

        SetRenderersVisible(true);
        blinkRoutine = null;
    }

    public void PlayDeathVisual()
    {
        if (isDead)
            return;

        isDead = true;

        StopAllVisualRoutines();

        SetRenderersVisible(true);
        RestoreOriginalColors();

        if (rotateOnDeath && visualRoot != null)
        {
            deathRoutine = StartCoroutine(DeathRotateRoutine());
        }
    }

    private IEnumerator DeathRotateRoutine()
    {
        Quaternion startRotation = visualRoot.localRotation;
        Quaternion targetRotation = Quaternion.Euler(deathRotation);

        float timer = 0f;

        while (timer < deathRotateTime)
        {
            timer += Time.deltaTime;
            float t = timer / deathRotateTime;

            visualRoot.localRotation = Quaternion.Slerp(startRotation, targetRotation, t);

            yield return null;
        }

        visualRoot.localRotation = targetRotation;
        deathRoutine = null;
    }

    public void ResetVisual()
    {
        isDead = false;

        StopAllVisualRoutines();

        SetRenderersVisible(true);
        RestoreOriginalColors();

        if (visualRoot != null)
        {
            visualRoot.localRotation = originalLocalRotation;
        }
    }

    private void StopAllVisualRoutines()
    {
        if (hitRoutine != null)
        {
            StopCoroutine(hitRoutine);
            hitRoutine = null;
        }

        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
            blinkRoutine = null;
        }

        if (deathRoutine != null)
        {
            StopCoroutine(deathRoutine);
            deathRoutine = null;
        }
    }

    private void SetRenderersColor(Color color)
    {
        if (renderersToAffect == null)
            return;

        foreach (Renderer rend in renderersToAffect)
        {
            if (rend != null && rend.material != null)
            {
                rend.material.color = color;
            }
        }
    }

    private void RestoreOriginalColors()
    {
        if (renderersToAffect == null || originalColors == null)
            return;

        for (int i = 0; i < renderersToAffect.Length; i++)
        {
            if (renderersToAffect[i] != null && renderersToAffect[i].material != null)
            {
                renderersToAffect[i].material.color = originalColors[i];
            }
        }
    }

    private void SetRenderersVisible(bool visible)
    {
        if (renderersToAffect == null)
            return;

        foreach (Renderer rend in renderersToAffect)
        {
            if (rend != null)
            {
                rend.enabled = visible;
            }
        }
    }
}