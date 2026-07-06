using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerTankController : MonoBehaviour
{
    [Header("Movimiento")]
    public float walkSpeed = 2f;
    public float runSpeed = 3.4f;
    public float rotationSpeed = 120f;

    [Header("Gravedad")]
    public float gravity = -20f;

    private CharacterController characterController;
    private float verticalVelocity;
    private bool movementLocked;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (PauseMenuUI.Instance != null && PauseMenuUI.Instance.IsPaused)
            return;

        MovePlayer();
    }

    private void MovePlayer()
    {
        float moveInput = Input.GetAxisRaw("Vertical");
        float turnInput = Input.GetAxisRaw("Horizontal");

        if (movementLocked)
        {
            moveInput = 0f;
        }

        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        transform.Rotate(Vector3.up * turnInput * rotationSpeed * Time.deltaTime);

        Vector3 movement = transform.forward * moveInput * currentSpeed;

        if (characterController.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;
        movement.y = verticalVelocity;

        characterController.Move(movement * Time.deltaTime);
    }

    public void SetMovementLocked(bool locked)
    {
        movementLocked = locked;
    }

    public void ResetVerticalVelocity()
    {
        verticalVelocity = -2f;
    }

    public void TeleportTo(Transform destination)
    {
        if (destination == null)
            return;

        TeleportToPosition(destination.position, destination.rotation);
    }

    public void TeleportToPosition(Vector3 position, Quaternion rotation)
    {
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }

        bool controllerWasEnabled = characterController.enabled;

        if (controllerWasEnabled)
        {
            characterController.enabled = false;
        }

        transform.SetPositionAndRotation(position, rotation);

        ResetVerticalVelocity();

        if (controllerWasEnabled)
        {
            characterController.enabled = true;
        }

        Physics.SyncTransforms();

        if (characterController.enabled)
        {
            characterController.Move(Vector3.zero);
        }
    }

    private void OnDisable()
    {
        movementLocked = false;
    }
}