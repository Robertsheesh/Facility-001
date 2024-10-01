using UnityEngine;
using Cinemachine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using System.Collections;

public class WakeUpSequence : MonoBehaviour
{
    public CinemachineVirtualCamera bedCamera;        // Cinemachine camera for the wake-up view
    public CinemachineVirtualCamera sittingCamera;    // Cinemachine camera for sitting on the bed view
    public CinemachineVirtualCamera gameplayCamera;   // Cinemachine camera for gameplay view
    public float wakeUpDuration = 5.0f;               // Time in seconds for bed view (including fade and blinking)
    public float sittingDuration = 3.0f;              // Time in seconds for sitting on the bed view
    public float initialBlackDuration = 2.0f;         // Duration for the pitch black screen at the start
    public float fadeInDuration = 7.0f;               // Duration for the gradual fade from black to view
    public float blinkDuration = 1.0f;                // Duration for each blink (total time for eyes to close and reopen)
    public PostProcessVolume postProcessVolume;       // Post-processing volume for the vignette effect

    public Image blackScreenImage;                    // Reference to the black screen UI Image
    public Text wakeUpText;                           // Reference to the UI text for "Press space bar to wake up"

    private Vignette vignetteEffect;                  // Vignette effect for blinking and fading from black
    private float timer = 0.0f;
    private bool isWakingUp = false;                  // Track if the wake-up sequence has started
    private bool isSitting = false;                   // Track if the player is sitting on the bed
    private int blinkCount = 0;                       // Number of blinks
    private bool isBlinking = false;                  // Track if currently blinking
    private bool hasFadedOut = false;                 // Track if the fade-out has completed
    private bool playerWantsToWakeUp = false;         // Track if the player pressed the space bar to wake up

    public float textFadeDuration = 2.0f;             // Duration for fading the wake-up text in and out
    public float lookAroundSpeed = 1.5f;              // Speed at which the camera looks around while sitting
    public float lookAngleRange = 15f;                // Angle range for looking left and right while sitting
    private bool isTextFading = true;                 // Track if the text is currently fading

    private Quaternion originalSittingRotation;       // The original rotation of the sitting camera

    void Start()
    {
        // Ensure gameplay and sitting cameras are inactive at the start
        gameplayCamera.Priority = 0;
        sittingCamera.Priority = 0;

        // Initialize vignette effect for wake-up
        InitializeVignetteEffect();

        // Ensure the black screen starts fully visible
        blackScreenImage.color = new Color(0f, 0f, 0f, 1f);  // Fully opaque black
        blackScreenImage.gameObject.SetActive(true);  // Ensure the black image is active

        // Store the original rotation of the sitting camera
        originalSittingRotation = sittingCamera.transform.localRotation;

        // Start with a slight delay to ensure bed camera activates properly
        Invoke("ActivateBedCamera", 0.1f);

        // Start the fade in/out cycle for the text
        StartCoroutine(FadeTextInOut());

        DisablePlayerControl();  // Disable player control during wake-up sequence
    }

    void Update()
    {
        // Check if the player pressed the space bar to wake up
        if (!playerWantsToWakeUp && Input.GetKeyDown(KeyCode.Space))
        {
            playerWantsToWakeUp = true;
            StopCoroutine(FadeTextInOut());  // Stop the text fade coroutine
            StartCoroutine(FadeOutText());  // Start fading out the text
            isWakingUp = true;  // Start the wake-up sequence
        }

        if (isWakingUp)
        {
            timer += Time.deltaTime;

            // First few seconds is pitch black, then start fade-in and blinking
            if (timer < initialBlackDuration)
            {
                // Keep the screen fully black for the first few seconds
                blackScreenImage.color = new Color(0f, 0f, 0f, 1f);
            }
            else if (timer < initialBlackDuration + fadeInDuration && !hasFadedOut)
            {
                // Gradually fade the black screen
                float fadeProgress = (timer - initialBlackDuration) / fadeInDuration;
                blackScreenImage.color = new Color(0f, 0f, 0f, Mathf.SmoothStep(1f, 0f, fadeProgress));

                // Check if fade-out is completed
                if (fadeProgress >= 1f)
                {
                    hasFadedOut = true;
                    blackScreenImage.gameObject.SetActive(false);
                }
            }
            else if (hasFadedOut && !isBlinking)
            {
                // Start applying the blinking effect after the fade-in
                ApplyBlinkingEffect();
            }

            // Switch to sitting camera after the wake-up duration
            if (!isSitting && timer > initialBlackDuration + fadeInDuration + wakeUpDuration)
            {
                SwitchToSittingCamera();
            }

            // While sitting, make the camera look left and right
            if (isSitting)
            {
                LookAroundWhileSitting();
            }

            // Switch to gameplay camera after sitting duration
            if (isSitting && timer > initialBlackDuration + fadeInDuration + wakeUpDuration + sittingDuration)
            {
                SwitchToGameplayCamera();
            }
        }
    }

    private void InitializeVignetteEffect()
    {
        // Find the vignette effect from the post-processing volume
        if (postProcessVolume.profile.TryGetSettings(out vignetteEffect))
        {
            vignetteEffect.intensity.value = 0f;  // Start with zero intensity, fade in/out for blinking later
        }
    }

    private void ApplyBlinkingEffect()
    {
        if (vignetteEffect != null && !isBlinking)
        {
            float blinkCycleDuration = (wakeUpDuration - fadeInDuration) / 3f;  // Duration for each blink cycle (even distribution within wakeUpDuration)
            if (blinkCount < 3 && timer > initialBlackDuration + fadeInDuration + blinkCount * blinkCycleDuration)
            {
                StartCoroutine(GradualBlink());
            }
        }
    }

    private IEnumerator GradualBlink()
    {
        isBlinking = true;
        blinkCount++;

        // Close eyes (increase vignette intensity)
        float elapsedTime = 0f;
        while (elapsedTime < blinkDuration / 2f)
        {
            vignetteEffect.intensity.value = Mathf.Lerp(0f, 0.75f, elapsedTime / (blinkDuration / 2f));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Pause briefly with eyes closed
        yield return new WaitForSeconds(0.1f);

        // Open eyes (decrease vignette intensity)
        elapsedTime = 0f;
        while (elapsedTime < blinkDuration / 2f)
        {
            vignetteEffect.intensity.value = Mathf.Lerp(0.75f, 0f, elapsedTime / (blinkDuration / 2f));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        isBlinking = false;
    }

    // Coroutine to fade the text in and out
    private IEnumerator FadeTextInOut()
    {
        while (!playerWantsToWakeUp)
        {
            // Fade in
            float elapsedTime = 0f;
            while (elapsedTime < textFadeDuration)
            {
                float alpha = Mathf.Lerp(0f, 1f, elapsedTime / textFadeDuration);
                wakeUpText.color = new Color(wakeUpText.color.r, wakeUpText.color.g, wakeUpText.color.b, alpha);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Fade out
            elapsedTime = 0f;
            while (elapsedTime < textFadeDuration)
            {
                float alpha = Mathf.Lerp(1f, 0f, elapsedTime / textFadeDuration);
                wakeUpText.color = new Color(wakeUpText.color.r, wakeUpText.color.g, wakeUpText.color.b, alpha);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
    }

    // Coroutine to fade the text out when the player wakes up
    private IEnumerator FadeOutText()
    {
        float elapsedTime = 0f;
        while (elapsedTime < textFadeDuration)
        {
            float alpha = Mathf.Lerp(wakeUpText.color.a, 0f, elapsedTime / textFadeDuration);
            wakeUpText.color = new Color(wakeUpText.color.r, wakeUpText.color.g, wakeUpText.color.b, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        wakeUpText.gameObject.SetActive(false);  // Disable the text after it fully fades out
    }

    private void LookAroundWhileSitting()
    {
        // Animate the sitting camera's local rotation to look left and right
        float angle = Mathf.Sin(Time.time * lookAroundSpeed) * lookAngleRange;
        sittingCamera.transform.localRotation = originalSittingRotation * Quaternion.Euler(0, angle, 0);
    }

    private void ActivateBedCamera()
    {
        bedCamera.Priority = 10;  // Activate bed camera
    }

    private void SwitchToSittingCamera()
    {
        bedCamera.Priority = 0;              // Disable bed camera
        sittingCamera.Priority = 10;         // Enable sitting on bed camera
        isSitting = true;                    // Mark that we are now in the sitting phase
    }

    private void SwitchToGameplayCamera()
    {
        sittingCamera.Priority = 0;          // Disable sitting camera
        gameplayCamera.Priority = 10;        // Enable gameplay camera

        isWakingUp = false;
        EnablePlayerControl();  // Re-enable player control after switching
    }

    private void DisablePlayerControl()
    {
       // Disable player movement and input (replace with your specific code for your player controller)
    }

    private void EnablePlayerControl()
    {
        // Re-enable player movement and input after wake-up
    }
}
