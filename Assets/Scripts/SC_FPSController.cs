using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]

public class SC_FPSController : MonoBehaviour
{
    public float walkingSpeed = 7.5f;
    public float runningSpeed = 11.5f;
    public float waterWalkingSpeed = 3.5f; // Slower walking speed in water
    public float waterRunningSpeed = 5.5f; // Slower running speed in water
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public Camera playerCamera;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;

    private float normalWalkingSpeed;
    private float normalRunningSpeed;

    CharacterController characterController;
    Animator animator;  // Reference to the Animator component on the child player model
    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;

    [HideInInspector]
    public bool canMove = true;

    // Threshold for dead zone to prevent small input values from triggering animations
    public float inputDeadZone = 0.1f;

    private bool isJumping = false;  // Track if the player is in the air
    private bool isInWater = false;  // Track if the player is in water

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        // Store normal movement speeds
        normalWalkingSpeed = walkingSpeed;
        normalRunningSpeed = runningSpeed;

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Check if the CharacterController is active and enabled before doing anything
        if (!characterController.enabled)
        {
            return; // Exit Update if the CharacterController is disabled
        }

        // We are grounded, so recalculate move direction based on axes
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        // Get inputs for movement
        float verticalInput = Input.GetAxis("Vertical"); // Forward/Backward
        float horizontalInput = Input.GetAxis("Horizontal"); // Left/Right

        // Apply the dead zone
        if (Mathf.Abs(verticalInput) < inputDeadZone)
        {
            verticalInput = 0; // Treat very small input values as no input
        }

        if (Mathf.Abs(horizontalInput) < inputDeadZone)
        {
            horizontalInput = 0; // Treat very small input values as no input
        }

        // Determine if running forward (only allow running forward)
        bool isRunning = Input.GetKey(KeyCode.LeftShift) && verticalInput > 0;

        // Calculate movement speed based on whether running or walking
        float currentWalkingSpeed = isInWater ? waterWalkingSpeed : normalWalkingSpeed;
        float currentRunningSpeed = isInWater ? waterRunningSpeed : normalRunningSpeed;

        float curSpeedX = canMove ? (isRunning ? currentRunningSpeed : currentWalkingSpeed) * verticalInput : 0;
        float curSpeedY = canMove ? currentWalkingSpeed * horizontalInput : 0; // Always walk when strafing or moving backwards

        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        // Jump logic
        if (Input.GetButton("Jump") && canMove && characterController.isGrounded && !isJumping)
        {
            moveDirection.y = jumpSpeed;
            isJumping = true;
            animator.SetTrigger("Jump"); // Trigger the jump animation
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        // Apply gravity
        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }
        else
        {
            // If grounded and was jumping, reset the jump state
            if (isJumping)
            {
                isJumping = false;
                animator.ResetTrigger("Jump"); // Reset jump trigger when grounded
            }
        }

        // Move the controller
        characterController.Move(moveDirection * Time.deltaTime);

        // Player and Camera rotation
        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }

        // Update Animator Parameters for blend tree movement
        animator.SetFloat("Vertical", verticalInput);   // Send vertical input to blend tree
        animator.SetFloat("Horizontal", horizontalInput); // Send horizontal input to blend tree
        animator.SetBool("isRunning", isRunning);       // Set running animation based on movement

        // Update isGrounded parameter in the Animator
        animator.SetBool("isGrounded", characterController.isGrounded); // Update isGrounded in Animator
    }

    // Detect when the player enters water
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            isInWater = true;
        }
    }

    // Detect when the player exits water
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            isInWater = false;
        }
    }
}
