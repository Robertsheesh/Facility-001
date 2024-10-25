using System.Collections;
using UnityEngine;

public class UnopenableDoorButton : MonoBehaviour, IInteractable
{
    public AudioSource buttonPressSound;   // The sound that plays when the button is pressed
    public Light redLight;                 // The red light associated with the button
    public float flashDuration = 0.2f;     // How long each flash lasts
    public int flashCount = 3;             // Number of times the light flashes
    public float delayBeforeFlashing = 0.5f; // Delay before flashing starts

    private bool isFlashing = false;       // Prevent multiple interactions during flashing

    void Start()
    {
        // Ensure the red light is on at the start
        if (redLight != null)
        {
            redLight.enabled = true;
        }
    }

    // Implementation of the Interact method from IInteractable
    public void Interact()
    {
        if (!isFlashing)
        {
            // Play the button press sound
            ButtonPressSound();

            // Start the red light flashing sequence after a delay
            StartCoroutine(FlashRedLight());
        }
    }

    // Play the button press sound
    private void ButtonPressSound()
    {
        if (buttonPressSound != null)
        {
            buttonPressSound.Play();  // Play the sound
        }
    }

    // Coroutine to flash the red light after a delay
    private IEnumerator FlashRedLight()
    {
        isFlashing = true;

        // Wait for the delay before flashing starts
        yield return new WaitForSeconds(delayBeforeFlashing);

        // Flashing sequence
        for (int i = 0; i < flashCount; i++)
        {
            // Turn off the red light
            if (redLight != null)
            {
                redLight.enabled = false;
            }

            // Wait for the flash duration
            yield return new WaitForSeconds(flashDuration);

            // Turn the red light back on
            if (redLight != null)
            {
                redLight.enabled = true;
            }

            // Wait for the flash duration before the next flash
            yield return new WaitForSeconds(flashDuration);
        }

        // Ensure the red light remains on after the flashing sequence
        if (redLight != null)
        {
            redLight.enabled = true;
        }

        isFlashing = false;
    }
}
