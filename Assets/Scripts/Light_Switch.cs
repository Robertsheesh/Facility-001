using UnityEngine;

public class LightSwitch : MonoBehaviour
{
    public Light spotlight;  // Assign the spotlight in the Inspector
    public Animator leverAnimator;  // Assign the lever's Animator in the Inspector
    public float interactionRange = 2f;  // Range in which the player can interact with the switch
    public Transform player;  // Assign the player's transform in the Inspector
    public AudioSource lightSound;  // Audio source for light switch sound

    private bool isLightOn = true;  // Track whether the light is on or off

    void Start()
    {
        if (spotlight == null)
        {
            Debug.LogError("No spotlight assigned to the LightSwitch script.");
        }

        if (leverAnimator == null)
        {
            Debug.LogError("No lever animator assigned to the LightSwitch script.");
        }

        // Sync the initial lever state with the light state
        isLightOn = spotlight.enabled;  // Check if the light is initially on or off
        leverAnimator.SetBool("IsLightOn", isLightOn);  // Set the animator to match the light state
    }

    void Update()
    {
        // Check if the player is within range
        if (Vector3.Distance(player.position, transform.position) <= interactionRange)
        {
            // Check if the player presses the "E" key
            if (Input.GetKeyDown(KeyCode.E))
            {
                ToggleLight();
            }
        }
    }

    // Toggle the light and animation
    void ToggleLight()
    {
        if (spotlight != null && leverAnimator != null)
        {
            isLightOn = !isLightOn;  // Toggle the light state
            spotlight.enabled = isLightOn;  // Turn the light on or off

            // Trigger the lever animation
            leverAnimator.SetBool("IsLightOn", isLightOn);
        }

        StartLightSound();
    }

    // Play the light switch sound
    void StartLightSound()
    {
        if (lightSound != null)
        {
            lightSound.Play();  // Start playing the light sound
        }
    }

    // Optional: Stop the light sound (if needed)
    void StopLightSound()
    {
        if (lightSound != null)
        {
            lightSound.Stop();  // Stop playing the light sound
        }
    }
}
