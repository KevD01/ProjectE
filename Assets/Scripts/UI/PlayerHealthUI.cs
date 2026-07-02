using TMPro;
using UnityEngine;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private TMP_Text healthStateText;

    private PlayerHealth playerHealth;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
        }
    }

    private void Update()
    {
        if (playerHealth == null)
            return;

        if (healthStateText != null)
        {
            healthStateText.text = "Estado: " + playerHealth.GetHealthState();
        }
    }
}