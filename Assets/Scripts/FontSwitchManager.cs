using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;

public class FontSwitchManager : MonoBehaviour
{
    public Font originalFont;         // The normal font (English text)
    public Font alienFont;            // The alien/cryptic font
    public float switchInterval = 180f;        // Interval in seconds (e.g., 3 minutes)
    public float switchDuration = 1f;          // Duration for the font switch (in seconds)

    public List<Text> specificUITextElements = new List<Text>(); // Specific Text elements to switch

    public AudioSource glitchSoundSource;      // The AudioSource component to play glitch sounds
    public AudioClip glitchSound1;             // First glitch sound
    public AudioClip glitchSound2;             // Second glitch sound

    public PostProcessVolume postProcessVolume;  // Reference to the Post Processing Volume
    public float chromaticAberrationIntensity = 1f;  // Intensity of chromatic aberration during glitch
    public float chromaticAberrationSpeed = 2f;      // Speed at which chromatic aberration fades in and out
    public float bloomIntensity = 2f;                // Intensity of bloom effect during glitch
    public float bloomSpeed = 2f;                    // Speed at which bloom fades in and out

    private ChromaticAberration chromaticAberration; // Reference to the Chromatic Aberration effect
    private Bloom bloomEffect;                       // Reference to the Bloom effect

    public CinemachineVirtualCamera playerCamera;    // Reference to the player's Cinemachine camera
    public CinemachineVirtualCamera computerCamera;  // Reference to the computer's Cinemachine camera
    public float cameraShakeAmplitude = 2f;          // Camera shake intensity
    public float cameraShakeFrequency = 2f;          // Camera shake frequency
    public float shakeDuration = 0.5f;               // Duration of the camera shake

    private CinemachineBasicMultiChannelPerlin playerCameraNoise;  // Noise component for player camera shake
    private CinemachineBasicMultiChannelPerlin computerCameraNoise; // Noise component for computer camera shake

    private bool isPlayerInControlRoom = false; // Track if the player is in the Control Room
    private bool useFirstSound = true;         // Toggle to alternate between sounds
    private bool glitchIsActive = false;       // Ensure glitch only happens when player is in the Control Room

    void Start()
    {
        // Initialize post-processing effects and camera shake
        InitializePostProcessingEffects();
        InitializeCameraShake();
    }

    void Update()
    {
        if (isPlayerInControlRoom && !glitchIsActive)
        {
            StartCoroutine(FontSwitchRoutine());
        }
    }

    // This function is called by TriggerZone.cs when the player enters a control room trigger
    public void PlayerEnteredControlRoom()
    {
        isPlayerInControlRoom = true;
    }

    // This function is called by TriggerZone.cs when the player exits a control room trigger
    public void PlayerExitedControlRoom()
    {
        isPlayerInControlRoom = false;
    }

    // Coroutine that switches the font and applies post-processing at intervals
    private IEnumerator FontSwitchRoutine()
    {
        glitchIsActive = true;  // Prevent re-triggering during glitch sequence
        yield return new WaitForSeconds(switchInterval); // Wait for the specified interval

        // Play the glitch sound
        PlayGlitchSound();

        // Switch to alien font and apply post-processing effects
        ChangeFont(alienFont);
        StartCoroutine(ApplyChromaticAberration(chromaticAberrationIntensity, chromaticAberrationSpeed));
        StartCoroutine(ApplyBloom(bloomIntensity, bloomSpeed));

        // Apply camera shake to both player and computer cameras
        StartCoroutine(ApplyCameraShake(playerCameraNoise));
        StartCoroutine(ApplyCameraShake(computerCameraNoise));

        yield return new WaitForSeconds(switchDuration); // Wait for the switch duration

        // Switch back to original font and remove post-processing effects
        ChangeFont(originalFont);
        StartCoroutine(ApplyChromaticAberration(0f, chromaticAberrationSpeed));
        StartCoroutine(ApplyBloom(0f, bloomSpeed));

        glitchIsActive = false;
    }

    // Initialize the Chromatic Aberration, Bloom effects, and Camera Shake from the post-processing profile
    private void InitializePostProcessingEffects()
    {
        if (postProcessVolume != null)
        {
            postProcessVolume.profile.TryGetSettings(out chromaticAberration);
            postProcessVolume.profile.TryGetSettings(out bloomEffect);

            // Start with no effects
            if (chromaticAberration != null) chromaticAberration.intensity.value = 0f;
            if (bloomEffect != null) bloomEffect.intensity.value = 0f;
        }
    }

    // Initialize the camera shake noise for both player and computer cameras
    private void InitializeCameraShake()
    {
        if (playerCamera != null)
        {
            playerCameraNoise = playerCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

            if (playerCameraNoise != null)
            {
                playerCameraNoise.m_AmplitudeGain = 0f;  // Start with no shake
                playerCameraNoise.m_FrequencyGain = 0f;
            }
        }

        if (computerCamera != null)
        {
            computerCameraNoise = computerCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

            if (computerCameraNoise != null)
            {
                computerCameraNoise.m_AmplitudeGain = 0f;  // Start with no shake
                computerCameraNoise.m_FrequencyGain = 0f;
            }
        }
    }

    // Gradually apply chromatic aberration intensity
    private IEnumerator ApplyChromaticAberration(float targetIntensity, float speed)
    {
        if (chromaticAberration == null) yield break;

        float startIntensity = chromaticAberration.intensity.value;
        float elapsedTime = 0f;

        while (elapsedTime < 1f)
        {
            chromaticAberration.intensity.value = Mathf.Lerp(startIntensity, targetIntensity, elapsedTime);
            elapsedTime += Time.deltaTime * speed;
            yield return null;
        }

        chromaticAberration.intensity.value = targetIntensity;
    }

    // Gradually apply bloom intensity
    private IEnumerator ApplyBloom(float targetIntensity, float speed)
    {
        if (bloomEffect == null) yield break;

        float startIntensity = bloomEffect.intensity.value;
        float elapsedTime = 0f;

        while (elapsedTime < 1f)
        {
            bloomEffect.intensity.value = Mathf.Lerp(startIntensity, targetIntensity, elapsedTime);
            elapsedTime += Time.deltaTime * speed;
            yield return null;
        }

        bloomEffect.intensity.value = targetIntensity;
    }

    // Apply camera shake using the Cinemachine Noise component
    private IEnumerator ApplyCameraShake(CinemachineBasicMultiChannelPerlin cameraNoise)
    {
        if (cameraNoise == null) yield break;

        // Increase camera shake intensity
        float elapsedTime = 0f;
        while (elapsedTime < shakeDuration)
        {
            cameraNoise.m_AmplitudeGain = Mathf.Lerp(0f, cameraShakeAmplitude, elapsedTime / shakeDuration);
            cameraNoise.m_FrequencyGain = Mathf.Lerp(0f, cameraShakeFrequency, elapsedTime / shakeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Hold the shake for a brief moment
        yield return new WaitForSeconds(0.1f);

        // Gradually reduce camera shake intensity
        elapsedTime = 0f;
        while (elapsedTime < shakeDuration)
        {
            cameraNoise.m_AmplitudeGain = Mathf.Lerp(cameraShakeAmplitude, 0f, elapsedTime / shakeDuration);
            cameraNoise.m_FrequencyGain = Mathf.Lerp(cameraShakeFrequency, 0f, elapsedTime / shakeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure shake values return to zero
        cameraNoise.m_AmplitudeGain = 0f;
        cameraNoise.m_FrequencyGain = 0f;
    }

    // Change the font of specific Text elements
    private void ChangeFont(Font newFont)
    {
        foreach (Text textElement in specificUITextElements)
        {
            if (textElement != null)
            {
                textElement.font = newFont;
            }
        }
    }

    // Play alternating glitch sounds
    private void PlayGlitchSound()
    {
        if (glitchSoundSource != null)
        {
            if (useFirstSound)
            {
                if (glitchSound1 != null)
                {
                    glitchSoundSource.PlayOneShot(glitchSound1);  // Play the first sound
                }
            }
            else
            {
                if (glitchSound2 != null)
                {
                    glitchSoundSource.PlayOneShot(glitchSound2);  // Play the second sound
                }
            }

            // Alternate the sound for next time
            useFirstSound = !useFirstSound;
        }
    }
}
