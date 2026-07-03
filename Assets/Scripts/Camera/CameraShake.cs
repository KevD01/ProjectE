using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    private Coroutine shakeRoutine;

    private void Awake()
    {
        Instance = this;
    }

    public void Shake(float duration, float strength)
    {
        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
        }

        shakeRoutine = StartCoroutine(ShakeRoutine(duration, strength));
    }

    private IEnumerator ShakeRoutine(float duration, float strength)
    {
        Vector3 originalPosition = transform.position;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            Vector3 randomOffset = Random.insideUnitSphere * strength;
            randomOffset.z = 0f;

            transform.position = originalPosition + randomOffset;

            yield return null;
        }

        transform.position = originalPosition;
        shakeRoutine = null;
    }
}