using UnityEngine;
using System.Collections;

public class ExperimentDoor : MonoBehaviour
{
    public Animator doorAnimator;      // Reference to the door's Animator
    public AudioSource doorSound;      // Optional: Door sound for opening/closing
    public Light buttonLight1;         // First button light (e.g., one side of the door)

    private bool doorOpened = false;   // Track if the door is opened or closed
    private bool isInUse = false;      // Prevent spamming interactions

    public float openDelay = 0f;       // Delay before the door opens or closes

    // Ensure the door starts closed
    void Start()
    {
        doorAnimator.SetBool("IsOpen", false);
        SetLights(Color.red); // Both lights start red when the door is closed
    }

    // Method to toggle the door's state (open/close)
    public void ToggleDoor()
    {
        if (isInUse) return; // If interaction is in progress, return early

        if (doorOpened)
        {
            CloseDoor(); // If the door is open, close it
        }
        else
        {
            OpenDoor(); // If the door is closed, open it
        }
    }

    // Method to open the door with a delay
    private void OpenDoor()
    {
        if (isInUse) return; // If interaction is in progress, return early

        isInUse = true;  // Mark as in use to prevent multiple interactions
        StartCoroutine(OpenDoorWithDelay());  // Start the coroutine to open the door with a delay
        SetLights(Color.green);  // Change both lights to green when the door is opened
        doorOpened = true;  // Mark the door as opened
    }

    // Coroutine to handle the door opening after the delay
    private IEnumerator OpenDoorWithDelay()
    {
        yield return new WaitForSeconds(openDelay);  // Wait for the specified delay time

        doorAnimator.SetBool("IsOpen", true);  // Set the Animator parameter to play the open animation
        PlayDoorSound();  // Play the door opening sound AFTER the delay
        isInUse = false;  // Mark interaction as complete
    }

    // Method to close the door with a delay
    private void CloseDoor()
    {
        if (isInUse) return; // If interaction is in progress, return early

        isInUse = true;  // Mark as in use to prevent multiple interactions
        StartCoroutine(CloseDoorWithDelay());  // Start the coroutine to close the door with a delay
        SetLights(Color.red);  // Change both lights to red when the door is closed
        doorOpened = false;  // Mark the door as closed
    }

    // Coroutine to handle the door closing after the delay
    private IEnumerator CloseDoorWithDelay()
    {
        yield return new WaitForSeconds(openDelay);  // Wait for the specified delay time

        doorAnimator.SetBool("IsOpen", false);  // Set the Animator parameter to play the close animation
        PlayDoorSound();  // Play the door closing sound AFTER the delay
        isInUse = false;  // Mark interaction as complete
    }

    // Play door sound
    private void PlayDoorSound()
    {
        if (doorSound != null)
        {
            doorSound.Play();  // Play the door sound (either open or close)
        }
    }

    // Helper method to set both button lights' colors
    private void SetLights(Color color)
    {
        buttonLight1.color = color;
    }
}
