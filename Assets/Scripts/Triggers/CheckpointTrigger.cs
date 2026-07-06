using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{
    [Header("Checkpoint")]
    [SerializeField] private Transform checkpointSpawnPoint;
    [SerializeField] private Transform checkpointCameraPoint;
    [SerializeField] private bool saveOnlyOnce = true;

    [Header("Debug")]
    [SerializeField] private bool showDebug = true;

    private bool alreadySaved;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        SaveCheckpoint();
    }

    private void SaveCheckpoint()
    {
        if (saveOnlyOnce && alreadySaved)
            return;

        if (CheckpointManager.Instance == null)
        {
            Debug.LogWarning("No existe CheckpointManager en la escena.");
            return;
        }

        if (checkpointSpawnPoint == null)
        {
            Debug.LogWarning(gameObject.name + " no tiene Checkpoint Spawn Point asignado.");
            return;
        }

        CheckpointManager.Instance.SetCheckpoint(checkpointSpawnPoint, checkpointCameraPoint);

        alreadySaved = true;

        if (showDebug)
        {
            Debug.Log(gameObject.name + " activó checkpoint.");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}