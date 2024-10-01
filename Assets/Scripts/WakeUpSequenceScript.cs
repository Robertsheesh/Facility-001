using UnityEngine;
using Cinemachine;

public class WakeUpSequence : MonoBehaviour
{
    public CinemachineVirtualCamera bedCamera;       // Cinemachine camera for the wake-up view
    public CinemachineVirtualCamera gameplayCamera;  // Cinemachine camera for gameplay view
    public float wakeUpDuration = 5.0f;              // Time in seconds before switching to gameplay
    public float initialDelay = 0.1f;                // Delay before bed camera activates to avoid immediate switch

    private float timer = 0.0f;
    private bool isWakingUp = true;

    void Start()
    {
        // Ensure gameplay camera is inactive at the start
        gameplayCamera.Priority = 0;

        // Start with a slight delay to ensure bed camera activates properly
        Invoke("ActivateBedCamera", initialDelay);

        DisablePlayerControl();  // Disable player control during wake-up
    }

    void Update()
    {
        if (isWakingUp)
        {
            timer += Time.deltaTime;

            // After wake-up duration, switch to gameplay camera
            if (timer > wakeUpDuration)
            {
                SwitchToGameplayCamera();
            }
        }
    }

    private void ActivateBedCamera()
    {
        // Ensure the bed camera is active after a slight delay
        bedCamera.Priority = 10;  // Bed camera active
    }

    private void SwitchToGameplayCamera()
    {
        bedCamera.Priority = 0;             // Disable bed camera
        gameplayCamera.Priority = 10;       // Enable gameplay camera

        isWakingUp = false;
        EnablePlayerControl();  // Re-enable player control after switching
    }

    private void DisablePlayerControl()
    {
        // Disable player movement and input (replace with your specific code for your player controller)
        // Example (replace with your controller script):
        // SC_FPSController playerController = FindObjectOfType<SC_FPSController>();
        // playerController.canMove = false;
    }

    private void EnablePlayerControl()
    {
        // Re-enable player movement and input after wake-up
        // Example (replace with your controller script):
        // SC_FPSController playerController = FindObjectOfType<SC_FPSController>();
        // playerController.canMove = true;
    }
}
