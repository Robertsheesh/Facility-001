using UnityEngine;

public class DoorController : MonoBehaviour
{
    public Animator doorAnimator;       // Reference to the door's Animator
    public float openDelay = 1f;        // Delay before the door opens or closes
    public bool isDoorOpen = false;     // Track the current state of the door (open or closed)
    public AudioSource doorSound;

    private bool playerInRange = false; // Track if the player is near the button to open/close the door

    void Update()
    {
        // Check if the player is near the button and presses the left mouse button to open or close the door
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (isDoorOpen)
            {
                CloseDoor();
            }
            else
            {
                OpenDoor();
            }
        }
    }

    // Method to open the door
    public void OpenDoor()
    {
        Debug.Log("Opening door...");
        isDoorOpen = true;
        doorAnimator.SetBool("IsOpen", true); // Set the Animator parameter to play the open animation
        Invoke("StartDoorSound", openDelay); // Optional: Add a delay before playing the sound
    }

    // Method to close the door
    public void CloseDoor()
    {
        Debug.Log("Closing door...");
        isDoorOpen = false;
        doorAnimator.SetBool("IsOpen", false); // Set the Animator parameter to play the close animation
        Invoke("StartDoorSound", openDelay); // Optional: Add a delay before playing the sound
    }

    // Play sound when the door opens (optional)
    void StartDoorSound()
    {
        if (doorSound != null)
        {
            doorSound.Play();  // Start playing the door sound
        }
    }

    void StopDoorSound()
    {
        if (doorSound != null)
        {
            doorSound.Stop();  // Stop playing the door sound
        }
    }

    // Detect when the player enters the button's trigger zone
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("Player is near the door button.");
        }
    }

    // Detect when the player leaves the button's trigger zone
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log("Player left the door button area.");
        }
    }

    void Start()
    {
        doorAnimator.SetBool("IsOpen", false); // Ensure the door is closed at game start
    }
}
