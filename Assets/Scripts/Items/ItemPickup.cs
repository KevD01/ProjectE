using System.Collections;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Objeto")]
    [SerializeField] private ItemData itemData;
    [SerializeField] private int amount = 1;

    [Header("Interacción")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float interactionDistance = 2.2f;

    [Header("Mensaje al recoger")]
    [SerializeField] private float pickupMessageTime = 1.3f;

    [Header("Audio")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private float pickupVolume = 0.8f;

    [Header("Eventos al recoger")]
    [SerializeField] private GameObject[] objectsToActivateOnPickup;
    [SerializeField] private GameObject[] objectsToDisableOnPickup;
    [SerializeField] private DoorTransition[] doorsToUnlockOnPickup;
    [SerializeField] private AudioClip eventSound;
    [SerializeField] private float eventSoundVolume = 0.9f;

    [TextArea(2, 4)]
    [SerializeField] private string eventMessageAfterPickup = "";
    [SerializeField] private float eventMessageTime = 1.4f;

    [Header("Debug")]
    [SerializeField] private bool showDebug = true;

    private const string interactionMessage = "Oprime E para recoger";

    private GameObject playerObject;
    private bool isPickedUp;
    private bool promptIsVisible;

    private Renderer[] renderers;
    private Collider[] colliders;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        colliders = GetComponentsInChildren<Collider>();
    }

    private void Start()
    {
        playerObject = GameObject.FindGameObjectWithTag("Player");
    }

    private void Update()
    {
        if (isPickedUp)
            return;

        if (IsGameplayPaused())
        {
            HidePrompt();
            return;
        }

        if (playerObject == null)
        {
            playerObject = GameObject.FindGameObjectWithTag("Player");
            return;
        }

        bool playerClose = IsPlayerCloseEnough();

        if (playerClose)
        {
            ShowPrompt();

            if (Input.GetKeyDown(interactKey))
            {
                TryPickup();
            }
        }
        else
        {
            HidePrompt();
        }
    }

    private bool IsPlayerCloseEnough()
    {
        if (playerObject == null)
            return false;

        float distance = Vector3.Distance(transform.position, playerObject.transform.position);
        return distance <= interactionDistance;
    }

    private void ShowPrompt()
    {
        if (promptIsVisible)
            return;

        InteractionPromptUI.Instance?.Show(BuildBeforePickupMessage());
        promptIsVisible = true;
    }

    private void HidePrompt()
    {
        if (!promptIsVisible)
            return;

        InteractionPromptUI.Instance?.Hide();
        promptIsVisible = false;
    }

    private string BuildBeforePickupMessage()
    {
        string description = "";

        if (itemData != null)
        {
            description = itemData.itemDescription;
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            description = "Hay un objeto aquí.";
        }

        return description + "\n\n" + interactionMessage;
    }

    private void TryPickup()
    {
        if (itemData == null)
        {
            Debug.LogWarning(gameObject.name + " no tiene ItemData asignado.");
            return;
        }

        if (PlayerInventory.Instance == null)
        {
            Debug.LogWarning("No existe PlayerInventory en la escena.");
            return;
        }

        PlayerInventory.Instance.AddItem(itemData, amount);

        StartCoroutine(PickupRoutine());
    }

    private IEnumerator PickupRoutine()
    {
        isPickedUp = true;
        promptIsVisible = false;

        DisableVisualAndColliders();

        GameAudioManager.Instance?.PlaySFXNoPitch(pickupSound, pickupVolume);

        InteractionPromptUI.Instance?.Show(BuildCollectedMessage());

        yield return new WaitForSeconds(pickupMessageTime);

        RunPickupEvents();

        if (!string.IsNullOrWhiteSpace(eventMessageAfterPickup))
        {
            InteractionPromptUI.Instance?.Show(eventMessageAfterPickup);
            yield return new WaitForSeconds(eventMessageTime);
        }

        InteractionPromptUI.Instance?.Hide();

        Destroy(gameObject);
    }

    private string BuildCollectedMessage()
    {
        if (itemData == null)
        {
            return "Recogiste un objeto.";
        }

        if (amount > 1)
        {
            return "Recogiste: " + itemData.itemName + " x" + amount;
        }

        return "Recogiste: " + itemData.itemName;
    }

    private void RunPickupEvents()
    {
        ActivateObjects();
        DisableObjects();
        UnlockDoors();
        PlayEventSound();

        if (showDebug)
        {
            Debug.Log(gameObject.name + " ejecutó eventos al recoger.");
        }
    }

    private void ActivateObjects()
    {
        if (objectsToActivateOnPickup == null)
            return;

        foreach (GameObject objectToActivate in objectsToActivateOnPickup)
        {
            if (objectToActivate != null)
            {
                objectToActivate.SetActive(true);
            }
        }
    }

    private void DisableObjects()
    {
        if (objectsToDisableOnPickup == null)
            return;

        foreach (GameObject objectToDisable in objectsToDisableOnPickup)
        {
            if (objectToDisable != null)
            {
                objectToDisable.SetActive(false);
            }
        }
    }

    private void UnlockDoors()
    {
        if (doorsToUnlockOnPickup == null)
            return;

        foreach (DoorTransition door in doorsToUnlockOnPickup)
        {
            if (door != null)
            {
                door.UnlockFromPuzzle();
            }
        }
    }

    private void PlayEventSound()
    {
        GameAudioManager.Instance?.PlaySFXNoPitch(eventSound, eventSoundVolume);
    }

    private void DisableVisualAndColliders()
    {
        if (renderers != null)
        {
            foreach (Renderer rend in renderers)
            {
                if (rend != null)
                {
                    rend.enabled = false;
                }
            }
        }

        if (colliders != null)
        {
            foreach (Collider col in colliders)
            {
                if (col != null)
                {
                    col.enabled = false;
                }
            }
        }
    }

    private bool IsGameplayPaused()
    {
        if (PauseMenuUI.Instance != null && PauseMenuUI.Instance.IsPaused)
            return true;

        if (GameOverUI.Instance != null && GameOverUI.Instance.IsGameOver)
            return true;

        if (EndingUI.Instance != null && EndingUI.Instance.EndingActive)
            return true;

        if (NoteUI.Instance != null && NoteUI.Instance.IsOpen)
            return true;

        if (NoteArchiveUI.Instance != null && NoteArchiveUI.Instance.IsOpen)
            return true;

        if (InventoryUI.Instance != null && InventoryUI.Instance.IsOpen)
            return true;

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}