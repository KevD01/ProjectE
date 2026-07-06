using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance;

    [Header("Checkpoint actual")]
    [SerializeField] private Transform currentCheckpoint;
    [SerializeField] private Transform currentCameraPoint;

    private Vector3 checkpointPosition;
    private Quaternion checkpointRotation;
    private bool hasCheckpoint;

    private int checkpointVersion;

    private List<PlayerInventory.InventorySaveEntry> savedInventory;
    private int savedWeaponAmmoInClip;
    private bool hasWeaponState;

    private int savedHealth;
    private bool hasHealthState;

    private readonly List<PickupState> savedPickups = new List<PickupState>();
    private readonly List<EnemyState> savedEnemies = new List<EnemyState>();
    private readonly List<RestorableObjectState> savedRestorableObjects = new List<RestorableObjectState>();

    public bool HasCheckpoint => hasCheckpoint;
    public Transform CurrentCameraPoint => currentCameraPoint;
    public int CurrentCheckpointVersion => checkpointVersion;

    private class PickupState
    {
        public ItemPickup pickup;
        public bool activeSelf;
    }

    private class EnemyState
    {
        public EnemyHealth enemy;
        public bool activeSelf;
        public bool isDead;
        public int currentHealth;
        public Vector3 position;
        public Quaternion rotation;
    }

    private class RestorableObjectState
    {
        public CheckpointRestorableObject restorableObject;
        public bool activeSelf;
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (currentCheckpoint != null)
        {
            SetCheckpoint(currentCheckpoint, currentCameraPoint);
        }
    }

    public void SetCheckpoint(Transform checkpoint, Transform cameraPoint)
    {
        if (checkpoint == null)
            return;

        checkpointVersion++;

        currentCheckpoint = checkpoint;
        currentCameraPoint = cameraPoint;

        checkpointPosition = checkpoint.position;
        checkpointRotation = checkpoint.rotation;

        CapturePlayerState();
        CaptureWorldState();

        hasCheckpoint = true;

        Debug.Log("Checkpoint guardado: " + checkpoint.name);
    }

    private void CapturePlayerState()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
            return;

        if (PlayerInventory.Instance != null)
        {
            savedInventory = PlayerInventory.Instance.CaptureInventorySnapshot();
        }

        PlayerWeaponController weapon = playerObject.GetComponent<PlayerWeaponController>();

        if (weapon != null)
        {
            savedWeaponAmmoInClip = weapon.CurrentAmmoInClip;
            hasWeaponState = true;
        }

        PlayerHealth health = playerObject.GetComponent<PlayerHealth>();

        if (health != null)
        {
            savedHealth = health.CurrentHealth;
            hasHealthState = true;
        }

        Debug.Log("Estado del jugador guardado en checkpoint.");
    }

    private void CaptureWorldState()
    {
        savedPickups.Clear();
        savedEnemies.Clear();
        savedRestorableObjects.Clear();

        ItemPickup[] pickups = FindObjectsByType<ItemPickup>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (ItemPickup pickup in pickups)
        {
            if (pickup == null)
                continue;

            savedPickups.Add(new PickupState
            {
                pickup = pickup,
                activeSelf = pickup.gameObject.activeSelf
            });
        }

        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (EnemyHealth enemy in enemies)
        {
            if (enemy == null)
                continue;

            savedEnemies.Add(new EnemyState
            {
                enemy = enemy,
                activeSelf = enemy.gameObject.activeSelf,
                isDead = enemy.IsDead,
                currentHealth = enemy.CurrentHealth,
                position = enemy.transform.position,
                rotation = enemy.transform.rotation
            });
        }

        CheckpointRestorableObject[] restorableObjects = FindObjectsByType<CheckpointRestorableObject>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (CheckpointRestorableObject restorableObject in restorableObjects)
        {
            if (restorableObject == null)
                continue;

            savedRestorableObjects.Add(new RestorableObjectState
            {
                restorableObject = restorableObject,
                activeSelf = restorableObject.CaptureActiveState()
            });
        }

        Debug.Log("Estado del mundo guardado en checkpoint.");
    }

    public void RespawnPlayer(GameObject playerObject)
    {
        if (playerObject == null)
            return;

        if (!hasCheckpoint)
            return;

        RemoveObjectsSpawnedAfterCheckpoint();

        RestoreWorldState();
        RestorePlayerPosition(playerObject);
        RestorePlayerState(playerObject);
        RestoreCamera();

        Debug.Log("Estado completo restaurado desde checkpoint.");
    }

    private void RestoreWorldState()
    {
        foreach (PickupState pickupState in savedPickups)
        {
            if (pickupState == null || pickupState.pickup == null)
                continue;

            pickupState.pickup.RestorePickupFromCheckpoint(pickupState.activeSelf);
        }

        foreach (EnemyState enemyState in savedEnemies)
        {
            if (enemyState == null || enemyState.enemy == null)
                continue;

            enemyState.enemy.RestoreFromCheckpoint(
                enemyState.activeSelf,
                enemyState.isDead,
                enemyState.currentHealth,
                enemyState.position,
                enemyState.rotation
            );
        }

        foreach (RestorableObjectState objectState in savedRestorableObjects)
        {
            if (objectState == null || objectState.restorableObject == null)
                continue;

            objectState.restorableObject.RestoreActiveState(objectState.activeSelf);
        }
    }

    private void RemoveObjectsSpawnedAfterCheckpoint()
    {
        CheckpointSpawnedObject[] spawnedObjects = FindObjectsByType<CheckpointSpawnedObject>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (CheckpointSpawnedObject spawnedObject in spawnedObjects)
        {
            if (spawnedObject == null)
                continue;

            if (spawnedObject.SpawnCheckpointVersion >= checkpointVersion)
            {
                Destroy(spawnedObject.gameObject);
            }
        }
    }

    private void RestorePlayerPosition(GameObject playerObject)
    {
        PlayerTankController movement = playerObject.GetComponent<PlayerTankController>();

        if (movement != null)
        {
            movement.TeleportToPosition(checkpointPosition, checkpointRotation);
        }
        else
        {
            playerObject.transform.SetPositionAndRotation(checkpointPosition, checkpointRotation);
            Physics.SyncTransforms();
        }
    }

    private void RestorePlayerState(GameObject playerObject)
    {
        if (PlayerInventory.Instance != null && savedInventory != null)
        {
            PlayerInventory.Instance.RestoreInventorySnapshot(savedInventory);
        }

        PlayerWeaponController weapon = playerObject.GetComponent<PlayerWeaponController>();

        if (weapon != null && hasWeaponState)
        {
            weapon.RestoreAmmoInClip(savedWeaponAmmoInClip);
        }

        PlayerHealth health = playerObject.GetComponent<PlayerHealth>();

        if (health != null && hasHealthState)
        {
            health.RestoreHealthFromCheckpoint(savedHealth);
        }
    }

    private void RestoreCamera()
    {
        if (FixedCameraManager.Instance == null)
            return;

        FixedCameraManager.Instance.ClearActiveZones();

        if (currentCameraPoint != null)
        {
            FixedCameraManager.Instance.ChangeCamera(currentCameraPoint);
        }
    }
}