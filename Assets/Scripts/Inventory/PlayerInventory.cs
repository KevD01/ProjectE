using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance;

    [System.Serializable]
    public class InventorySaveEntry
    {
        public ItemData itemData;
        public int quantity;

        public InventorySaveEntry(ItemData itemData, int quantity)
        {
            this.itemData = itemData;
            this.quantity = quantity;
        }
    }

    [Header("Inventario")]
    [SerializeField] private List<InventorySlot> slots = new List<InventorySlot>();

    public List<InventorySlot> Slots => slots;

    private void Awake()
    {
        Instance = this;
    }

    public void AddItem(ItemData itemData, int amount = 1)
    {
        if (itemData == null)
            return;

        if (amount <= 0)
            return;

        if (itemData.canStack)
        {
            AddStackableItem(itemData, amount);
        }
        else
        {
            AddNonStackableItem(itemData, amount);
        }

        Debug.Log("Objeto agregado: " + itemData.itemName + " x" + amount);
    }

    private void AddStackableItem(ItemData itemData, int amount)
    {
        int remainingAmount = amount;

        foreach (InventorySlot slot in slots)
        {
            if (slot == null || slot.itemData != itemData)
                continue;

            if (slot.quantity >= itemData.maxStack)
                continue;

            int availableSpace = itemData.maxStack - slot.quantity;
            int amountToAdd = Mathf.Min(availableSpace, remainingAmount);

            slot.quantity += amountToAdd;
            remainingAmount -= amountToAdd;

            if (remainingAmount <= 0)
                return;
        }

        while (remainingAmount > 0)
        {
            int amountForNewSlot = Mathf.Min(itemData.maxStack, remainingAmount);

            slots.Add(new InventorySlot(itemData, amountForNewSlot));

            remainingAmount -= amountForNewSlot;
        }
    }

    private void AddNonStackableItem(ItemData itemData, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            slots.Add(new InventorySlot(itemData, 1));
        }
    }

    public bool HasItem(ItemData itemData)
    {
        if (itemData == null)
            return false;

        foreach (InventorySlot slot in slots)
        {
            if (slot == null)
                continue;

            if (slot.itemData == itemData && slot.quantity > 0)
                return true;
        }

        return false;
    }

    public int GetTotalQuantity(ItemData itemData)
    {
        if (itemData == null)
            return 0;

        int total = 0;

        foreach (InventorySlot slot in slots)
        {
            if (slot == null)
                continue;

            if (slot.itemData == itemData)
            {
                total += slot.quantity;
            }
        }

        return total;
    }

    public bool RemoveItem(ItemData itemData, int amount = 1)
    {
        if (itemData == null)
            return false;

        if (amount <= 0)
            return false;

        if (GetTotalQuantity(itemData) < amount)
            return false;

        int remainingAmount = amount;

        for (int i = slots.Count - 1; i >= 0; i--)
        {
            InventorySlot slot = slots[i];

            if (slot == null || slot.itemData != itemData)
                continue;

            int amountToRemove = Mathf.Min(slot.quantity, remainingAmount);

            slot.quantity -= amountToRemove;
            remainingAmount -= amountToRemove;

            if (slot.quantity <= 0)
            {
                slots.RemoveAt(i);
            }

            if (remainingAmount <= 0)
                return true;
        }

        return true;
    }

    public bool RemoveAt(int index, int amount = 1)
    {
        if (index < 0 || index >= slots.Count)
            return false;

        InventorySlot slot = slots[index];

        if (slot == null)
            return false;

        if (amount <= 0)
            return false;

        slot.quantity -= amount;

        if (slot.quantity <= 0)
        {
            slots.RemoveAt(index);
        }

        return true;
    }

    public List<InventorySaveEntry> CaptureInventorySnapshot()
    {
        List<InventorySaveEntry> snapshot = new List<InventorySaveEntry>();

        foreach (InventorySlot slot in slots)
        {
            if (slot == null)
                continue;

            if (slot.itemData == null)
                continue;

            if (slot.quantity <= 0)
                continue;

            snapshot.Add(new InventorySaveEntry(slot.itemData, slot.quantity));
        }

        return snapshot;
    }

    public void RestoreInventorySnapshot(List<InventorySaveEntry> snapshot)
    {
        slots.Clear();

        if (snapshot == null)
            return;

        foreach (InventorySaveEntry entry in snapshot)
        {
            if (entry == null)
                continue;

            if (entry.itemData == null)
                continue;

            if (entry.quantity <= 0)
                continue;

            slots.Add(new InventorySlot(entry.itemData, entry.quantity));
        }

        Debug.Log("Inventario restaurado desde checkpoint.");
    }

    public void ClearInventory()
    {
        slots.Clear();
    }
}