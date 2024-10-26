using System.Collections;
using UnityEngine;
using System;

public class AirlockController : MonoBehaviour
{
    public event Action OnAirlockSequenceComplete;

    public Animator firstDoorAnimator;    // Animator for the first door (entry door)
    public Animator secondDoorAnimator;   // Animator for the second door (exit door)
    public Animator leverAnimator;        // Animator for the lever
    public ParticleSystem smokeSystem;    // Particle system for smoke inside the airlock
    public AudioSource doorSound;         // Sound for the first door opening/closing
    public AudioSource door2Sound;        // Sound for the second door opening/closing
    public AudioSource leverSound;        // Sound for the lever switch
    public AudioSource smokeSound;        // Sound for the smoke
    public AudioSource alarmSound;        // Alarm sound for the airlock sequence
    public Light light1;                  // First light to flicker during operation
    public Light light2;                  // Second light to flicker during operation
    public Renderer[] lampRenderers;      // Array of lamp renderers with emission materials
    public float smokeDuration = 10f;     // Duration for the smoke particle system
    public float flickerInterval = 0.2f;  // Interval for the lights to flicker
    public float soundFadeDuration = 1f;  // Duration for smoke sound fade in and out

    private bool isInUse = false;         // Prevent multiple interactions during the process
    private Color[] originalEmissionColors; // Store original emission colors of the lamps

    void Start()
    {
        // Ensure the smoke system is disabled (not playing) initially
        if (smokeSystem != null)
        {
            smokeSystem.Stop();
            smokeSystem.gameObject.SetActive(false); // Disable smoke at the start
        }

        // Ensure lights are on initially
        if (light1 != null) light1.enabled = true;
        if (light2 != null) light2.enabled = true;

        // Store the original emission color of the lamps
        if (lampRenderers != null && lampRenderers.Length > 0)
        {
            originalEmissionColors = new Color[lampRenderers.Length];
            for (int i = 0; i < lampRenderers.Length; i++)
            {
                originalEmissionColors[i] = lampRenderers[i].material.GetColor("_EmissionColor");
            }
        }
    }

    // The method that handles interaction when the player presses "E" on the switch
    public void InteractWithAirlock()
    {
        if (!isInUse) // Check if the airlock is already in use
        {
            StartCoroutine(AirlockSequence());
        }
    }

    // Coroutine that handles the full airlock sequence
    private IEnumerator AirlockSequence()
    {
        isInUse = true;

        // 1. Play the lever switch sound and animate the lever (turn it on)
        if (leverAnimator != null)
        {
            leverAnimator.SetBool("IsOn", true); // Turn the lever on
            PlayLeverSound();
        }

        // 2. Wait for 0.5 seconds before closing the first door
        yield return new WaitForSeconds(0.5f);

        // 3. Close the first door and play the door closing sound
        if (firstDoorAnimator != null)
        {
            firstDoorAnimator.SetBool("IsOpen", false); // Close the first door
            PlayDoorSound();
        }

        // Wait for 2 seconds before starting the smoke
        yield return new WaitForSeconds(2f);

        // 4. Activate the smoke system and start fading in the smoke sound
        if (smokeSystem != null)
        {
            smokeSystem.gameObject.SetActive(true); // Activate the smoke object
            smokeSystem.Play(); // Start the smoke particle system
            StartCoroutine(FadeInSound(smokeSound, soundFadeDuration)); // Fade in the smoke sound
        }

        // 5. Play alarm sound while the operation is in progress
        if (alarmSound != null)
        {
            alarmSound.Play(); // Start the alarm sound
        }

        // 6. Start flickering lights and lamp emission during the smoke duration
        StartCoroutine(FlickerLightsAndEmission());

        // 7. Wait for the smoke duration
        yield return new WaitForSeconds(smokeDuration);

        // 8. Stop emitting particles but allow current particles to fade out naturally
        if (smokeSystem != null)
        {
            smokeSystem.Stop(withChildren: false, stopBehavior: ParticleSystemStopBehavior.StopEmitting);
        }

        // 9. Fade out the smoke sound
        StartCoroutine(FadeOutSound(smokeSound, soundFadeDuration));

        // 10. Stop the alarm sound after the sequence
        if (alarmSound != null)
        {
            alarmSound.Stop(); // Stop the alarm sound
        }

        // Wait for 0.5 seconds before resetting the lever
        yield return new WaitForSeconds(2f);

        // 11. Reset the lever to its original position (turn it off)
        if (leverAnimator != null)
        {
            leverAnimator.SetBool("IsOn", false); // Turn the lever off
        }

        // 12. Open the second door and play the door opening sound
        if (secondDoorAnimator != null)
        {
            secondDoorAnimator.SetBool("IsOpen", true); // Open the second door
            PlayDoorSound2();
        }

        // 13. Wait until all smoke particles have disappeared before disabling the smoke system
        yield return new WaitUntil(() => !smokeSystem.IsAlive(true));

        // Ensure the smoke system is deactivated after all particles are gone
        smokeSystem.gameObject.SetActive(false);

        isInUse = false; // The airlock process is complete
        OnAirlockSequenceComplete?.Invoke();
    }

    // Coroutine to handle flickering of lights and lamps' emission during the operation
    private IEnumerator FlickerLightsAndEmission()
    {
        float elapsedTime = 0f;

        while (elapsedTime < smokeDuration)
        {
            // Toggle light states
            if (light1 != null) light1.enabled = !light1.enabled;
            if (light2 != null) light2.enabled = !light2.enabled;

            // Toggle emission on lamps
            if (lampRenderers != null && originalEmissionColors != null)
            {
                for (int i = 0; i < lampRenderers.Length; i++)
                {
                    // Toggle emission between original color and black (off)
                    Color emissionColor = lampRenderers[i].material.GetColor("_EmissionColor") == Color.black
                        ? originalEmissionColors[i]
                        : Color.black;
                    lampRenderers[i].material.SetColor("_EmissionColor", emissionColor);

                    // Enable or disable the emission keyword based on color
                    if (emissionColor == Color.black)
                    {
                        lampRenderers[i].material.DisableKeyword("_EMISSION");
                    }
                    else
                    {
                        lampRenderers[i].material.EnableKeyword("_EMISSION");
                    }
                }
            }

            // Wait for flicker interval before toggling again
            yield return new WaitForSeconds(flickerInterval);

            elapsedTime += flickerInterval;
        }

        // Ensure both lights and lamp emissions are back on at the end of the flicker sequence
        if (light1 != null) light1.enabled = true;
        if (light2 != null) light2.enabled = true;

        if (lampRenderers != null)
        {
            for (int i = 0; i < lampRenderers.Length; i++)
            {
                // Restore original emission color
                lampRenderers[i].material.SetColor("_EmissionColor", originalEmissionColors[i]);
                lampRenderers[i].material.EnableKeyword("_EMISSION");
            }
        }
    }

    // Coroutine to fade in sound
    private IEnumerator FadeInSound(AudioSource audioSource, float duration)
    {
        float currentTime = 0f;
        audioSource.volume = 0f;  // Start with volume at 0
        audioSource.Play();       // Start playing the sound

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, 0.4f, currentTime / duration); // Gradually increase volume
            yield return null;
        }
    }

    // Coroutine to fade out sound
    private IEnumerator FadeOutSound(AudioSource audioSource, float duration)
    {
        float currentTime = 0f;
        float startVolume = audioSource.volume; // Get the current volume to fade from

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, currentTime / duration); // Gradually decrease volume
            yield return null;
        }

        // Ensure the volume reaches zero and stop the sound after the fade-out
        audioSource.volume = 0f;
        audioSource.Stop();
    }

    // Play the first door sound (opening or closing)
    private void PlayDoorSound()
    {
        if (doorSound != null)
        {
            doorSound.Play();
        }
    }

    // Play the second door sound (opening or closing)
    private void PlayDoorSound2()
    {
        if (door2Sound != null)
        {
            door2Sound.Play();
        }
    }

    // Play the lever sound
    private void PlayLeverSound()
    {
        if (leverSound != null)
        {
            leverSound.Play();
        }
    }
}
