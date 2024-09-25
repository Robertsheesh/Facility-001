using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public float health = 100f;
    public Text healthText;  // Assign a UI Text to display health
    private bool isWearingSuit = false; // Track if the suit is equipped
    public AudioSource breathingAudioSource;  // Audio source for the breathing sound

    private void Update()
    {
        // Update health display in the UI
        if (healthText != null)
        {
            healthText.text = "Health: " + health.ToString("F0") + "%";
        }
    }

    // Method to modify health (can be used for various damage types)
    public void ModifyHealth(float amount)
    {
        health += amount;
        health = Mathf.Clamp(health, 0, 100);  // Keep health between 0 and 100

        if (health <= 0)
        {
            Debug.Log("Player is dead!");
            // Handle player death here
        }
    }

    // Equip the radiation suit (called when the player picks up the suit)
    public void EquipRadiationSuit()
    {
        if (!isWearingSuit)
        {
            isWearingSuit = true;
            Debug.Log("Radiation suit equipped.");

            // Start the breathing sound when the suit is equipped
            if (breathingAudioSource != null)
            {
                breathingAudioSource.loop = true;  // Loop the breathing sound
                breathingAudioSource.Play();       // Play the breathing sound
            }
        }
    }

    // Remove the radiation suit (optional, in case you want to remove the suit)
    public void RemoveRadiationSuit()
    {
        if (isWearingSuit)
        {
            isWearingSuit = false;
            Debug.Log("Radiation suit removed.");

            // Stop the breathing sound when the suit is removed
            if (breathingAudioSource != null && breathingAudioSource.isPlaying)
            {
                breathingAudioSource.Stop();       // Stop the breathing sound
            }
        }
    }

    // Method to check if the player is wearing the suit
    public bool IsWearingSuit()
    {
        return isWearingSuit;
    }
}
