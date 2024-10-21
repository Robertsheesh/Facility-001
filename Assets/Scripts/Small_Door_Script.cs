using UnityEngine;

public class DoorSwitch : MonoBehaviour, IInteractable
{
    public DoorController1 doorController;   // Reference to the DoorController1 instance
    public AudioSource buttonPressSound;    // Button press sound

    // Implementation of the Interact method from IInteractable
    public void Interact()
    {
        if (doorController != null) // Check if doorController reference is assigned
        {
            ButtonPressSound();
            doorController.ToggleDoor(); // Call ToggleDoor() on the doorController instance
        }
    }

    // Play button press sound
    private void ButtonPressSound()
    {
        if (buttonPressSound != null)
        {
            buttonPressSound.Play();  // Start playing the button press sound
        }
    }
}
