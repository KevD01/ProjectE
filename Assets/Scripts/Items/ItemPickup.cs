using System.Collections;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Objeto")]
    [SerializeField] private ItemData itemData;
    [SerializeField] private int amount = 1;

    [Header("Descripción al acercarse")]
    [SerializeField] private bool showDescriptionOnPrompt = true;

    [TextArea(2, 4)]
    [SerializeField] private string pickupDescription = "Hay un objeto aquí.";

    [SerializeField] private bool useItemDescriptionIfPickupDescriptionIsEmpty = true;

    [Header("Audio")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private AudioClip inventoryFullSound;
    [SerializeField] private float pickupVolume = 0.8f;
    [SerializeField] private float inventoryFullVolume = 0.7f;

    [Header("Mensaje al recoger")]
    [SerializeField] private string pickupMessagePrefix = "Recogiste";
    [SerializeField] private float pickupMessageTime = 1.2f;

    [Header("Interacción")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float interactionDistance = 2.3f;
    [SerializeField] private string interactionMessage = "Presiona E para recoger";

    [Header("Debug")]
    [SerializeField] private bool showInteractionRange = true;

    private GameObject playerObject;
    private bool promptIsVisible;
    private bool isPickedUp;
    private bool showingTemporaryMessage;

    private Coroutine temporaryMessageRoutine;

    private void Start()
    {
        FindPlayer();
    }

    private void Update()
    {
        if (playerObject == null)
        {
            FindPlayer();
        }

        UpdatePrompt();

        if (isPickedUp)
            return;

        if (IsGameplayPaused())
            return;

        if (playerObject == null)
            return;

        if (!IsPlayerCloseEnough())
            return;

        if (Input.GetKeyDown(interactKey))
        {
            PickUpItem();
        }
    }

    private void FindPlayer()
    {
        playerObject = GameObject.FindGameObjectWithTag("Player");
    }

    private void UpdatePrompt()
    {
        if (isPickedUp)
            return;

        if (showingTemporaryMessage)
            return;

        bool shouldShowPrompt =
            playerObject != null &&
            IsPlayerCloseEnough() &&
            !IsGameplayPaused();

        if (shouldShowPrompt)
        {
            if (!promptIsVisible)
            {
                InteractionPromptUI.Instance?.Show(BuildInteractionPrompt());
                promptIsVisible = true;
            }
        }
        else
        {
            if (promptIsVisible)
            {
                InteractionPromptUI.Instance?.Hide();
                promptIsVisible = false;
            }
        }
    }

    private string BuildInteractionPrompt()
    {
        string itemName = itemData != null ? itemData.itemName : "objeto";

        if (!showDescriptionOnPrompt)
        {
            return interactionMessage + ": " + itemName;
        }

        string description = GetPickupDescription();

        if (string.IsNullOrWhiteSpace(description))
        {
            return interactionMessage + ": " + itemName;
        }

        return description + "\n" + interactionMessage + ": " + itemName;
    }

    private string GetPickupDescription()
    {
        if (!string.IsNullOrWhiteSpace(pickupDescription))
        {
            return pickupDescription;
        }

        if (useItemDescriptionIfPickupDescriptionIsEmpty &&
            itemData != null &&
            !string.IsNullOrWhiteSpace(itemData.itemDescription))
        {
            return itemData.itemDescription;
        }

        return "";
    }

    private bool IsPlayerCloseEnough()
    {
        if (playerObject == null)
            return false;

        float distance = Vector3.Distance(transform.position, playerObject.transform.position);
        return distance <= interactionDistance;
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

    private void PickUpItem()
    {
        if (isPickedUp)
            return;

        if (itemData == null)
        {
            Debug.LogWarning("Este pickup no tiene ItemData asignado.");
            return;
        }

        if (PlayerInventory.Instance == null)
        {
            Debug.LogWarning("No existe PlayerInventory en la escena.");
            return;
        }

        bool added = PlayerInventory.Instance.AddItem(itemData, amount);

        if (!added)
        {
            GameAudioManager.Instance?.PlaySFXNoPitch(inventoryFullSound, inventoryFullVolume);
            ShowTemporaryMessage("Inventario lleno.");
            return;
        }

        StartCoroutine(PickupRoutine());
    }

    private IEnumerator PickupRoutine()
    {
        isPickedUp = true;
        promptIsVisible = false;

        DisableVisualAndColliders();

        GameAudioManager.Instance?.PlaySFXNoPitch(pickupSound, pickupVolume);

        string message = BuildPickupMessage();
        InteractionPromptUI.Instance?.Show(message);

        yield return new WaitForSeconds(pickupMessageTime);

        InteractionPromptUI.Instance?.Hide();

        Destroy(gameObject);
    }

    private string BuildPickupMessage()
    {
        if (itemData == null)
            return pickupMessagePrefix + ": objeto";

        if (amount > 1)
        {
            return pickupMessagePrefix + ": " + itemData.itemName + " x" + amount;
        }

        return pickupMessagePrefix + ": " + itemData.itemName;
    }

    private void DisableVisualAndColliders()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();

        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer rend in renderers)
        {
            rend.enabled = false;
        }
    }

    private void ShowTemporaryMessage(string message)
    {
        if (temporaryMessageRoutine != null)
        {
            StopCoroutine(temporaryMessageRoutine);
        }

        temporaryMessageRoutine = StartCoroutine(TemporaryMessageRoutine(message));
    }

    private IEnumerator TemporaryMessageRoutine(string message)
    {
        showingTemporaryMessage = true;
        promptIsVisible = false;

        InteractionPromptUI.Instance?.Show(message);

        yield return new WaitForSeconds(1.2f);

        InteractionPromptUI.Instance?.Hide();

        showingTemporaryMessage = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (!showInteractionRange)
            return;

        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}