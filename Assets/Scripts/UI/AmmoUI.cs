using TMPro;
using UnityEngine;

public class AmmoUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private TMP_Text ammoText;
    [SerializeField] private ItemData ammoItem;

    private void Update()
    {
        if (ammoText == null || ammoItem == null)
            return;

        int ammoAmount = 0;

        if (PlayerInventory.Instance != null)
        {
            ammoAmount = PlayerInventory.Instance.GetTotalQuantity(ammoItem);
        }

        ammoText.text = "Balas: " + ammoAmount;
    }
}