using Cinemachine;
using UnityEngine;

public class SC_FPSController : MonoBehaviour
{
    public float walkingSpeed = 7.5f;
    public float runningSpeed = 11.5f;
    public float crouchSpeed = 3.5f; // Speed when crouching
    public float crouchHeight = 1.0f; // Height of the CharacterController when crouching
    public float standingHeight = 2.0f; // Normal height of the CharacterController
    public float crouchCameraHeight = 0.5f; // Height for the camera when crouching
    public float standingCameraHeight = 1.5f; // Normal height of the camera
    public float waterWalkingSpeed = 3.5f;
    public float waterRunningSpeed = 5.5f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;

    public Camera playerCamera;
    public CinemachineVirtualCamera standingCamera; // Cinemachine camera for standing
    public CinemachineVirtualCamera crouchingCamera; // Cinemachine camera for crouching

    private CinemachineBasicMultiChannelPerlin noise; // For camera head bobbing (reference to the active camera's noise)

    private float normalWalkingSpeed;
    private float normalRunningSpeed;
    private float airMoveSpeed;

    private CharacterController characterController;
    private Animator animator;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0; // Vertical rotation (pitch)

    [HideInInspector]
    public bool canMove = true;

    private bool isJumping = false;
    public bool isCrouching = false;
    private bool isInWater = false;

    public float walkingBobFrequency = 1.5f;
    public float runningBobFrequency = 2.5f;
    public float crouchBobFrequency = 1.0f; // Less intense bobbing for crouching
    public float bobAmplitude = 0.2f; // Head bob amplitude

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        normalWalkingSpeed = walkingSpeed;
        normalRunningSpeed = runningSpeed;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Ensure crouch camera is initially inactive
        if (crouchingCamera != null)
        {
            crouchingCamera.Priority = 0; // Lower priority means it's inactive initially
        }

        // Set initial noise from standing camera
        if (standingCamera != null)
        {
            noise = standingCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }
        else
        {
            Debug.LogError("Standing camera is not assigned.");
        }
    }

    void Update()
    {
        if (!characterController.enabled) return;

        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        float verticalInput = Input.GetAxis("Vertical");
        float horizontalInput = Input.GetAxis("Horizontal");

        bool isRunning = Input.GetKey(KeyCode.LeftShift) && verticalInput > 0 && !isCrouching;

        // Check if the crouch key (LeftControl) is being held down
        bool isCrouchingKeyHeld = Input.GetKey(KeyCode.LeftControl);

        // Handle crouch state
        HandleCrouch(isCrouchingKeyHeld);

        float currentWalkingSpeed = isCrouching ? crouchSpeed : (isInWater ? waterWalkingSpeed : normalWalkingSpeed);
        float currentRunningSpeed = isInWater ? waterRunningSpeed : normalRunningSpeed;

        // Calculate movement speed based on grounded state
        if (characterController.isGrounded)
        {
            // Update speed based on running or walking
            airMoveSpeed = isRunning ? currentRunningSpeed : currentWalkingSpeed; // Store the speed before jumping
        }

        // If grounded, calculate movement normally
        float curSpeedX = canMove ? airMoveSpeed * verticalInput : 0;
        float curSpeedY = canMove ? airMoveSpeed * horizontalInput : 0;

        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (Input.GetButton("Jump") && canMove && characterController.isGrounded && !isJumping && !isCrouching)
        {
            moveDirection.y = jumpSpeed;
            isJumping = true;
            animator.SetTrigger("Jump");
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }
        else
        {
            if (isJumping)
            {
                isJumping = false;
                animator.ResetTrigger("Jump");
            }
        }

        characterController.Move(moveDirection * Time.deltaTime);

        if (canMove)
        {
            // Update vertical (pitch) and horizontal rotation
            HandleLookRotation();

            // Rotate the player model
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }

        // Update Animator Parameters for movement
        animator.SetFloat("Vertical", verticalInput);
        animator.SetFloat("Horizontal", horizontalInput);
        animator.SetBool("isRunning", isRunning);
        animator.SetBool("isGrounded", characterController.isGrounded);
        animator.SetBool("isCrouching", isCrouching);

        // Apply camera head bobbing based on movement state
        if (noise != null)
        {
            if (verticalInput != 0 || horizontalInput != 0)
            {
                ApplyHeadBob(isRunning, isCrouching);
            }
            else
            {
                // Reset noise when not moving
                noise.m_AmplitudeGain = Mathf.Lerp(noise.m_AmplitudeGain, 0f, Time.deltaTime * 10f);
                noise.m_FrequencyGain = Mathf.Lerp(noise.m_FrequencyGain, 0f, Time.deltaTime * 10f);
            }
        }
    }

    // Handles crouching and modifies the player's height and camera priority accordingly
    void HandleCrouch(bool crouchKeyHeld)
    {
        if (crouchKeyHeld && characterController.isGrounded)
        {
            // Start crouching
            characterController.height = crouchHeight; // Adjust CharacterController height
            crouchingCamera.Priority = 10; // Activate crouching camera
            standingCamera.Priority = 0; // Deactivate standing camera
            isCrouching = true;

            // Update noise reference to crouching camera's noise component
            noise = crouchingCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }
        else if (!crouchKeyHeld && isCrouching)
        {
            // Stand up
            characterController.height = standingHeight; // Adjust CharacterController height
            standingCamera.Priority = 10; // Activate standing camera
            crouchingCamera.Priority = 0; // Deactivate crouching camera
            isCrouching = false;

            // Update noise reference to standing camera's noise component
            noise = standingCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }

        // Sync vertical rotation (pitch) between cameras
        SyncCameraRotation();
    }

    // Handles looking up and down (vertical rotation)
    void HandleLookRotation()
    {
        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);

        // Apply pitch (vertical rotation) to the current active camera
        if (isCrouching)
        {
            crouchingCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        }
        else
        {
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        }
    }

    // Sync the vertical rotation between cameras when switching
    void SyncCameraRotation()
    {
        if (isCrouching)
        {
            crouchingCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        }
        else
        {
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        }
    }

    void ApplyHeadBob(bool isRunning, bool isCrouching)
    {
        float targetAmplitude = bobAmplitude;
        float targetFrequency = isRunning ? runningBobFrequency : (isCrouching ? crouchBobFrequency : walkingBobFrequency);

        noise.m_AmplitudeGain = Mathf.Lerp(noise.m_AmplitudeGain, targetAmplitude, Time.deltaTime * 10f);
        noise.m_FrequencyGain = Mathf.Lerp(noise.m_FrequencyGain, targetFrequency, Time.deltaTime * 10f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            isInWater = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            isInWater = false;
        }
    }
}
