using TMPro;
using UnityEngine;

public class AmmoUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private TMP_Text ammoText;
    [SerializeField] private PlayerWeaponController weaponController;

    private void Start()
    {
        if (weaponController == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                weaponController = player.GetComponent<PlayerWeaponController>();
            }
        }
    }

    private void Update()
    {
        if (ammoText == null)
            return;

        if (weaponController == null)
        {
            ammoText.text = "Munición: --";
            return;
        }

        int clip = weaponController.CurrentAmmoInClip;
        int maxClip = weaponController.MaxAmmoInClip;
        int reserve = weaponController.GetReserveAmmo();

        if (!weaponController.HasRequiredWeaponEquipped())
        {
            ammoText.text = "Pistola: no equipada\nReserva: " + reserve;
            return;
        }

        if (weaponController.IsReloading)
        {
            ammoText.text = "Recargando...\nReserva: " + reserve;
            return;
        }

        ammoText.text =
            "Pistola: " + clip + "/" + maxClip +
            "\nReserva: " + reserve;
    }
}