using UnityEngine;

public class FootstepSoundController : MonoBehaviour
{
    public AudioSource footstepAudioSource;
    public AudioClip concreteSound;
    public AudioClip metalSound;
    public AudioClip waterSound; // Add water footstep sound
    public AudioClip ventSound;
    private CharacterController characterController;

    public float walkStepRate = 0.5f;  // Time between footstep sounds when walking
    public float runStepRate = 0.3f;   // Time between footstep sounds when running
    public float waterStepRate = 1.0f; // Time between footstep sounds in water
    private float stepCooldown;

    public KeyCode runKey = KeyCode.LeftShift; // Key to check if running
    private bool isInWater = false; // Track if the player is in water

    // Head bobbing variables
    public Transform followTarget;  // Assign your Cinemachine Follow Target here
    public float walkBobSpeed = 10f;   // Speed of bobbing when walking
    public float runBobSpeed = 15f;    // Speed of bobbing when running
    public float bobAmount = 0.05f;    // Height of bobbing
    private float defaultYPos;
    private float bobTimer;

    public SC_FPSController playerController; // Reference to the player's FPS controller for crouching state

    // Minimum movement speed required for footstep sound
    public float movementThreshold = 0.2f;

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        // Ensure that playerController is assigned, find it if not manually assigned
        if (playerController == null)
        {
            playerController = FindObjectOfType<SC_FPSController>();
        }

        if (followTarget != null)
        {
            defaultYPos = followTarget.localPosition.y;
        }
        else
        {
            Debug.LogError("Follow target not assigned!");
        }
    }

    void Update()
    {
        // Get player's current velocity magnitude
        float playerSpeed = characterController.velocity.magnitude;

        // Adjust step rate based on whether the player is running or walking
        bool isRunning = Input.GetKey(runKey) && playerSpeed > movementThreshold;

        // Check if the player is crouching via the playerController
        bool isCrouching = playerController != null && playerController.isCrouching;

        float currentStepRate = isInWater ? waterStepRate : (isRunning ? runStepRate : walkStepRate);

        // Only play footsteps if the player is moving faster than the movement threshold and not crouching
        if ((characterController.isGrounded || isInWater) && playerSpeed > movementThreshold && stepCooldown <= 0f && !isCrouching)
        {
            CheckAndPlayFootstep();
            stepCooldown = currentStepRate; // Set cooldown based on walking/running and surface type
        }

        if (stepCooldown > 0)
        {
            stepCooldown -= Time.deltaTime;
        }

        HandleHeadBobbing(isRunning, playerSpeed > movementThreshold && !isCrouching);
    }


    void CheckAndPlayFootstep()
    {
        if (footstepAudioSource == null)
        {
            Debug.LogError("FootstepAudioSource is not assigned!");
            return;
        }

        AudioClip clipToPlay = null;

        // Play water footstep sound if in water, otherwise check surface material
        if (isInWater)
        {
            clipToPlay = waterSound; // Always play water footstep sound in water
        }
        else
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out hit, 2.5f))
            {
                if (hit.collider != null && hit.collider.sharedMaterial != null)
                {
                    switch (hit.collider.sharedMaterial.name)
                    {
                        case "Concrete":
                            clipToPlay = concreteSound;
                            break;
                        case "Metal":
                            clipToPlay = metalSound;
                            break;
                        case "Vent":
                            clipToPlay = ventSound;
                            break;
                        default:
                            Debug.Log("Surface material not recognized.");
                            return; // Exit if the surface is not recognized
                    }
                }
                else
                {
                    Debug.Log("No material found on collider or collider is null.");
                    return;
                }
            }
            else
            {
                Debug.Log("Raycast did not hit any collider.");
                return;
            }
        }

        if (clipToPlay != null)
        {
            footstepAudioSource.clip = clipToPlay;
            footstepAudioSource.Play();
        }
        else
        {
            Debug.LogError("No clip available for the surface.");
        }
    }

    void HandleHeadBobbing(bool isRunning, bool isMoving)
    {
        // Only apply head bobbing if the player is moving
        if (isMoving && (characterController.isGrounded || isInWater))
        {
            // Adjust bob speed based on whether the player is running or walking
            float bobSpeed = isRunning ? runBobSpeed : walkBobSpeed;

            // Calculate the bob amount using a sine wave
            bobTimer += Time.deltaTime * bobSpeed;
            float newY = defaultYPos + Mathf.Sin(bobTimer) * bobAmount;

            // Apply the bobbing effect to the Follow Target's Y position
            followTarget.localPosition = new Vector3(followTarget.localPosition.x, newY, followTarget.localPosition.z);
        }
        else
        {
            // Reset the Follow Target's position when the player stops moving
            bobTimer = 0;
            followTarget.localPosition = new Vector3(followTarget.localPosition.x, defaultYPos, followTarget.localPosition.z);
        }
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
