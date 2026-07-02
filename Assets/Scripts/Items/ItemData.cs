using UnityEngine;

public enum ItemType
{
    Key,
    Ammo,
    Healing,
    Puzzle,
    Misc
}

[CreateAssetMenu(fileName = "NewItemData", menuName = "Ecos del Silencio/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Identificación")]
    public string itemId = "item_001";

    [Header("Información")]
    public string itemName = "Objeto";
    
    [TextArea(2, 6)]
    public string itemDescription = "Descripción del objeto.";

    [Header("Tipo")]
    public ItemType itemType = ItemType.Misc;

    [Header("Inventario")]
    public bool canStack = false;
    public int maxStack = 1;
}