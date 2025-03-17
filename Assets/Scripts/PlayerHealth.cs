using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public float health = 100f;
    public Text healthText;  // Assign a UI Text to display health
    public AudioSource breathingAudioSource;  // Audio source for the breathing sound
    public AudioSource syringeUseSound; // Sound effect for using the med syringe
    private bool isWearingSuit = false; // Track if the suit is equipped
    private bool isHealing = false;  // Prevent multiple uses of the syringe

    [Header("Fall Damage Settings")]
    public float minFallHeight = 3f; // Minimum height before taking fall damage
    public float maxFallHeight = 10f; // Height at which max fall damage is applied
    public float maxFallDamage = 50f; // Max damage taken from a fall
    private float lastGroundY; // Stores last Y position when grounded
    private bool isGrounded = true; // Track if the player is on the ground
    public AudioSource fallDamageSound;

    [Header("Blood Overlay Settings")]
    public Image bloodOverlay;  // 🩸 UI Image for blood overlay
    public float minHealthForEffect = 60f; // Health threshold to start showing blood
    public float maxOpacityHealthThreshold = 10f; // Health where blood is fully visible

    private CharacterController characterController; // Reference to the CharacterController

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        lastGroundY = transform.position.y;

        if (bloodOverlay != null)
        {
            SetBloodOverlayAlpha(0); // Ensure the overlay starts fully invisible
        }
    }

    private void Update()
    {
        // Update health display in the UI
        if (healthText != null)
        {
            healthText.text = "Health: " + health.ToString("F0") + "%";
        }

        HandleFallDamage();
        UpdateBloodOverlay();
    }

    void UpdateBloodOverlay()
    {
        if (bloodOverlay == null) return;

        // Calculate base opacity based on health (lower health = higher opacity)
        float alpha = Mathf.InverseLerp(minHealthForEffect, maxOpacityHealthThreshold, health);
        alpha = 0 + Mathf.Clamp01(alpha); // Invert so lower health = higher opacity

        // Add pulse effect when health is very low (below 30 HP)
        if (health <= 30f)
        {
            float pulseSpeed = 2.5f; // Speed of the pulsing effect
            float pulse = Mathf.Abs(Mathf.Sin(Time.time * pulseSpeed)); // Oscillates between 0 and 1
            alpha = Mathf.Lerp(alpha * 0.8f, alpha, pulse); // Smooth pulsing
        }

        SetBloodOverlayAlpha(alpha);
    }


    void SetBloodOverlayAlpha(float alpha)
    {
        Color overlayColor = bloodOverlay.color;
        overlayColor.a = alpha;
        bloodOverlay.color = overlayColor;
    }


    // Handle Fall Damage Logic
    private void HandleFallDamage()
    {
        if (characterController.isGrounded)
        {
            if (!isGrounded) // Just landed
            {
                float fallDistance = lastGroundY - transform.position.y;

                if (fallDistance > minFallHeight)
                {
                    float damage = Mathf.Lerp(0, maxFallDamage, (fallDistance - minFallHeight) / (maxFallHeight - minFallHeight));
                    ModifyHealth(-damage);

                    // Play Fall Damage Sound (Only if Damage Taken)
                    if (damage > 0 && fallDamageSound != null)
                    {
                        fallDamageSound.Play();
                        Debug.Log($"Fall Damage: {damage} (Fall Height: {fallDistance}m)");
                    }
                }
            }

            isGrounded = true;
            lastGroundY = transform.position.y;
        }
        else
        {
            if (isGrounded) // Start Falling
            {
                lastGroundY = transform.position.y;
            }
            isGrounded = false;
        }
    }

    // Modify health (can be used for various damage types)
    public void ModifyHealth(float amount)
    {
        health += amount;
        health = Mathf.Clamp(health, 0, 100);  // Keep health between 0 and 100

        if (health <= 0)
        {
            Debug.Log("Player is dead!");
            // Handle player death here (disable movement, show death screen, etc.)
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

    // Use Medical Syringe
    public void UseMedSyringe()
    {
        if (isHealing || health >= 100) return; // Prevent over-healing or multiple uses

        isHealing = true;
        StartCoroutine(HealOverTime());
    }

    private IEnumerator HealOverTime()
    {
        Debug.Log("Using Med Syringe...");

        if (syringeUseSound != null)
        {
            syringeUseSound.Play(); // Play the healing sound
        }

        float healAmount = 40f; // Amount of health to restore
        float healTime = 1f; // Duration of healing effect
        float elapsedTime = 0f;
        float startHealth = health;

        while (elapsedTime < healTime)
        {
            health = Mathf.Lerp(startHealth, Mathf.Clamp(startHealth + healAmount, 0, 100), elapsedTime / healTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        health = Mathf.Clamp(startHealth + healAmount, 0, 100); // Ensure exact healing

        Debug.Log("Health Restored!");
        isHealing = false;
    }
}
