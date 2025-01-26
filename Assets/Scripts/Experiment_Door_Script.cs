using UnityEngine;
using System.Collections;

public class ExperimentDoor : MonoBehaviour
{
    public Animator doorAnimator;      // Reference to the door's Animator
    public AudioSource doorSound;      // Optional: Door sound for opening/closing
    public Light buttonLight1;         // First button light (e.g., one side of the door)

    public Light chamberLight1;
    public Light chamberLight2;

    private bool doorOpened = false;   // Track if the door is opened or closed
    private bool isInUse = false;      // Prevent spamming interactions

    public float openDelay = 0f;       // Delay before the door opens or closes

    public AudioSource lightHumm1;
    public AudioSource lightHumm2;
    public AudioSource flickerSound;

    // Ensure the door starts closed
    void Start()
    {
        doorAnimator.SetBool("IsOpen", false);
        SetLights(Color.red); // Both lights start red when the door is closed
        if (chamberLight1 != null) chamberLight1.enabled = false;
        if (chamberLight2 != null) chamberLight2.enabled = false;
        lightHumm1.Stop();
        lightHumm2.Stop();
    }

    public bool IsDoorOpen()
    {
        return doorOpened;
    }

    public bool IsDoorClosed()
    {
        return !doorOpened; // Returns true if the door is NOT open (i.e., closed)
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
        StartCoroutine(FlickerChamberLights());
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
        StartCoroutine(TurnOffChamberLights());
    }

    // Coroutine to flicker the chamber lights on with delay
    private IEnumerator FlickerChamberLights()
    {
        yield return new WaitForSeconds(1.5f);  // Initial delay before lights start flickering

        int flickerCount = 3;  // Number of flickers
        float flickerSpeed = 0.1f;  // Speed of flickering

        for (int i = 0; i < flickerCount; i++)
        {
            if (chamberLight1 != null) chamberLight1.enabled = !chamberLight1.enabled;
            if (chamberLight2 != null) chamberLight2.enabled = !chamberLight2.enabled;
            yield return new WaitForSeconds(flickerSpeed);
        }

        // Ensure the lights stay on after flickering
        if (chamberLight1 != null) chamberLight1.enabled = true;
        if (chamberLight2 != null) chamberLight2.enabled = true;

        lightHumm1.Play();
        lightHumm2.Play();
        flickerSound.Play();
    }

    // Coroutine to turn off chamber lights after a short delay
    private IEnumerator TurnOffChamberLights()
    {
        yield return new WaitForSeconds(2.5f);  // Wait for 1 second before turning off the lights

        if (chamberLight1 != null) chamberLight1.enabled = false;
        if (chamberLight2 != null) chamberLight2.enabled = false;

        lightHumm1.Stop();
        lightHumm2.Stop();
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
