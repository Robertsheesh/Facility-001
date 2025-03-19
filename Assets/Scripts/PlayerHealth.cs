using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Cinemachine;
using UnityEngine.Rendering.PostProcessing;

public class PlayerHealth : MonoBehaviour
{
    public float health = 100f;
    public Text healthText;  // Assign a UI Text to display health
    public AudioSource breathingAudioSource;  // Audio source for the breathing sound
    private bool isWearingSuit = false; // Track if the suit is equipped

    [Header("Fall Damage Settings")]
    public float minFallHeight = 8f; // Minimum height before taking fall damage
    public float maxFallHeight = 10f; // Height at which max fall damage is applied
    public float maxFallDamage = 70f; // Max damage taken from a fall
    private float lastGroundY; // Stores last Y position when grounded
    private bool isGrounded = true; // Track if the player is on the ground
    public AudioSource fallDamageSound;

    [Header("Blood Overlay Settings")]
    public Image bloodOverlay;  // 🩸 UI Image for blood overlay
    public float minHealthForEffect = 60f; // Health threshold to start showing blood
    public float maxOpacityHealthThreshold = 10f; // Health where blood is fully visible

    [Header("Damage Screen Effect")]
    public Camera playerCamera;  // Reference to the main player camera
    private bool isDistorting = false;

    public CinemachineVirtualCamera playerCamera1; // Assign in Inspector

    [Header("Healing Effect")]
    public Image healingOverlay; // Assign the healing overlay PNG in the inspector
    public AudioSource syringeUseSound; // Sound effect for using the med syringe
    private bool isHealing = false;  // Prevent multiple uses of the syringe

    [Header("Post Processing")]
    public PostProcessVolume postProcessingVolume;
    private ColorGrading colorGrading;


    private CharacterController characterController; // Reference to the CharacterController

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        lastGroundY = transform.position.y;

        if (bloodOverlay != null)
        {
            SetBloodOverlayAlpha(0); // Ensure the overlay starts fully invisible
        }
        if (postProcessingVolume != null)
        {
            postProcessingVolume.profile.TryGetSettings(out colorGrading);
            if (colorGrading == null)
            {
                Debug.LogError("Color Grading not found in Post Process Volume!");
            }
        }
        else
        {
            Debug.LogError("No PostProcessVolume assigned!");
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
        UpdateSaturationEffect();
        CameraShakeManager.Instance.UpdateLowHealthShake(health);
    }

    void UpdateSaturationEffect()
    {
        if (colorGrading == null) return;

        // If health is above 50, reset saturation
        if (health >= 50)
        {
            colorGrading.saturation.value = 0;  // Default saturation
        }
        else
        {
            // Reduce saturation as health drops
            float newSaturation = Mathf.Lerp(-100, 0, health / 50f);
            colorGrading.saturation.value = newSaturation;
        }
    }

    void UpdateBloodOverlay()
    {
        if (bloodOverlay == null) return;

        // Base opacity calculation (lower health = more visible)
        float baseAlpha = Mathf.InverseLerp(minHealthForEffect, maxOpacityHealthThreshold, health);
        baseAlpha = Mathf.Clamp01(baseAlpha); // Ensure valid range

        // Stronger pulse effect when critically low (health < 30)
        if (health <= 30f)
        {
            float fadeSpeed = 0.5f; // Slower fade effect (2 seconds back and forth)
            float fadeIntensity = 0.4f; // Maximum opacity variation

            float fadeValue = Mathf.PingPong(Time.time * fadeSpeed, fadeIntensity);
            baseAlpha = Mathf.Clamp01(baseAlpha + fadeValue);
        }

        SetBloodOverlayAlpha(baseAlpha);
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

                    if (damage > 0 && fallDamageSound != null)
                    {
                        fallDamageSound.Play();
                        Debug.Log($"💀 Fall Damage Taken: {damage} (Fall Height: {fallDistance}m)");
                    }

                    if (damage > 10) // Only shake for significant falls
                    {
                        float shakeIntensity = Mathf.Clamp(damage / 15f, 2f, 8f);
                        Debug.Log($"Applying Fall Shake - Intensity: {shakeIntensity}, Frequency: {Mathf.Clamp(damage / 10f, 2f, 20f)} on both cameras.");
                        CameraShakeManager.Instance.ShakeOnFall(damage);
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

        if (amount < 0) // Only distort when taking damage
        {
            StartCoroutine(ApplyScreenStretchEffect());
        }

        if (health <= 0)
        {
            Debug.Log("Player is dead!");
            // Handle player death here (disable movement, show death screen, etc.)
        }
    }

    IEnumerator ApplyScreenStretchEffect()
    {
        if (isDistorting || playerCamera == null) yield break;
        isDistorting = true;

        float duration = 0.3f;  // Effect duration
        float baseFOV = playerCamera.fieldOfView;
        float stretchFOV = baseFOV * 1.05f; // Increase FOV slightly

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float lerpFactor = Mathf.Sin((elapsedTime / duration) * Mathf.PI); // Smooth transition
            playerCamera.fieldOfView = Mathf.Lerp(baseFOV, stretchFOV, lerpFactor);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        playerCamera.fieldOfView = baseFOV; // Reset FOV
        isDistorting = false;
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
        if (isHealing) return; // Allow healing effect even at full health

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

        StartCoroutine(FadeHealingOverlay(0.15f, 0.5f)); // Always show healing effect (50% opacity)

        float healAmount = 40f;
        float healTime = 1f;
        float elapsedTime = 0f;
        float startHealth = health;

        // Apply healing if not already at max health
        if (health < 100)
        {
            while (elapsedTime < healTime)
            {
                health = Mathf.Lerp(startHealth, Mathf.Clamp(startHealth + healAmount, 0, 100), elapsedTime / healTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            health = Mathf.Clamp(startHealth + healAmount, 0, 100);
            Debug.Log("Health Restored!");
        }
        else
        {
            yield return new WaitForSeconds(1f); // Hold the effect for 1 second even if already at 100 health
        }

        StartCoroutine(FadeHealingOverlay(0f, 1f)); // Fade out overlay

        isHealing = false;
    }

    // Fade the healing overlay in or out
    private IEnumerator FadeHealingOverlay(float targetAlpha, float duration)
    {
        if (healingOverlay == null) yield break;

        Color startColor = healingOverlay.color;
        float startAlpha = startColor.a;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);
            healingOverlay.color = new Color(startColor.r, startColor.g, startColor.b, newAlpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        healingOverlay.color = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);
    }
}
