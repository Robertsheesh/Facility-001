using UnityEngine;
using Cinemachine;

public class ComputerCameraSwitcher : MonoBehaviour
{
    public CinemachineVirtualCamera playerCamera;    // The player's Cinemachine virtual camera
    public CinemachineVirtualCamera computerCamera;  // The computer's Cinemachine virtual camera
    public GameObject computerUI;                    // Reference to the world-space canvas for the computer UI
    public float interactionDistance = 2.5f;         // Maximum distance to interact with the computer
    public GameObject computerCLIObject;             // Reference to the GameObject that holds the ComputerCLIExperiment script
    public GameObject pauseMenu;                     // Reference to the Pause Menu
    public PlayerInteraction PlayerInteractionScript; // Reference to the PlayerInteraction script
    public GameObject interactionUIParent;           // Reference to the interaction UI parent object
    public ObjectPicker objectPicker;
    private bool isComputerActive = false;

    private ComputerCLIExperiment computerCLI;       // Reference to the ComputerCLIExperiment script
    public bool isUsingComputer = false;             // Track if player is using the computer
    private SC_FPSController playerController;       // Player controller for movement control

    private bool interactionInProgress = false;      // To prevent interaction spamming
    private bool isPlayerInRange = false;            // Track if player is within the trigger range

    void Start()
    {
        playerController = FindObjectOfType<SC_FPSController>();
        if (computerCLIObject != null)
        {
            computerCLI = computerCLIObject.GetComponent<ComputerCLIExperiment>();
        }

        if (computerUI != null)
        {
            computerUI.SetActive(true);
        }
        else
        {
            Debug.LogError("Computer UI is not assigned!");
        }

        if (computerCamera != null)
        {
            computerCamera.Priority = 0;
        }
        else
        {
            Debug.LogError("Computer camera is not assigned!");
        }

        if (computerCLI != null)
        {
            computerCLI.DisableInput();
        }
        else
        {
            Debug.LogError("ComputerCLIExperiment script not found on the assigned object!");
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !interactionInProgress && isPlayerInRange)
        {
            ToggleComputer();
        }

        if (Input.GetKeyDown(KeyCode.Escape) && isUsingComputer)
        {
            ExitComputerMode();
        }
    }

    void ToggleComputer()
    {
        if (interactionInProgress) return;
        interactionInProgress = true;

        if (objectPicker != null)
        {
            objectPicker.UnequipCurrentItem(); // Call the unequip function
        }

        isComputerActive = !isComputerActive;

        if (objectPicker != null)
        {
            objectPicker.isUsingComputer = isComputerActive;
        }

        isUsingComputer = !isUsingComputer;

        if (isUsingComputer)
        {
            EnterComputerMode();
        }
        else
        {
            ExitComputerMode();
        }

        Invoke("ResetInteraction", 0.2f);
    }

    void EnterComputerMode()
    {
        playerCamera.Priority = 0;
        computerCamera.Priority = 10;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (interactionUIParent != null)
        {
            interactionUIParent.SetActive(false);
        }

        if (playerController != null)
        {
            playerController.canMove = false;
        }

        if (computerCLI != null)
        {
            computerCLI.EnableInput();
        }
    }

    void ExitComputerMode()
    {

        if (objectPicker != null)
        {
            objectPicker.isUsingComputer = false; // Re-enable inventory keys
        }

        playerCamera.Priority = 10;
        computerCamera.Priority = 0;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (interactionUIParent != null)
        {
            interactionUIParent.SetActive(true);
        }

        if (playerController != null)
        {
            playerController.canMove = true;
        }

        if (computerCLI != null)
        {
            computerCLI.DisableInput();
        }
    }

    private void ResetInteraction()
    {
        interactionInProgress = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }
}