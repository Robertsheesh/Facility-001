using UnityEngine;

public class CameraShake : MonoBehaviour
{
    // Shake duration in seconds
    public float shakeDuration = 0.5f;

    // Intensity of the shake effect
    public float shakeAmount = 0.7f;

    // How fast the shake effect dampens
    public float decreaseFactor = 1.0f;

    private Vector3 originalPos;
    private float currentShakeDuration = 0f;

    void OnEnable()
    {
        // Store the camera's initial position
        originalPos = transform.localPosition;
    }

    void Update()
    {
        // Check if there is still time to shake
        if (currentShakeDuration > 0)
        {
            // Shake the camera by adding a random offset to its position
            transform.localPosition = originalPos + Random.insideUnitSphere * shakeAmount;

            // Decrease the shake duration over time
            currentShakeDuration -= Time.deltaTime * decreaseFactor;
        }
        else
        {
            // Reset the camera's position to its original state once shaking is done
            currentShakeDuration = 0f;
            transform.localPosition = originalPos;
        }
    }

    // This method can be called to trigger the shake effect
    public void TriggerShake(float duration)
    {
        currentShakeDuration = duration;
    }
}
