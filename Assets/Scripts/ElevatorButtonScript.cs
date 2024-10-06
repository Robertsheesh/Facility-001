using UnityEngine;
using System.Collections;

public class ElevatorButtonScript : MonoBehaviour, IInteractable
{
    public Animator buttonAnimator;           // Reference to the button's Animator
    public ElevatorController elevator;       // Reference to the ElevatorController script

    public AudioSource buttonSound;           // Optional: Sound for when the button is pressed
    private bool buttonPressed = false;       // Track if the button has already been pressed

    public Renderer buttonRenderer;           // Reference to the button's Renderer component
    public Material defaultMaterial;          // Material for the button in the default state
    public Material pressedMaterial;          // Material for the button in the pressed state

    public void Interact()
    {
        if (!buttonPressed)  // Check if the button hasn't already been pressed
        {
            buttonPressed = true;
            AnimateButtonPress();  // Trigger the button animation

            if (buttonSound != null)
            {
                buttonSound.Play();  // Play button press sound (optional)
            }

            // Change the button's texture to the "pressed" state
            ChangeButtonTexture(true);

            // Call the elevator after pressing the button
            elevator.CallElevator();
        }
    }

    void AnimateButtonPress()
    {
        if (buttonAnimator != null)
        {
            buttonAnimator.SetTrigger("Pressed");  // Trigger the button press animation
        }
    }

    // This method will change the button texture with a 2-second delay
    public void ChangeButtonTexture(bool isPressed)
    {
        // Start a coroutine to add the delay
        StartCoroutine(ChangeTextureAfterDelay(isPressed));
    }

    // Coroutine to handle the 2-second delay
    private IEnumerator ChangeTextureAfterDelay(bool isPressed)
    {
        yield return new WaitForSeconds(0.2f); // Wait for 2 seconds before executing the code below

        if (buttonRenderer != null)
        {
            // Switch between the default and pressed material based on the button state
            if (isPressed)
            {
                buttonRenderer.material = pressedMaterial;  // Change to pressed material
            }
            else
            {
                buttonRenderer.material = defaultMaterial;  // Revert to default material
            }
        }
    }
}

