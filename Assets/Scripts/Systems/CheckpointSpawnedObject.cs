using UnityEngine;

public class CheckpointSpawnedObject : MonoBehaviour
{
    [SerializeField] private int spawnCheckpointVersion;

    public int SpawnCheckpointVersion => spawnCheckpointVersion;

    public void MarkSpawned(int checkpointVersion)
    {
        spawnCheckpointVersion = checkpointVersion;
    }
}