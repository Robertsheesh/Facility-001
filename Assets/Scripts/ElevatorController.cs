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
        Debug.Log("Elevator doors opening...");

        doorAnimator.SetBool("IsOpen", true);  // Trigger the door opening animation

        // Play the door opening sound
        if (doorOpenSound != null)
        {
            doorOpenSound.Play();
        }
    }

    // Method to close the doors (use before the elevator moves)
    public void CloseDoors()
    {
        Debug.Log("Elevator doors closing...");

        doorAnimator.SetBool("IsOpen", false);  // Trigger the door closing animation

        doorCloseSound.Play();

    }
}
