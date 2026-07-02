using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance;

    [Header("Configuración")]
    [SerializeField] private int maxSlots = 8;

    private readonly List<InventorySlot> slots = new List<InventorySlot>();

    public IReadOnlyList<InventorySlot> Slots => slots;
    public int MaxSlots => maxSlots;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public bool AddItem(ItemData itemData, int amount)
    {
        if (itemData == null)
            return false;

        if (amount <= 0)
            return false;

        if (itemData.canStack)
        {
            foreach (InventorySlot slot in slots)
            {
                if (slot.itemData == itemData && slot.quantity < itemData.maxStack)
                {
                    int availableSpace = itemData.maxStack - slot.quantity;
                    int amountToAdd = Mathf.Min(availableSpace, amount);

                    slot.quantity += amountToAdd;
                    amount -= amountToAdd;

                    if (amount <= 0)
                    {
                        Debug.Log("Objeto agregado: " + itemData.itemName);
                        return true;
                    }
                }
            }
        }

        while (amount > 0)
        {
            if (slots.Count >= maxSlots)
            {
                Debug.Log("Inventario lleno.");
                return false;
            }

            int amountForNewSlot = itemData.canStack
                ? Mathf.Min(amount, itemData.maxStack)
                : 1;

            slots.Add(new InventorySlot(itemData, amountForNewSlot));
            amount -= amountForNewSlot;

            if (!itemData.canStack)
                break;
        }

        Debug.Log("Objeto agregado: " + itemData.itemName);
        return true;
    }

    public bool HasItem(ItemData itemData)
    {
        if (itemData == null)
            return false;

        foreach (InventorySlot slot in slots)
        {
            if (slot.itemData == itemData)
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
            if (slot.itemData == itemData)
            {
                total += slot.quantity;
            }
        }

        return total;
    }

    public bool RemoveItem(ItemData itemData, int amount)
    {
        if (itemData == null)
            return false;

        if (amount <= 0)
            return false;

        if (GetTotalQuantity(itemData) < amount)
            return false;

        for (int i = slots.Count - 1; i >= 0; i--)
        {
            InventorySlot slot = slots[i];

            if (slot.itemData != itemData)
                continue;

            int amountToRemove = Mathf.Min(slot.quantity, amount);
            slot.quantity -= amountToRemove;
            amount -= amountToRemove;

            if (slot.quantity <= 0)
            {
                slots.RemoveAt(i);
            }

            if (amount <= 0)
                return true;
        }

        return true;
    }

    public bool RemoveAt(int index, int amount)
    {
        if (index < 0 || index >= slots.Count)
            return false;

        if (amount <= 0)
            return false;

        InventorySlot slot = slots[index];

        slot.quantity -= amount;

        if (slot.quantity <= 0)
        {
            slots.RemoveAt(index);
        }

        return true;
    }
}