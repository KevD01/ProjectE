using System.Collections;
using UnityEngine;

public class DoorTransition : MonoBehaviour
{
    [Header("Destino")]
    [SerializeField] private Transform destinationSpawnPoint;
    [SerializeField] private Transform destinationCameraPoint;

    [Header("Transición")]
    [SerializeField] private float fadeOutTime = 0.4f;
    [SerializeField] private float fadeInTime = 0.4f;
    [SerializeField] private float waitInBlackTime = 0.2f;

    [Header("Interacción")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float interactionDistance = 1.8f;
    [SerializeField] private string interactionMessage = "Presiona E para usar la puerta";

    private GameObject playerObject;
    private bool playerInside;
    private bool isTransitioning;
    private bool promptIsVisible;

    private void Update()
    {
        UpdatePrompt();

        if (isTransitioning)
            return;

        if (playerObject == null)
            return;

        if (!playerInside)
            return;

        if (!IsPlayerCloseEnough())
            return;

        if (Input.GetKeyDown(interactKey))
        {
            StartCoroutine(UseDoor());
        }
    }

    private void UpdatePrompt()
    {
        bool shouldShowPrompt =
            !isTransitioning &&
            playerObject != null &&
            playerInside &&
            IsPlayerCloseEnough();

        if (shouldShowPrompt)
        {
            if (!promptIsVisible)
            {
                InteractionPromptUI.Instance?.Show(interactionMessage);
                promptIsVisible = true;
            }
        }
        else
        {
            if (promptIsVisible)
            {
                InteractionPromptUI.Instance?.Hide();
                promptIsVisible = false;
            }
        }
    }

    private bool IsPlayerCloseEnough()
    {
        if (playerObject == null)
            return false;

        float distance = Vector3.Distance(transform.position, playerObject.transform.position);
        return distance <= interactionDistance;
    }

    private IEnumerator UseDoor()
    {
        if (playerObject == null || destinationSpawnPoint == null)
            yield break;

        isTransitioning = true;

        InteractionPromptUI.Instance?.Hide();
        promptIsVisible = false;

        GameObject currentPlayer = playerObject;

        playerInside = false;
        playerObject = null;

        PlayerTankController playerMovement = currentPlayer.GetComponent<PlayerTankController>();

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
            playerMovement.ResetVerticalVelocity();
        }

        if (ScreenFader.Instance != null)
        {
            yield return ScreenFader.Instance.FadeOut(fadeOutTime);
        }

        yield return new WaitForSeconds(waitInBlackTime);

        if (playerMovement != null)
        {
            playerMovement.TeleportTo(destinationSpawnPoint);
        }
        else
        {
            currentPlayer.transform.SetPositionAndRotation(
                destinationSpawnPoint.position,
                destinationSpawnPoint.rotation
            );

            Physics.SyncTransforms();
        }

        if (FixedCameraManager.Instance != null)
        {
            FixedCameraManager.Instance.ClearActiveZones();

            if (destinationCameraPoint != null)
            {
                FixedCameraManager.Instance.ChangeCamera(destinationCameraPoint);
            }
        }

        yield return null;

        if (ScreenFader.Instance != null)
        {
            yield return ScreenFader.Instance.FadeIn(fadeInTime);
        }

        if (playerMovement != null)
        {
            playerMovement.ResetVerticalVelocity();
            playerMovement.enabled = true;
        }

        isTransitioning = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerObject = other.gameObject;
        playerInside = true;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerObject = other.gameObject;
        playerInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (playerObject == other.gameObject)
        {
            playerInside = false;
            playerObject = null;
        }

        InteractionPromptUI.Instance?.Hide();
        promptIsVisible = false;
    }
}