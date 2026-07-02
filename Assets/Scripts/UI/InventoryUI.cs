using System.Text;
using TMPro;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance;

    [Header("Referencias")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private TMP_Text inventoryListText;
    [SerializeField] private TMP_Text inventoryDescriptionText;
    [SerializeField] private TMP_Text inventoryHelpText;

    [Header("Input")]
    [SerializeField] private KeyCode inventoryKey = KeyCode.I;

    private PlayerTankController playerMovement;
    private bool isOpen;
    private int selectedIndex;

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

        if (!isOpen)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseInventory();
            return;
        }

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            MoveSelection(-1);
        }

        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            MoveSelection(1);
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

        selectedIndex = Mathf.Clamp(selectedIndex, 0, GetLastValidIndex());

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

    private void MoveSelection(int direction)
    {
        if (PlayerInventory.Instance == null)
            return;

        int itemCount = PlayerInventory.Instance.Slots.Count;

        if (itemCount <= 0)
            return;

        selectedIndex += direction;

        if (selectedIndex < 0)
        {
            selectedIndex = itemCount - 1;
        }

        if (selectedIndex >= itemCount)
        {
            selectedIndex = 0;
        }

        RefreshInventory();
    }

    private int GetLastValidIndex()
    {
        if (PlayerInventory.Instance == null)
            return 0;

        int itemCount = PlayerInventory.Instance.Slots.Count;

        if (itemCount <= 0)
            return 0;

        return itemCount - 1;
    }

    private void RefreshInventory()
    {
        if (PlayerInventory.Instance == null || PlayerInventory.Instance.Slots.Count <= 0)
        {
            if (inventoryListText != null)
            {
                inventoryListText.text = "No tienes objetos.";
            }

            if (inventoryDescriptionText != null)
            {
                inventoryDescriptionText.text = "";
            }

            if (inventoryHelpText != null)
            {
                inventoryHelpText.text = "I o Escape para cerrar";
            }

            return;
        }

        RefreshItemList();
        RefreshItemDescription();
        RefreshHelpText();
    }

    private void RefreshItemList()
    {
        if (inventoryListText == null)
            return;

        StringBuilder builder = new StringBuilder();

        for (int i = 0; i < PlayerInventory.Instance.Slots.Count; i++)
        {
            InventorySlot slot = PlayerInventory.Instance.Slots[i];

            if (slot.itemData == null)
                continue;

            string marker = i == selectedIndex ? "> " : "  ";

            if (slot.quantity > 1)
            {
                builder.AppendLine(marker + slot.itemData.itemName + " x" + slot.quantity);
            }
            else
            {
                builder.AppendLine(marker + slot.itemData.itemName);
            }
        }

        inventoryListText.text = builder.ToString();
    }

    private void RefreshItemDescription()
    {
        if (inventoryDescriptionText == null)
            return;

        InventorySlot selectedSlot = PlayerInventory.Instance.Slots[selectedIndex];

        if (selectedSlot == null || selectedSlot.itemData == null)
        {
            inventoryDescriptionText.text = "";
            return;
        }

        ItemData item = selectedSlot.itemData;

        inventoryDescriptionText.text =
            item.itemName +
            "\n\n" +
            item.itemDescription;
    }

    private void RefreshHelpText()
    {
        if (inventoryHelpText == null)
            return;

        inventoryHelpText.text = "W / S para seleccionar     I o Escape para cerrar";
    }
}