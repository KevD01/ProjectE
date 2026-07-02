using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Objeto")]
    [SerializeField] private ItemData itemData;
    [SerializeField] private int amount = 1;

    [Header("Interacción")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float interactionDistance = 1.8f;
    [SerializeField] private string interactionMessage = "Presiona E para recoger";

    private GameObject playerObject;
    private bool playerInside;
    private bool promptIsVisible;

    private void Update()
    {
        UpdatePrompt();

        if (NoteUI.Instance != null && NoteUI.Instance.IsOpen)
            return;

        if (NoteArchiveUI.Instance != null && NoteArchiveUI.Instance.IsOpen)
            return;

        if (InventoryUI.Instance != null && InventoryUI.Instance.IsOpen)
            return;

        if (playerObject == null)
            return;

        if (!playerInside)
            return;

        if (!IsPlayerCloseEnough())
            return;

        if (Input.GetKeyDown(interactKey))
        {
            PickUpItem();
        }
    }

    private void UpdatePrompt()
    {
        bool shouldShowPrompt =
            playerObject != null &&
            playerInside &&
            IsPlayerCloseEnough() &&
            (NoteUI.Instance == null || !NoteUI.Instance.IsOpen) &&
            (NoteArchiveUI.Instance == null || !NoteArchiveUI.Instance.IsOpen) &&
            (InventoryUI.Instance == null || !InventoryUI.Instance.IsOpen);

        if (shouldShowPrompt)
        {
            if (!promptIsVisible)
            {
                string itemName = itemData != null ? itemData.itemName : "objeto";
                InteractionPromptUI.Instance?.Show(interactionMessage + ": " + itemName);
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

    private bool IsPlayerCloseEnough()
    {
        if (playerObject == null)
            return false;

        float distance = Vector3.Distance(transform.position, playerObject.transform.position);
        return distance <= interactionDistance;
    }

    private void PickUpItem()
    {
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
            InteractionPromptUI.Instance?.Show("Inventario lleno");
            return;
        }

        InteractionPromptUI.Instance?.Hide();
        promptIsVisible = false;

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerObject = other.gameObject;
        playerInside = true;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerObject = other.gameObject;
        playerInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (playerObject == other.gameObject)
        {
            playerInside = false;
            playerObject = null;
        }

        InteractionPromptUI.Instance?.Hide();
        promptIsVisible = false;
    }
}