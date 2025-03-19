using UnityEngine;
using Cinemachine;

public class ComputerInteraction : MonoBehaviour
{
    public CinemachineVirtualCamera playerCamera;    // The player's Cinemachine virtual camera
    public CinemachineVirtualCamera computerCamera;  // The computer's Cinemachine virtual camera
    public GameObject computerUI;                    // Reference to the world-space canvas for the computer UI
    public float interactionDistance = 2.5f;         // Maximum distance to interact with the computer
    public GameObject computerCLIObject;             // Reference to the GameObject that holds the ComputerCLI script
    public GameObject pauseMenu;                     // Reference to the Pause Menu
    public ObjectPicker objectPicker;
    private bool isComputerActive = false;

    private ComputerCLI computerCLI;                 // Reference to the ComputerCLI script
    public bool isUsingComputer = false;             // Track if player is using the computer
    private SC_FPSController playerController;       // Player controller for movement control

    private bool interactionInProgress = false;      // To prevent interaction spamming
    private bool isPlayerInRange = false;            // Track if player is within the trigger range

    public PlayerInteraction PlayerInteractionScript; // Reference to your PlayerInteraction script
    public GameObject interactionUIParent; // Reference to the parent UI object

    public ComputerDialogue dialogueSystem;

    void Start()
    {
        // Get player controller
        playerController = FindObjectOfType<SC_FPSController>(); // Or manually assign in the inspector

        // Find the ComputerCLI script on the specified GameObject
        if (computerCLIObject != null)
        {
            computerCLI = computerCLIObject.GetComponent<ComputerCLI>();
        }

        // Ensure computer UI is always visible
        if (computerUI != null)
        {
            computerUI.SetActive(true);
        }
        else
        {
            Debug.LogError("Computer UI is not assigned!");
        }

        // Ensure computer camera is inactive initially
        if (computerCamera != null)
        {
            computerCamera.Priority = 0;
        }
        else
        {
            Debug.LogError("Computer camera is not assigned!");
        }

        // Disable computer input initially
        if (computerCLI != null)
        {
            computerCLI.DisableInput(); // Disable input when not using the computer
        }
        else
        {
            Debug.LogError("ComputerCLI script not found on the assigned object!");
        }
        // Ensure PlayerInteractionScript is assigned (either from Inspector or dynamically)
        if (PlayerInteractionScript == null)
        {
            PlayerInteractionScript = FindObjectOfType<PlayerInteraction>();
            if (PlayerInteractionScript == null)
            {
                Debug.LogError("PlayerInteractionScript not found!");
            }
        }
        dialogueSystem = GetComponent<ComputerDialogue>();
        if (dialogueSystem == null)
        {
            Debug.LogError("ComputerDialogue component not found on the GameObject!");
        }

    }

    void Update()
    {
        // Prevent any interaction if dialogue is active
        if (dialogueSystem != null && dialogueSystem.IsDialogueActive())
        {
            if (playerController != null)
            {
                playerController.canMove = false; // Disable all movement
                playerController.isCrouching = false; // Force standing position
                playerController.characterController.height = playerController.standingHeight;
                playerController.standingCamera.Priority = 10;
                playerController.crouchingCamera.Priority = 0;
            }
            return;  // Exit early to prevent any further input
        }


        if (isUsingComputer)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(0))
            {
                ExitComputerMode();
            }
        }

        if (Input.GetMouseButtonDown(0) && !interactionInProgress && isPlayerInRange)
        {
            Debug.Log("Player interacted with the computer");
            ToggleComputer();
        }
    }




    void ToggleComputer()
    {
        if (interactionInProgress) return; // Prevent multiple clicks while transitioning

        Debug.Log("Toggling computer interaction");
        interactionInProgress = true;

        isUsingComputer = !isUsingComputer;

        isComputerActive = !isComputerActive;

        if (objectPicker != null)
        {
            objectPicker.isUsingComputer = isComputerActive;
        }

        if (isUsingComputer)
        {
            if (objectPicker != null)
            {
                objectPicker.UnequipCurrentItem(); // Call the unequip function
            }
            // Hide all interaction texts
            PlayerInteractionScript.SetCrosshairDefault();

            Debug.Log("Switching to computer mode");
            // Switch to computer camera
            playerCamera.Priority = 0;       // Deactivate player camera by lowering its priority
            computerCamera.Priority = 10;    // Activate computer camera by increasing its priority

            Cursor.lockState = CursorLockMode.Locked; // Lock cursor for normal gameplay
            Cursor.visible = false;

            if (interactionUIParent != null)
            {
                interactionUIParent.SetActive(false);  // Disable the entire parent object
                Debug.Log("Hiding interaction UI parent");
            }

            // Enable player movement and restore crouching capability
            if (playerController != null)
            {
                playerController.canMove = false; // Disable all movement
                playerController.isCrouching = false; // Force standing position
                playerController.characterController.height = playerController.standingHeight;
                playerController.standingCamera.Priority = 10;
                playerController.crouchingCamera.Priority = 0;
            }

            // Enable computer input after a delay
            if (computerCLI != null)
            {
                computerCLI.EnableInput();
            }
        }
    }

    void ExitComputerMode()
    {
        Debug.Log("Switching back to player mode");
        // Disable input field and reset focus when exiting computer
        if (computerCLI != null)
        {
            computerCLI.DisableInput();
        }

        if (objectPicker != null)
        {
            objectPicker.isUsingComputer = false; // Re-enable inventory keys
        }

        // Switch back to player camera
        playerCamera.Priority = 10;      // Reactivate player camera
        computerCamera.Priority = 0;     // Deactivate computer camera

        Cursor.lockState = CursorLockMode.Locked; // Lock cursor for normal gameplay
        Cursor.visible = false;

        if (interactionUIParent != null)
        {
            interactionUIParent.SetActive(true);  // Enable the entire parent object
            Debug.Log("Hiding interaction UI parent");
        }

        // Re-enable player movement
        if (playerController != null)
        {
            playerController.canMove = true;
            Debug.Log("Player movement enabled");
        }
        // Delay to allow proper interaction before resetting the flag
        Invoke("ResetInteraction", 0.2f); // Small delay to prevent multiple interactions in rapid succession
    }

        private void ResetInteraction()
    {
        Debug.Log("Resetting interaction");
        interactionInProgress = false;
    }

    // When the player enters the interaction trigger range
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            Debug.Log("Player entered interaction range");
        }
    }

    // When the player exits the interaction trigger range
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            Debug.Log("Player exited interaction range");
        }
    }
}
