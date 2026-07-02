using UnityEngine;

public class CameraZoneTrigger : MonoBehaviour
{
    [Header("Punto de cámara que se activará")]
    [SerializeField] private Transform cameraPoint;

    [Header("Prioridad")]
    [SerializeField] private int priority = 0;

    public Transform CameraPoint => cameraPoint;
    public int Priority => priority;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (FixedCameraManager.Instance != null)
        {
            FixedCameraManager.Instance.RegisterZone(this);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (FixedCameraManager.Instance != null)
        {
            FixedCameraManager.Instance.RegisterZone(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (FixedCameraManager.Instance != null)
        {
            FixedCameraManager.Instance.UnregisterZone(this);
        }
    }
}