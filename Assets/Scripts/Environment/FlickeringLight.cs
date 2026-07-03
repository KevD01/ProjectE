using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class FlickeringLight : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private bool flickerOnStart = true;
    [SerializeField] private float minIntensity = 0.1f;
    [SerializeField] private float maxIntensity = 2f;
    [SerializeField] private float minTimeBetweenFlickers = 0.03f;
    [SerializeField] private float maxTimeBetweenFlickers = 0.18f;

    private Light targetLight;
    private float originalIntensity;
    private Coroutine flickerRoutine;

    private void Awake()
    {
        targetLight = GetComponent<Light>();
        originalIntensity = targetLight.intensity;
    }

    private void Start()
    {
        if (flickerOnStart)
        {
            StartFlicker();
        }
    }

    public void StartFlicker()
    {
        if (flickerRoutine != null)
            return;

        flickerRoutine = StartCoroutine(FlickerLoop());
    }

    public void StopFlicker()
    {
        if (flickerRoutine != null)
        {
            StopCoroutine(flickerRoutine);
            flickerRoutine = null;
        }

        targetLight.intensity = originalIntensity;
    }

    public void FlickerForSeconds(float seconds)
    {
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(FlickerForSecondsRoutine(seconds));
        }
    }

    private IEnumerator FlickerForSecondsRoutine(float seconds)
    {
        float timer = 0f;

        while (timer < seconds)
        {
            timer += Time.deltaTime;

            targetLight.intensity = Random.Range(minIntensity, maxIntensity);

            float waitTime = Random.Range(minTimeBetweenFlickers, maxTimeBetweenFlickers);
            yield return new WaitForSeconds(waitTime);
        }

        targetLight.intensity = originalIntensity;
    }

    private IEnumerator FlickerLoop()
    {
        while (true)
        {
            targetLight.intensity = Random.Range(minIntensity, maxIntensity);

            float waitTime = Random.Range(minTimeBetweenFlickers, maxTimeBetweenFlickers);
            yield return new WaitForSeconds(waitTime);
        }
    }
}