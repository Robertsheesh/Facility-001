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

    private ComputerCLI computerCLI;                 // Reference to the ComputerCLI script
    public bool isUsingComputer = false;             // Track if player is using the computer
    private SC_FPSController playerController;       // Player controller for movement control

    private bool interactionInProgress = false;      // To prevent interaction spamming
    private bool isPlayerInRange = false;            // Track if player is within the trigger range

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
    }

    void Update()
    {
        // Allow interaction only when the player is inside the trigger area and presses the left mouse button
        if (Input.GetMouseButtonDown(0) && !interactionInProgress && isPlayerInRange)
        {
            Debug.Log("Player interacted with the computer");
            ToggleComputer();
        }

        // Automatically exit the computer view when Escape is pressed for the pause menu
        if (Input.GetKeyDown(KeyCode.Escape) && isUsingComputer)
        {
            Debug.Log("Switching back to player mode");
            // Disable input field and reset focus when exiting computer
            if (computerCLI != null)
            {
                computerCLI.DisableInput();
            }

            // Switch back to player camera
            playerCamera.Priority = 10;      // Reactivate player camera
            computerCamera.Priority = 0;     // Deactivate computer camera

            Cursor.lockState = CursorLockMode.Locked; // Lock cursor for normal gameplay
            Cursor.visible = false;

            // Re-enable player movement
            if (playerController != null)
            {
                playerController.canMove = true;
                Debug.Log("Player movement enabled");
            }
        }
    }

    void ToggleComputer()
    {
        if (interactionInProgress) return; // Prevent multiple clicks while transitioning

        Debug.Log("Toggling computer interaction");
        interactionInProgress = true;

        isUsingComputer = !isUsingComputer;

        if (isUsingComputer)
        {
            Debug.Log("Switching to computer mode");
            // Switch to computer camera
            playerCamera.Priority = 0;       // Deactivate player camera by lowering its priority
            computerCamera.Priority = 10;    // Activate computer camera by increasing its priority

            Cursor.lockState = CursorLockMode.Locked; // Lock cursor for normal gameplay
            Cursor.visible = false;

            // Disable player movement
            if (playerController != null)
            {
                playerController.canMove = false;
                Debug.Log("Player movement disabled");
            }

            // Enable computer input after a delay
            if (computerCLI != null)
            {
                computerCLI.EnableInput();
            }
        }
        else
        {
            Debug.Log("Switching back to player mode");
            // Disable input field and reset focus when exiting computer
            if (computerCLI != null)
            {
                computerCLI.DisableInput();
            }

            // Switch back to player camera
            playerCamera.Priority = 10;      // Reactivate player camera
            computerCamera.Priority = 0;     // Deactivate computer camera

            Cursor.lockState = CursorLockMode.Locked; // Lock cursor for normal gameplay
            Cursor.visible = false;

            // Re-enable player movement
            if (playerController != null)
            {
                playerController.canMove = true;
                Debug.Log("Player movement enabled");
            }
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
