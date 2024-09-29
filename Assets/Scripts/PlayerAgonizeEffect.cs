using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using System.Collections;

public class PlayerAgonizeEffect : MonoBehaviour
{
    public float maxDamage = 10f; // Max damage dealt to the player if they are close to the monster
    public float minDamage = 2f;  // Minimum damage if the player is at the edge of the range
    public float slowMultiplier = 0.5f; // How much to slow the player's movement (e.g., 50% speed)
    public float shakeDuration = 0.5f;  // Duration for screen shake
    public float shakeMagnitude = 0.1f; // Intensity of screen shake

    public float maxChromaticAberration = 1.0f; // Maximum chromatic aberration intensity
    private ChromaticAberration chromaticAberration; // Reference to the Chromatic Aberration effect

    public float maxLensDistortionY = 0.5f; // Maximum value for Lens Distortion Center Y
    private LensDistortion lensDistortion; // Reference to the Lens Distortion effect

    private SC_FPSController playerController;
    private PlayerHealth playerHealth;
    private Transform monster; // Reference to the monster
    private bool isAgonizing = false;
    private float agonizingDamageRange;

    private Vector3 initialCameraPosition;
    private Camera playerCam;

    private float originalWalkingSpeed;
    private float originalRunningSpeed;

    private bool isShaking = false; // Track if the camera shake is active

    void Start()
    {
        playerController = GetComponent<SC_FPSController>();
        playerHealth = GetComponent<PlayerHealth>(); // Get the PlayerHealth component
        playerCam = Camera.main;
        initialCameraPosition = playerCam.transform.localPosition;

        if (playerController != null)
        {
            originalWalkingSpeed = playerController.walkingSpeed;
            originalRunningSpeed = playerController.runningSpeed;
        }

        // Find the Post-Processing Volume and access the Chromatic Aberration and Lens Distortion components
        PostProcessVolume volume = FindObjectOfType<PostProcessVolume>();
        if (volume != null)
        {
            volume.profile.TryGetSettings(out chromaticAberration); // Get the chromatic aberration setting
            volume.profile.TryGetSettings(out lensDistortion); // Get the lens distortion setting
        }
    }

    void Update()
    {
        if (isAgonizing)
        {
            float distanceToMonster = Vector3.Distance(transform.position, monster.position);

            // Check if the player is within the agonizing range
            if (distanceToMonster <= agonizingDamageRange)
            {
                ApplyAgonizingEffects(distanceToMonster); // Apply effects
            }
            else
            {
                ResetPlayerEffects(); // Reset if out of range
            }
        }
    }

    // Called by the MonsterAI script when the monster starts agonizing
    public void StartAgonizing(Transform monsterTransform, float damageRange)
    {
        monster = monsterTransform; // Store the monster reference
        agonizingDamageRange = damageRange; // Store the range of agonizing effects
        isAgonizing = true;

        // Start the camera shake only if it's not already shaking
        if (!isShaking)
        {
            StartCoroutine(ShakeCamera());
        }
    }

    // Called by the MonsterAI script when the monster stops agonizing
    public void StopAgonizing()
    {
        isAgonizing = false;
        ResetPlayerEffects(); // Reset the effects on the player
    }

    private void ApplyAgonizingEffects(float distanceToMonster)
    {
        // Calculate damage based on proximity
        float damage = Mathf.Lerp(maxDamage, minDamage, distanceToMonster / agonizingDamageRange);

        // Apply damage to the player using the PlayerHealth script
        if (playerHealth != null)
        {
            playerHealth.ModifyHealth(-damage * Time.deltaTime); // Apply damage over time
        }

        // Slow player movement
        if (playerController != null)
        {
            playerController.walkingSpeed = originalWalkingSpeed * slowMultiplier;
            playerController.runningSpeed = originalRunningSpeed * slowMultiplier;
        }

        // Adjust the chromatic aberration intensity based on distance to monster
        if (chromaticAberration != null)
        {
            float proximityFactor = Mathf.Clamp01(1f - (distanceToMonster / agonizingDamageRange)); // Closer = stronger effect
            chromaticAberration.intensity.value = maxChromaticAberration * proximityFactor; // Scale the chromatic aberration
        }

        // Adjust the lens distortion "Center Y" based on distance to monster
        if (lensDistortion != null)
        {
            float proximityFactor = Mathf.Clamp01(1f - (distanceToMonster / agonizingDamageRange)); // Closer = stronger effect
            lensDistortion.centerY.value = maxLensDistortionY * proximityFactor; // Scale the lens distortion Center Y
        }

        // Ensure screen shake starts when the player is within range
        if (!isShaking) // If shaking is not already happening
        {
            StartCoroutine(ShakeCamera()); // Start the shake if not already shaking
        }
    }

    private IEnumerator ShakeCamera()
    {
        isShaking = true;
        float elapsed = 0.0f;
        Quaternion originalRotation = playerCam.transform.localRotation; // Store the original rotation

        while (isAgonizing) // Shake only while agonizing
        {
            float distanceToMonster = Vector3.Distance(transform.position, monster.position); // Calculate the distance to the monster

            // Interpolate the shake magnitude based on the distance to the monster
            float proximityFactor = Mathf.Clamp01(1f - (distanceToMonster / agonizingDamageRange)); // Closer = stronger shake
            float currentShakeMagnitude = shakeMagnitude * proximityFactor; // Scale the shake magnitude

            if (elapsed < shakeDuration)
            {
                // Apply rotational shake with the adjusted magnitude
                float xAngle = Random.Range(-1f, 1f) * currentShakeMagnitude;
                float yAngle = Random.Range(-1f, 1f) * currentShakeMagnitude;
                float zAngle = Random.Range(-1f, 1f) * currentShakeMagnitude * 0.5f; // Reduce Z-shake for less extreme effect

                // Rotate the camera around its axes based on the current shake magnitude
                playerCam.transform.localRotation = Quaternion.Euler(originalRotation.eulerAngles + new Vector3(xAngle, yAngle, zAngle));

                elapsed += Time.deltaTime;
            }
            else
            {
                elapsed = 0f; // Reset shake cycle
            }

            yield return null;
        }

        // Reset the camera's rotation after the shake effect ends
        playerCam.transform.localRotation = originalRotation;
        isShaking = false;
    }

    private void ResetPlayerEffects()
    {
        // Stop the screen shake
        StopAllCoroutines();
        playerCam.transform.localPosition = initialCameraPosition;
        isShaking = false;

        // Reset player movement to original speed
        if (playerController != null)
        {
            playerController.walkingSpeed = originalWalkingSpeed;
            playerController.runningSpeed = originalRunningSpeed;
        }

        // Reset chromatic aberration
        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.value = 0f;
        }

        // Reset lens distortion Center Y
        if (lensDistortion != null)
        {
            lensDistortion.centerY.value = 0f; // Reset to default
        }
    }
}
