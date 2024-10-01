using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using Cinemachine; // Import Cinemachine for camera noise
using System.Collections;

public class PlayerAgonizeEffect : MonoBehaviour
{
    public float maxDamage = 10f;
    public float minDamage = 2f;
    public float slowMultiplier = 0.5f;

    public float shakeDuration = 0.5f;
    public float shakeMagnitude = 0.1f;

    public float maxChromaticAberration = 1.0f;
    private ChromaticAberration chromaticAberration;

    public float maxLensDistortionY = 0.5f;
    private LensDistortion lensDistortion;

    private SC_FPSController playerController;
    private PlayerHealth playerHealth;
    private Transform monster;
    private bool isAgonizing = false;
    private float agonizingDamageRange;

    private float originalWalkingSpeed;
    private float originalRunningSpeed;

    // Cinemachine variables for camera shake
    public CinemachineVirtualCamera cinemachineCamera;  // Reference to player's Cinemachine camera
    private CinemachineBasicMultiChannelPerlin cinemachineNoise;  // Reference to Cinemachine noise

    void Start()
    {
        playerController = GetComponent<SC_FPSController>();
        playerHealth = GetComponent<PlayerHealth>();

        if (playerController != null)
        {
            originalWalkingSpeed = playerController.walkingSpeed;
            originalRunningSpeed = playerController.runningSpeed;
        }

        // Post-process effects
        PostProcessVolume volume = FindObjectOfType<PostProcessVolume>();
        if (volume != null)
        {
            volume.profile.TryGetSettings(out chromaticAberration);
            volume.profile.TryGetSettings(out lensDistortion);
        }

        // Get the Cinemachine noise component from the Cinemachine camera
        if (cinemachineCamera != null)
        {
            cinemachineNoise = cinemachineCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }
        else
        {
            Debug.LogError("Cinemachine camera is not assigned.");
        }
    }

    void Update()
    {
        if (isAgonizing)
        {
            float distanceToMonster = Vector3.Distance(transform.position, monster.position);

            if (distanceToMonster <= agonizingDamageRange)
            {
                ApplyAgonizingEffects(distanceToMonster);
            }
            else
            {
                ResetPlayerEffects();
            }
        }
    }

    public void StartAgonizing(Transform monsterTransform, float damageRange)
    {
        monster = monsterTransform;
        agonizingDamageRange = damageRange;
        isAgonizing = true;

        // Start camera noise shake if not already shaking
        if (cinemachineNoise != null)
        {
            cinemachineNoise.m_AmplitudeGain = 0; // Reset noise
            StartCoroutine(ApplyCinemachineShake());
        }
    }

    public void StopAgonizing()
    {
        isAgonizing = false;
        ResetPlayerEffects();
    }

    private void ApplyAgonizingEffects(float distanceToMonster)
    {
        // Calculate damage based on proximity
        float damage = Mathf.Lerp(maxDamage, minDamage, distanceToMonster / agonizingDamageRange);
        if (playerHealth != null)
        {
            playerHealth.ModifyHealth(-damage * Time.deltaTime);
        }

        // Slow player movement
        if (playerController != null)
        {
            playerController.walkingSpeed = originalWalkingSpeed * slowMultiplier;
            playerController.runningSpeed = originalRunningSpeed * slowMultiplier;
        }

        // Adjust chromatic aberration intensity
        if (chromaticAberration != null)
        {
            float proximityFactor = Mathf.Clamp01(1f - (distanceToMonster / agonizingDamageRange));
            chromaticAberration.intensity.value = maxChromaticAberration * proximityFactor;
        }

        // Adjust lens distortion based on proximity
        if (lensDistortion != null)
        {
            float proximityFactor = Mathf.Clamp01(1f - (distanceToMonster / agonizingDamageRange));
            lensDistortion.centerY.value = maxLensDistortionY * proximityFactor;
        }
    }

    private IEnumerator ApplyCinemachineShake()
    {
        float elapsed = 0f;
        while (isAgonizing && elapsed < shakeDuration)
        {
            float distanceToMonster = Vector3.Distance(transform.position, monster.position);
            float proximityFactor = Mathf.Clamp01(1f - (distanceToMonster / agonizingDamageRange));
            float currentShakeMagnitude = shakeMagnitude * proximityFactor;

            // Apply noise magnitude based on proximity
            if (cinemachineNoise != null)
            {
                cinemachineNoise.m_AmplitudeGain = Mathf.Lerp(cinemachineNoise.m_AmplitudeGain, currentShakeMagnitude, Time.deltaTime * 5f);
                cinemachineNoise.m_FrequencyGain = 1.0f; // You can adjust this frequency if needed
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Reset noise after shake ends
        if (cinemachineNoise != null)
        {
            cinemachineNoise.m_AmplitudeGain = Mathf.Lerp(cinemachineNoise.m_AmplitudeGain, 0f, Time.deltaTime * 5f);
        }
    }

    private void ResetPlayerEffects()
    {
        StopAllCoroutines();

        // Reset player movement
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

        // Reset lens distortion
        if (lensDistortion != null)
        {
            lensDistortion.centerY.value = 0f;
        }

        // Reset Cinemachine noise
        if (cinemachineNoise != null)
        {
            cinemachineNoise.m_AmplitudeGain = 0f;
            cinemachineNoise.m_FrequencyGain = 0f;
        }
    }
}
