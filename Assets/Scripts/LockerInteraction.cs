using UnityEngine;
using UnityEngine.UI;  // Required for UI elements

public class DoorInteraction : MonoBehaviour
{
    public Transform doorTransform;       // The door that will open/close
    public float openAngle = 90f;         // The angle the door will open to
    public float doorOpenSpeed = 2f;      // Speed of the door opening/closing
    public bool isOpen = true;           // Whether the door is currently open
    private bool isPlayerInRange = false; // Track if the player is in range to open the door

    public Text doorPromptText;           // Reference to the UI Text that shows the interaction prompt

    private Quaternion closedRotation;    // Store the original rotation of the door
    private Quaternion openRotation;      // The target rotation for the door when opened

    void Start()
    {
        // Save the initial (closed) rotation of the door
        closedRotation = doorTransform.rotation;
        // Calculate the open rotation in a single direction (e.g., always inward or outward)
        openRotation = closedRotation * Quaternion.Euler(openAngle, 0f, 0f);  // Always rotate on the Y-axis

        // Make sure the prompt text is initially hidden
        if (doorPromptText != null)
        {
            doorPromptText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            ToggleDoor();
        }

        // Smoothly rotate the door towards the target rotation (either open or closed)
        doorTransform.rotation = Quaternion.Slerp(doorTransform.rotation, isOpen ? openRotation : closedRotation, Time.deltaTime * doorOpenSpeed);
    }

    // Toggle the door open or closed
    public void ToggleDoor()
    {
        isOpen = !isOpen;

        // Hide the prompt text once the door is opened
        if (doorPromptText != null)
        {
            doorPromptText.gameObject.SetActive(false);
        }
    }

    // Detect when the player enters the door's interaction range
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;

            // Show the prompt text when the player is in range
            if (doorPromptText != null)
            {
                doorPromptText.text = "Press E to Open";  // You can modify the text if needed
                doorPromptText.gameObject.SetActive(true);
            }
        }
    }

    // Detect when the player exits the door's interaction range
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;

            // Hide the prompt text when the player is out of range
            if (doorPromptText != null)
            {
                doorPromptText.gameObject.SetActive(false);
            }
        }
    }
}
