using System.Collections;
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
    [SerializeField] private KeyCode useItemKey = KeyCode.E;

    private PlayerTankController playerMovement;
    private PlayerHealth playerHealth;
    private PlayerEquipment playerEquipment;
    private bool isOpen;
    private int selectedIndex;
    private bool showingTemporaryMessage;

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
            playerHealth = player.GetComponent<PlayerHealth>();
            playerEquipment = player.GetComponent<PlayerEquipment>();
        }
    }

    private void Update()
    {
        if (PauseMenuUI.Instance != null && PauseMenuUI.Instance.IsPaused)
            return;

        if (GameOverUI.Instance != null && GameOverUI.Instance.IsGameOver)
            return;

        if (EndingUI.Instance != null && EndingUI.Instance.EndingActive)
            return;

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

        if (Input.GetKeyDown(useItemKey) || Input.GetKeyDown(KeyCode.Return))
        {
            UseSelectedItem();
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

    public void ForceClose()
    {
        isOpen = false;
        showingTemporaryMessage = false;

        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
    }

    private void MoveSelection(int direction)
    {
        if (showingTemporaryMessage)
            return;

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

    private void UseSelectedItem()
    {
        if (showingTemporaryMessage)
            return;

        if (PlayerInventory.Instance == null)
            return;

        if (PlayerInventory.Instance.Slots.Count <= 0)
            return;

        if (selectedIndex < 0 || selectedIndex >= PlayerInventory.Instance.Slots.Count)
            return;

        InventorySlot selectedSlot = PlayerInventory.Instance.Slots[selectedIndex];

        if (selectedSlot == null || selectedSlot.itemData == null)
            return;

        ItemData item = selectedSlot.itemData;

        if (item.itemType == ItemType.Healing)
        {
            UseHealingItem(item);
            return;
        }

        if (item.itemType == ItemType.Weapon)
        {
            EquipWeapon(item);
            return;
        }

        StartCoroutine(ShowInventoryMessage("No puedes usar eso ahora."));
    }

    private void UseHealingItem(ItemData item)
    {
        if (playerHealth == null)
        {
            StartCoroutine(ShowInventoryMessage("No se encontró la vida del jugador."));
            return;
        }

        if (playerHealth.IsDead)
        {
            StartCoroutine(ShowInventoryMessage("No puedes usar eso ahora."));
            return;
        }

        if (playerHealth.IsFullHealth())
        {
            StartCoroutine(ShowInventoryMessage("Tu vida ya está al máximo."));
            return;
        }

        playerHealth.Heal(item.healingAmount);

        PlayerInventory.Instance.RemoveAt(selectedIndex, 1);

        selectedIndex = Mathf.Clamp(selectedIndex, 0, GetLastValidIndex());

        RefreshInventory();
    }

    private void EquipWeapon(ItemData item)
    {
        if (playerEquipment == null)
        {
            StartCoroutine(ShowInventoryMessage("No se encontró el equipo del jugador."));
            return;
        }

        playerEquipment.EquipWeapon(item);

        StartCoroutine(ShowInventoryMessage("Equipaste: " + item.itemName));
    }

    private IEnumerator ShowInventoryMessage(string message)
    {
        showingTemporaryMessage = true;

        if (inventoryDescriptionText != null)
        {
            inventoryDescriptionText.text = message;
        }

        yield return new WaitForSeconds(1.2f);

        showingTemporaryMessage = false;

        RefreshInventory();
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
            string equippedMarker = "";

            if (slot.itemData.itemType == ItemType.Weapon &&
                playerEquipment != null &&
                playerEquipment.HasEquippedWeapon(slot.itemData))
            {
                equippedMarker = " [Equipada]";
            }

            if (slot.quantity > 1)
            {
                builder.AppendLine(marker + slot.itemData.itemName + " x" + slot.quantity + equippedMarker);
            }
            else
            {
                builder.AppendLine(marker + slot.itemData.itemName + equippedMarker);
            }
        }

        inventoryListText.text = builder.ToString();
    }

    private void RefreshItemDescription()
    {
        if (inventoryDescriptionText == null)
            return;

        if (PlayerInventory.Instance == null || PlayerInventory.Instance.Slots.Count <= 0)
        {
            inventoryDescriptionText.text = "";
            return;
        }

        selectedIndex = Mathf.Clamp(selectedIndex, 0, PlayerInventory.Instance.Slots.Count - 1);

        InventorySlot selectedSlot = PlayerInventory.Instance.Slots[selectedIndex];

        if (selectedSlot == null || selectedSlot.itemData == null)
        {
            inventoryDescriptionText.text = "";
            return;
        }

        ItemData item = selectedSlot.itemData;

        string extraInfo = "";

        if (item.itemType == ItemType.Weapon &&
            playerEquipment != null &&
            playerEquipment.HasEquippedWeapon(item))
        {
            extraInfo = "\n\nEstado: Equipada";
        }

        inventoryDescriptionText.text =
            item.itemName +
            "\n\n" +
            item.itemDescription +
            extraInfo;
    }

    private void RefreshHelpText()
    {
        if (inventoryHelpText == null)
            return;

        inventoryHelpText.text = "W / S seleccionar     E usar/equipar     I o Escape cerrar";
    }
}