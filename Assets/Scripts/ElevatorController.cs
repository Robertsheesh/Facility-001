using UnityEngine;
using System.Collections;

public class ElevatorController : MonoBehaviour
{
    public Animator doorAnimator;                // Reference to the Animator controlling the doors
    public AudioSource elevatorArrivingSound;    // Optional: Sound to play when the elevator "arrives"
    public AudioSource doorOpenSound;            // Sound to play when the elevator doors open
    public AudioSource doorCloseSound;           // Sound to play when the elevator doors close
    public float doorOpenDelay = 2f;             // Delay before the elevator doors open when called
    private bool isMoving = false;               // Track if the elevator is currently moving

    // Method to call the elevator (this does not move the elevator, just opens doors after a delay)
    public void CallElevator()
    {
        Debug.Log("Calling elevator...");

        // Start the process of opening the elevator doors after the delay
        StartCoroutine(OpenDoorsWithDelay());
    }

    // Coroutine to open the doors after a delay
    IEnumerator OpenDoorsWithDelay()
    {
        yield return new WaitForSeconds(doorOpenDelay);  // Wait for the specified delay time
        OpenDoors();  // Open the elevator doors after the delay
        if (elevatorArrivingSound != null)
        {
            elevatorArrivingSound.Play();  // Play sound when the elevator "arrives"
        }
    }

    // Method to open the doors
    public void OpenDoors()
    {
        Debug.Log("Attempting to open elevator doors...");

        if (doorAnimator == null)
        {
            Debug.LogError("Door Animator is not assigned!");
            return;
        }

        // Ensure the parameter exists and set it to true to open the doors
        doorAnimator.SetBool("IsOpen", true);

        Debug.Log("Set IsOpen parameter to true. Playing door opening animation.");

        // Play the door opening sound
        if (doorOpenSound != null)
        {
            doorOpenSound.Play();
            Debug.Log("Playing door opening sound.");
        }
        else
        {
            Debug.LogWarning("No door opening sound assigned.");
        }
    }

    // Method to close the doors (use before the elevator moves)
    public void CloseDoors()
    {
        Debug.Log("Elevator doors closing...");

        doorAnimator.SetBool("IsOpen", false);  // Trigger the door closing animation

        // Play the door closing sound
        if (doorCloseSound != null)
        {
            doorCloseSound.Play();
        }
    }
}
