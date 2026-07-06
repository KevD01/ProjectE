using System.Collections;
using UnityEngine;

public class EndingTrigger : MonoBehaviour
{
    [Header("Interacción")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float interactionDistance = 2.2f;
    [SerializeField] private string interactionMessage = "Oprime E para salir";

    [Header("Final")]
    [SerializeField] private float waitBeforeEnding = 0.4f;

    [Header("Audio")]
    [SerializeField] private AudioClip endingSound;
    [SerializeField] private float endingVolume = 1f;

    private GameObject playerObject;
    private bool promptIsVisible;
    private bool endingStarted;

    private void Start()
    {
        playerObject = GameObject.FindGameObjectWithTag("Player");
    }

    private void Update()
    {
        if (endingStarted)
            return;

        if (IsGameplayPaused())
        {
            HidePrompt();
            return;
        }

        if (playerObject == null)
        {
            playerObject = GameObject.FindGameObjectWithTag("Player");
            return;
        }

        if (IsPlayerCloseEnough())
        {
            ShowPrompt();

            if (Input.GetKeyDown(interactKey))
            {
                StartCoroutine(EndingRoutine());
            }
        }
        else
        {
            HidePrompt();
        }
    }

    private bool IsPlayerCloseEnough()
    {
        if (playerObject == null)
            return false;

        float distance = Vector3.Distance(transform.position, playerObject.transform.position);
        return distance <= interactionDistance;
    }

    private void ShowPrompt()
    {
        if (promptIsVisible)
            return;

        InteractionPromptUI.Instance?.Show(interactionMessage);
        promptIsVisible = true;
    }

    private void HidePrompt()
    {
        if (!promptIsVisible)
            return;

        InteractionPromptUI.Instance?.Hide();
        promptIsVisible = false;
    }

    private IEnumerator EndingRoutine()
    {
        endingStarted = true;

        HidePrompt();

        GameAudioManager.Instance?.PlaySFXNoPitch(endingSound, endingVolume);

        DisablePlayer();

        if (ScreenFader.Instance != null)
        {
            yield return ScreenFader.Instance.FadeOut(waitBeforeEnding);
        }
        else
        {
            yield return new WaitForSeconds(waitBeforeEnding);
        }

        if (EndingUI.Instance != null)
        {
            EndingUI.Instance.ShowEnding();
        }
    }

    private void DisablePlayer()
    {
        if (playerObject == null)
            return;

        PlayerTankController movement = playerObject.GetComponent<PlayerTankController>();

        if (movement != null)
        {
            movement.enabled = false;
        }

        PlayerWeaponController weapon = playerObject.GetComponent<PlayerWeaponController>();

        if (weapon != null)
        {
            weapon.enabled = false;
        }

        CharacterController characterController = playerObject.GetComponent<CharacterController>();

        if (characterController != null)
        {
            characterController.enabled = false;
        }
    }

    private bool IsGameplayPaused()
    {
        if (GameOverUI.Instance != null && GameOverUI.Instance.IsGameOver)
            return true;

        if (EndingUI.Instance != null && EndingUI.Instance.EndingActive)
            return true;

        if (NoteUI.Instance != null && NoteUI.Instance.IsOpen)
            return true;

        if (NoteArchiveUI.Instance != null && NoteArchiveUI.Instance.IsOpen)
            return true;

        if (InventoryUI.Instance != null && InventoryUI.Instance.IsOpen)
            return true;

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}