using System.Collections.Generic;
using UnityEngine;

public class FixedCameraManager : MonoBehaviour
{
    public static FixedCameraManager Instance;

    [Header("Cámara principal")]
    [SerializeField] private Camera targetCamera;

    [Header("Cámara inicial")]
    [SerializeField] private Transform startingCameraPoint;

    [Header("Configuración")]
    [SerializeField] private bool instantCut = true;
    [SerializeField] private float smoothSpeed = 8f;

    private Transform currentCameraPoint;
    private readonly List<CameraZoneTrigger> activeZones = new List<CameraZoneTrigger>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (startingCameraPoint != null)
        {
            ChangeCamera(startingCameraPoint);
        }
    }

    private void LateUpdate()
    {
        if (instantCut)
            return;

        if (currentCameraPoint == null || targetCamera == null)
            return;

        targetCamera.transform.position = Vector3.Lerp(
            targetCamera.transform.position,
            currentCameraPoint.position,
            smoothSpeed * Time.deltaTime
        );

        targetCamera.transform.rotation = Quaternion.Slerp(
            targetCamera.transform.rotation,
            currentCameraPoint.rotation,
            smoothSpeed * Time.deltaTime
        );
    }

    public void RegisterZone(CameraZoneTrigger zone)
    {
        if (zone == null)
            return;

        if (!activeZones.Contains(zone))
        {
            activeZones.Add(zone);
        }

        EvaluateBestCamera();
    }

    public void UnregisterZone(CameraZoneTrigger zone)
    {
        if (zone == null)
            return;

        if (activeZones.Contains(zone))
        {
            activeZones.Remove(zone);
        }

        EvaluateBestCamera();
    }

    public void ClearActiveZones()
    {
        activeZones.Clear();
    }

    private void EvaluateBestCamera()
    {
        CameraZoneTrigger bestZone = null;

        foreach (CameraZoneTrigger zone in activeZones)
        {
            if (zone == null)
                continue;

            if (zone.CameraPoint == null)
                continue;

            if (bestZone == null || zone.Priority > bestZone.Priority)
            {
                bestZone = zone;
            }
        }

        if (bestZone != null)
        {
            ChangeCamera(bestZone.CameraPoint);
        }
        else if (startingCameraPoint != null)
        {
            ChangeCamera(startingCameraPoint);
        }
    }

    public void ChangeCamera(Transform newCameraPoint)
    {
        if (newCameraPoint == null)
            return;

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera == null)
            return;

        if (currentCameraPoint == newCameraPoint)
            return;

        currentCameraPoint = newCameraPoint;

        if (instantCut)
        {
            targetCamera.transform.position = currentCameraPoint.position;
            targetCamera.transform.rotation = currentCameraPoint.rotation;
        }
    }
}