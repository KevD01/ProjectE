using System.Collections;
using UnityEngine;

public class PlayerWeaponController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private ItemData requiredWeaponItem;
    [SerializeField] private ItemData ammoItem;
    [SerializeField] private GameObject muzzleFlashObject;

    [Header("Input")]
    [SerializeField] private int aimMouseButton = 1;
    [SerializeField] private int fireMouseButton = 0;
    [SerializeField] private KeyCode reloadKey = KeyCode.R;

    [Header("Pistola")]
    [SerializeField] private int damage = 25;
    [SerializeField] private float fireRange = 12f;
    [SerializeField] private float fireCooldown = 0.45f;
    [SerializeField] private float shotAngle = 100f;

    [Header("Cargador")]
    [SerializeField] private int maxAmmoInClip = 7;
    [SerializeField] private int currentAmmoInClip = 0;
    [SerializeField] private float reloadTime = 1.2f;

    [Header("Feedback visual")]
    [SerializeField] private float muzzleFlashTime = 0.05f;
    [SerializeField] private float cameraShakeDuration = 0.08f;
    [SerializeField] private float cameraShakeStrength = 0.06f;

    [Header("Audio")]
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip reloadSound;
    [SerializeField] private AudioClip emptyGunSound;
    [SerializeField] private AudioClip noWeaponSound;
    [SerializeField] private float shootVolume = 1f;
    [SerializeField] private float reloadVolume = 0.8f;
    [SerializeField] private float emptyGunVolume = 0.7f;

    [Header("Mensajes")]
    [SerializeField] private float noWeaponMessageCooldown = 1.2f;

    [Header("Debug")]
    [SerializeField] private bool showDebug = true;

    private PlayerTankController playerMovement;
    private float lastFireTime;
    private float lastNoWeaponMessageTime;
    private bool isAiming;
    private bool isReloading;
    private Coroutine muzzleFlashRoutine;

    public int CurrentAmmoInClip => currentAmmoInClip;
    public int MaxAmmoInClip => maxAmmoInClip;
    public ItemData AmmoItem => ammoItem;
    public bool IsReloading => isReloading;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerTankController>();

        currentAmmoInClip = Mathf.Clamp(currentAmmoInClip, 0, maxAmmoInClip);

        if (muzzleFlashObject != null)
        {
            muzzleFlashObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (IsGameplayPaused())
        {
            SetAiming(false);
            return;
        }

        HandleAiming();
        HandleReload();
        HandleShooting();
    }

    private void HandleAiming()
    {
        bool wantsToAim = Input.GetMouseButton(aimMouseButton);

        if (!wantsToAim)
        {
            SetAiming(false);
            return;
        }

        if (!HasRequiredWeaponEquipped())
        {
            SetAiming(false);

            if (Input.GetMouseButtonDown(aimMouseButton))
            {
                ShowNoWeaponMessage();
            }

            return;
        }

        SetAiming(true);
    }

    private void HandleReload()
    {
        if (!HasRequiredWeaponEquipped())
            return;

        if (isReloading)
            return;

        if (Input.GetKeyDown(reloadKey))
        {
            TryReload();
        }
    }

    private void HandleShooting()
    {
        if (!isAiming)
            return;

        if (isReloading)
            return;

        if (!Input.GetMouseButtonDown(fireMouseButton))
            return;

        if (Time.time < lastFireTime + fireCooldown)
            return;

        TryShoot();
    }

    private void TryShoot()
    {
        if (!HasRequiredWeaponEquipped())
        {
            ShowNoWeaponMessage();
            SetAiming(false);
            return;
        }

        if (currentAmmoInClip <= 0)
        {
            GameAudioManager.Instance?.PlaySFX(emptyGunSound, emptyGunVolume);

            InteractionPromptUI.Instance?.Show("No hay balas cargadas. Presiona R para recargar.");
            Invoke(nameof(HidePrompt), 1.4f);
            return;
        }

        currentAmmoInClip--;
        lastFireTime = Time.time;

        PlayShootFeedback();

        EnemyHealth enemy = FindEnemyInFront();

        if (enemy != null)
        {
            enemy.TakeDamage(damage, transform.position);

            if (showDebug)
            {
                Debug.Log("Disparo golpeó a: " + enemy.gameObject.name);
                Debug.DrawLine(GetFireOrigin(), enemy.transform.position + Vector3.up, Color.red, 1f);
            }
        }
        else
        {
            if (showDebug)
            {
                Debug.Log("Disparo falló. No hay enemigo frente al jugador.");
                Debug.DrawRay(GetFireOrigin(), transform.forward * fireRange, Color.red, 1f);
            }
        }

        Debug.Log("Disparo. Cargador: " + currentAmmoInClip + "/" + maxAmmoInClip);
    }

    private void TryReload()
    {
        if (ammoItem == null)
        {
            Debug.LogWarning("No hay Ammo Item asignado.");
            return;
        }

        if (PlayerInventory.Instance == null)
        {
            Debug.LogWarning("No existe PlayerInventory en la escena.");
            return;
        }

        if (currentAmmoInClip >= maxAmmoInClip)
        {
            InteractionPromptUI.Instance?.Show("El cargador ya está lleno.");
            Invoke(nameof(HidePrompt), 1.2f);
            return;
        }

        int reserveAmmo = PlayerInventory.Instance.GetTotalQuantity(ammoItem);

        if (reserveAmmo <= 0)
        {
            GameAudioManager.Instance?.PlaySFX(emptyGunSound, emptyGunVolume);

            InteractionPromptUI.Instance?.Show("No tienes munición para recargar.");
            Invoke(nameof(HidePrompt), 1.2f);
            return;
        }

        StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        isReloading = true;

        GameAudioManager.Instance?.PlaySFXNoPitch(reloadSound, reloadVolume);

        InteractionPromptUI.Instance?.Show("Recargando...");

        yield return new WaitForSeconds(reloadTime);

        int missingAmmo = maxAmmoInClip - currentAmmoInClip;
        int reserveAmmo = PlayerInventory.Instance.GetTotalQuantity(ammoItem);
        int ammoToLoad = Mathf.Min(missingAmmo, reserveAmmo);

        bool removed = PlayerInventory.Instance.RemoveItem(ammoItem, ammoToLoad);

        if (removed)
        {
            currentAmmoInClip += ammoToLoad;
            currentAmmoInClip = Mathf.Clamp(currentAmmoInClip, 0, maxAmmoInClip);

            Debug.Log("Recarga completa. Cargador: " + currentAmmoInClip + "/" + maxAmmoInClip);
        }

        InteractionPromptUI.Instance?.Hide();

        isReloading = false;
    }

    public int GetReserveAmmo()
    {
        if (ammoItem == null || PlayerInventory.Instance == null)
            return 0;

        return PlayerInventory.Instance.GetTotalQuantity(ammoItem);
    }

    public bool HasRequiredWeaponEquipped()
    {
        if (requiredWeaponItem == null)
            return true;

        if (PlayerEquipment.Instance == null)
            return false;

        return PlayerEquipment.Instance.HasEquippedWeapon(requiredWeaponItem);
    }

    private void ShowNoWeaponMessage()
    {
        if (Time.time < lastNoWeaponMessageTime + noWeaponMessageCooldown)
            return;

        lastNoWeaponMessageTime = Time.time;

        GameAudioManager.Instance?.PlaySFX(noWeaponSound, emptyGunVolume);

        InteractionPromptUI.Instance?.Show("No tienes una pistola equipada.");
        Invoke(nameof(HidePrompt), 1.2f);
    }

    private void PlayShootFeedback()
    {
        GameAudioManager.Instance?.PlaySFX(shootSound, shootVolume);

        if (muzzleFlashObject != null)
        {
            if (muzzleFlashRoutine != null)
            {
                StopCoroutine(muzzleFlashRoutine);
            }

            muzzleFlashRoutine = StartCoroutine(MuzzleFlashRoutine());
        }

        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(cameraShakeDuration, cameraShakeStrength);
        }
    }

    private IEnumerator MuzzleFlashRoutine()
    {
        muzzleFlashObject.SetActive(true);

        yield return new WaitForSeconds(muzzleFlashTime);

        muzzleFlashObject.SetActive(false);
        muzzleFlashRoutine = null;
    }

    private EnemyHealth FindEnemyInFront()
    {
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);

        EnemyHealth bestEnemy = null;
        float bestDistance = Mathf.Infinity;

        foreach (EnemyHealth enemy in enemies)
        {
            if (enemy == null)
                continue;

            if (enemy.IsDead)
                continue;

            Vector3 directionToEnemy = enemy.transform.position - transform.position;
            directionToEnemy.y = 0f;

            float distanceToEnemy = directionToEnemy.magnitude;

            if (distanceToEnemy > fireRange)
                continue;

            if (distanceToEnemy <= 0.01f)
                continue;

            float angleToEnemy = Vector3.Angle(transform.forward, directionToEnemy.normalized);

            if (showDebug)
            {
                Debug.Log(
                    "Enemigo detectado: " + enemy.gameObject.name +
                    " | Distancia: " + distanceToEnemy.ToString("F2") +
                    " | Ángulo: " + angleToEnemy.ToString("F2")
                );
            }

            if (angleToEnemy > shotAngle * 0.5f)
                continue;

            if (distanceToEnemy < bestDistance)
            {
                bestDistance = distanceToEnemy;
                bestEnemy = enemy;
            }
        }

        return bestEnemy;
    }

    private Vector3 GetFireOrigin()
    {
        if (firePoint != null)
        {
            return firePoint.position;
        }

        return transform.position + Vector3.up * 1.35f + transform.forward * 1.1f;
    }

    private void SetAiming(bool aiming)
    {
        isAiming = aiming;

        if (playerMovement != null)
        {
            playerMovement.SetMovementLocked(isAiming);
        }
    }

    private void HidePrompt()
    {
        if (!isReloading)
        {
            InteractionPromptUI.Instance?.Hide();
        }
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

    private void OnDisable()
    {
        SetAiming(false);

        if (muzzleFlashObject != null)
        {
            muzzleFlashObject.SetActive(false);
        }
    }
}