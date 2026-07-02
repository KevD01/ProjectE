using System.Text;
using TMPro;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance;

    [Header("Referencias")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private TMP_Text inventoryListText;

    [Header("Input")]
    [SerializeField] private KeyCode inventoryKey = KeyCode.I;

    private PlayerTankController playerMovement;
    private bool isOpen;

    public bool IsOpen => isOpen;

    private void Awake()
    {
        Instance = this;

        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerMovement = player.GetComponent<PlayerTankController>();
        }
    }

    private void Update()
    {
        if (NoteUI.Instance != null && NoteUI.Instance.IsOpen)
            return;

        if (NoteArchiveUI.Instance != null && NoteArchiveUI.Instance.IsOpen)
            return;

        if (Input.GetKeyDown(inventoryKey))
        {
            if (isOpen)
                CloseInventory();
            else
                OpenInventory();
        }

        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseInventory();
        }
    }

    private void OpenInventory()
    {
        isOpen = true;

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        InteractionPromptUI.Instance?.Hide();

        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(true);
        }

        RefreshInventory();
    }

    private void CloseInventory()
    {
        isOpen = false;

        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }
    }

    private void RefreshInventory()
    {
        if (inventoryListText == null)
            return;

        if (PlayerInventory.Instance == null || PlayerInventory.Instance.Slots.Count <= 0)
        {
            inventoryListText.text = "No tienes objetos.";
            return;
        }

        StringBuilder builder = new StringBuilder();

        builder.AppendLine("Objetos:");
        builder.AppendLine();

        foreach (InventorySlot slot in PlayerInventory.Instance.Slots)
        {
            if (slot.itemData == null)
                continue;

            if (slot.quantity > 1)
            {
                builder.AppendLine("- " + slot.itemData.itemName + " x" + slot.quantity);
            }
            else
            {
                builder.AppendLine("- " + slot.itemData.itemName);
            }
        }

        inventoryListText.text = builder.ToString();
    }
}