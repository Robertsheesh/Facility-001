using Cinemachine;
using UnityEngine;

public class SC_FPSController : MonoBehaviour
{
    public float walkingSpeed = 7.5f;
    public float runningSpeed = 11.5f;
    public float noclipSpeed = 15.0f; // Speed when noclip is active
    public float crouchSpeed = 3.5f;
    public float crouchHeight = 1.0f;
    public float standingHeight = 2.0f;
    public float crouchCameraHeight = 0.5f;
    public float standingCameraHeight = 1.5f;
    public float waterWalkingSpeed = 3.5f;
    public float waterRunningSpeed = 5.5f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;

    public Camera playerCamera;
    public CinemachineVirtualCamera standingCamera;
    public CinemachineVirtualCamera crouchingCamera;

    private CinemachineBasicMultiChannelPerlin noise;

    public CharacterController characterController;
    private Animator animator;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;

    [HideInInspector]
    public bool canMove = true;

    private bool isJumping = false;
    public bool isCrouching = false;
    private bool isInWater = false;
    private bool isNoclip = false; // Noclip mode toggle

    public float walkingBobFrequency = 1.5f;
    public float runningBobFrequency = 2.5f;
    public float crouchBobFrequency = 1.0f;
    public float bobAmplitude = 0.2f;

    private float normalWalkingSpeed;
    private float normalRunningSpeed;
    private float airMoveSpeed;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        normalWalkingSpeed = walkingSpeed;
        normalRunningSpeed = runningSpeed;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (crouchingCamera != null)
        {
            crouchingCamera.Priority = 0;
            crouchingCamera.enabled= false;
        }

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
        // Toggle noclip mode with Shift + .
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) // Check if either Shift key is held
        {
            if (Input.GetKeyDown(KeyCode.Period)) // Check if the . key is pressed
            {
                ToggleNoclip();
            }
        }

        // Handle movement based on whether noclip is active or not
        if (isNoclip)
        {
            HandleNoclipMovement();
        }
        else
        {
            HandleNormalMovement();
        }
    }

    void ToggleNoclip()
    {
        isNoclip = !isNoclip;
        characterController.enabled = !isNoclip; // Disable CharacterController during noclip
        moveDirection = Vector3.zero; // Reset movement direction
    }

    void HandleNormalMovement()
    {
        if (!characterController.enabled) return;

        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        float verticalInput = Input.GetAxis("Vertical");
        float horizontalInput = Input.GetAxis("Horizontal");

        bool isRunning = Input.GetKey(KeyCode.LeftShift) && verticalInput > 0 && !isCrouching;
        bool isCrouchingKeyHeld = Input.GetKey(KeyCode.LeftControl);

        HandleCrouch(isCrouchingKeyHeld);

        float currentWalkingSpeed = isCrouching ? crouchSpeed : (isInWater ? waterWalkingSpeed : normalWalkingSpeed);
        float currentRunningSpeed = isInWater ? waterRunningSpeed : normalRunningSpeed;

        if (characterController.isGrounded)
        {
            airMoveSpeed = isRunning ? currentRunningSpeed : currentWalkingSpeed;
        }

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
            HandleLookRotation();

            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }

        animator.SetFloat("Vertical", verticalInput);
        animator.SetFloat("Horizontal", horizontalInput);
        animator.SetBool("isRunning", isRunning);
        animator.SetBool("isGrounded", characterController.isGrounded);
        animator.SetBool("isCrouching", isCrouching);

        if (noise != null)
        {
            if (verticalInput != 0 || horizontalInput != 0)
            {
                ApplyHeadBob(isRunning, isCrouching);
            }
            else
            {
                noise.m_AmplitudeGain = Mathf.Lerp(noise.m_AmplitudeGain, 0f, Time.deltaTime * 10f);
                noise.m_FrequencyGain = Mathf.Lerp(noise.m_FrequencyGain, 0f, Time.deltaTime * 10f);
            }
        }
    }

    void HandleNoclipMovement()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        Vector3 up = transform.TransformDirection(Vector3.up);

        float verticalInput = Input.GetAxis("Vertical");
        float horizontalInput = Input.GetAxis("Horizontal");
        float ascendInput = 0;

        if (Input.GetKey(KeyCode.Space))
        {
            ascendInput = 1; // Ascend
        }
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            ascendInput = -1; // Descend
        }

        moveDirection = (forward * verticalInput + right * horizontalInput + up * ascendInput).normalized;
        moveDirection *= noclipSpeed;

        transform.position += moveDirection * Time.deltaTime;

        HandleLookRotation();
    }

    void HandleCrouch(bool crouchKeyHeld)
    {
        if (!canMove) return; // Prevent crouching while using the computer

        if (crouchKeyHeld && characterController.isGrounded)
        {
            crouchingCamera.enabled = true;
            characterController.height = crouchHeight;
            crouchingCamera.Priority = 10;
            standingCamera.Priority = 0;
            isCrouching = true;
            noise = crouchingCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }
        else if (!crouchKeyHeld && isCrouching)
        {
            crouchingCamera.enabled = false;
            characterController.height = standingHeight;
            standingCamera.Priority = 10;
            crouchingCamera.Priority = 0;
            isCrouching = false;
            noise = standingCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }

        SyncCameraRotation();
    }

    void HandleLookRotation()
    {
        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);

        if (isCrouching)
        {
            crouchingCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        }
        else
        {
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        }
    }

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
