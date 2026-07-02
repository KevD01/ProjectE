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

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        MovePlayer();
    }

    private void MovePlayer()
    {
        float moveInput = Input.GetAxisRaw("Vertical");
        float turnInput = Input.GetAxisRaw("Horizontal");

        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        // Giro tipo tanque: A y D giran al personaje sobre su propio eje
        transform.Rotate(Vector3.up * turnInput * rotationSpeed * Time.deltaTime);

        // Movimiento hacia donde mira el personaje
        Vector3 movement = transform.forward * moveInput * currentSpeed;

        // Gravedad
        if (characterController.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;
        movement.y = verticalVelocity;

        characterController.Move(movement * Time.deltaTime);
    }
}