[System.Serializable]
public class InventorySlot
{
    public ItemData itemData;
    public int quantity;

    public InventorySlot(ItemData itemData, int quantity)
    {
        this.itemData = itemData;
        this.quantity = quantity;
    }
}