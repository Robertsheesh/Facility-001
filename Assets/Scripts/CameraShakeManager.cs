using System.Collections;
using UnityEngine;
using Cinemachine;

public class CameraShakeManager : MonoBehaviour
{
    public static CameraShakeManager Instance { get; private set; }

    private CinemachineVirtualCamera activeCamera;
    private CinemachineVirtualCamera standingCamera;
    private CinemachineVirtualCamera crouchingCamera;
    private CinemachineBasicMultiChannelPerlin noise;

    private CinemachineBasicMultiChannelPerlin standingNoise;
    private CinemachineBasicMultiChannelPerlin crouchingNoise;


    private Coroutine currentShakeRoutine;

    [Header("Head Bob Settings")]
    public float walkingBobIntensity = 0.2f;
    public float walkingBobFrequency = 1.5f;
    public float runningBobIntensity = 0.2f;
    public float runningBobFrequency = 2.5f;
    public float crouchBobIntensity = 0.2f;
    public float crouchBobFrequency = 1.0f;

    private float targetBobIntensity = 0f;
    private float targetBobFrequency = 0f;

    [Header("Fall Damage Shake")]
    public float maxFallShakeIntensity = 8f;  // Max amplitude shake
    public float maxFallShakeFrequency = 10f; // Max frequency shake
    public float fallShakeDuration = 0.8f;    // Increase duration for better impact
    public float fallShakeFadeOut = 1.2f;     // Smooth fade-out duration



    [Header("Low Health Shake")]
    public float lowHealthShakeMax = 8f;
    public float lowHealthThreshold = 50f;
    public float minHealthShakeFrequency = 0.01f;

    private bool isLowHealthShaking = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Assign standing camera
    public void AssignCamera(CinemachineVirtualCamera cam)
    {
        standingCamera = cam;
        standingNoise = standingCamera?.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        if (activeCamera == null)
        {
            activeCamera = standingCamera;
            noise = standingNoise; // Ensure default noise is assigned
        }
    }

    public void AssignCrouchCamera(CinemachineVirtualCamera cam)
    {
        crouchingCamera = cam;
        crouchingNoise = crouchingCamera?.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }


    // Toggle between standing and crouching camera
    public void UseCrouchCamera(bool useCrouch)
    {
        if (useCrouch && crouchingCamera != null)
        {
            activeCamera = crouchingCamera;
        }
        else
        {
            activeCamera = standingCamera;
        }
        UpdateNoiseComponent();
    }

    // Updates the current active camera's noise component
    private void UpdateNoiseComponent()
    {
        noise = activeCamera?.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        if (noise == null)
        {
            Debug.LogError("Camera Noise Component Not Found!");
        }
    }

    // Walking & Crouching Bob
    public void SetHeadBob(bool isMoving, bool isRunning, bool isCrouching)
    {
        if (!isMoving)
        {
            targetBobIntensity = 0f;
            targetBobFrequency = 0f;
        }
        else if (isRunning)
        {
            targetBobIntensity = runningBobIntensity;
            targetBobFrequency = runningBobFrequency;
        }
        else if (isCrouching)
        {
            targetBobIntensity = crouchBobIntensity;
            targetBobFrequency = crouchBobFrequency;
        }
        else
        {
            targetBobIntensity = walkingBobIntensity;
            targetBobFrequency = walkingBobFrequency;
        }
    }

    private void Update()
    {
        if (noise == null) return;

        // Smooth transition for bobbing effect
        noise.m_AmplitudeGain = Mathf.Lerp(noise.m_AmplitudeGain, targetBobIntensity, Time.deltaTime * 5f);
        noise.m_FrequencyGain = Mathf.Lerp(noise.m_FrequencyGain, targetBobFrequency, Time.deltaTime * 5f);
    }

    // Fall Damage Shake
    public void ShakeOnFall(float damage)
    {
        if (standingNoise == null || crouchingNoise == null) return;

        // ✅ Improved scaling to make the shake feel more proportional
        float intensity = Mathf.Clamp(damage / 10f, 2f, maxFallShakeIntensity);
        float frequency = Mathf.Clamp(damage / 6f, 2f, maxFallShakeFrequency);

        // ✅ Run shake on both cameras with a smoother fade-out
        StartCoroutine(ShakeRoutine(standingNoise, fallShakeDuration, intensity, frequency, fallShakeFadeOut));
        StartCoroutine(ShakeRoutine(crouchingNoise, fallShakeDuration, intensity, frequency, fallShakeFadeOut));
    }


    // Low Health Shake
    public void UpdateLowHealthShake(float currentHealth)
    {
        if (standingNoise == null || crouchingNoise == null) return;

        if (currentHealth <= lowHealthThreshold)
        {
            float intensity = Mathf.Lerp(0f, lowHealthShakeMax, 1f - (currentHealth / lowHealthThreshold));
            float frequency = Mathf.Clamp(minHealthShakeFrequency, 0.01f, 2f);

            standingNoise.m_AmplitudeGain = intensity;
            standingNoise.m_FrequencyGain = frequency;

            crouchingNoise.m_AmplitudeGain = intensity;
            crouchingNoise.m_FrequencyGain = frequency;

            isLowHealthShaking = true;
        }
        else if (isLowHealthShaking)
        {
            StartCoroutine(ResetShake(standingNoise, 1f));
            StartCoroutine(ResetShake(crouchingNoise, 1f));
            isLowHealthShaking = false;
        }
    }

    private IEnumerator ShakeRoutine(CinemachineBasicMultiChannelPerlin camNoise, float duration, float intensity, float frequency, float fadeOutTime)
    {
        if (camNoise == null) yield break;

        float elapsedTime = 0f;
        float startAmplitude = camNoise.m_AmplitudeGain;
        float startFrequency = camNoise.m_FrequencyGain;

        // ✅ Increase amplitude & frequency gradually over half of the duration
        while (elapsedTime < duration / 2f)
        {
            float t = elapsedTime / (duration / 2f);
            camNoise.m_AmplitudeGain = Mathf.Lerp(startAmplitude, intensity, t);
            camNoise.m_FrequencyGain = Mathf.Lerp(startFrequency, frequency, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(duration / 2f); // ✅ Hold the shake for a bit

        // ✅ Smoothly reduce the shake effect over `fadeOutTime`
        elapsedTime = 0f;
        while (elapsedTime < fadeOutTime)
        {
            float t = elapsedTime / fadeOutTime;
            camNoise.m_AmplitudeGain = Mathf.Lerp(intensity, 0, t);
            camNoise.m_FrequencyGain = Mathf.Lerp(frequency, 0, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // ✅ Ensure values reset to 0
        camNoise.m_AmplitudeGain = 0;
        camNoise.m_FrequencyGain = 0;
    }


    private IEnumerator ResetShake(CinemachineBasicMultiChannelPerlin camNoise, float fadeDuration)
    {
        if (camNoise == null) yield break;

        float startAmplitude = camNoise.m_AmplitudeGain;
        float startFrequency = camNoise.m_FrequencyGain;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            camNoise.m_AmplitudeGain = Mathf.Lerp(startAmplitude, 0, elapsedTime / fadeDuration);
            camNoise.m_FrequencyGain = Mathf.Lerp(startFrequency, 0, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        camNoise.m_AmplitudeGain = 0;
        camNoise.m_FrequencyGain = 0;
    }

}
