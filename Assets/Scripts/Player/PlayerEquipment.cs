using System;
using UnityEngine;

[Serializable]
public class WeaponVisualEntry
{
    public ItemData weaponItem;
    public GameObject weaponVisual;
}

public class PlayerEquipment : MonoBehaviour
{
    public static PlayerEquipment Instance;

    [Header("Arma equipada")]
    [SerializeField] private ItemData equippedWeapon;

    [Header("Visuales de armas")]
    [SerializeField] private WeaponVisualEntry[] weaponVisuals;

    public ItemData EquippedWeapon => equippedWeapon;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        HideAllWeaponVisuals();
        RefreshWeaponVisuals();
    }

    public void EquipWeapon(ItemData weaponItem)
    {
        if (weaponItem == null)
            return;

        if (weaponItem.itemType != ItemType.Weapon)
        {
            Debug.LogWarning(weaponItem.itemName + " no es un arma.");
            return;
        }

        equippedWeapon = weaponItem;

        RefreshWeaponVisuals();

        Debug.Log("Arma equipada: " + equippedWeapon.itemName);
    }

    public bool HasEquippedWeapon(ItemData weaponItem)
    {
        if (weaponItem == null)
            return false;

        return equippedWeapon == weaponItem;
    }

    public string GetEquippedWeaponName()
    {
        if (equippedWeapon == null)
            return "Ninguna";

        return equippedWeapon.itemName;
    }

    private void RefreshWeaponVisuals()
    {
        HideAllWeaponVisuals();

        if (equippedWeapon == null)
            return;

        foreach (WeaponVisualEntry entry in weaponVisuals)
        {
            if (entry == null)
                continue;

            if (entry.weaponItem == null || entry.weaponVisual == null)
                continue;

            if (entry.weaponItem == equippedWeapon)
            {
                entry.weaponVisual.SetActive(true);
                return;
            }
        }
    }

    private void HideAllWeaponVisuals()
    {
        if (weaponVisuals == null)
            return;

        foreach (WeaponVisualEntry entry in weaponVisuals)
        {
            if (entry == null)
                continue;

            if (entry.weaponVisual != null)
            {
                entry.weaponVisual.SetActive(false);
            }
        }
    }
}