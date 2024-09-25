using UnityEngine;
using UnityEngine.UI;

public class MachineStatePanel : MonoBehaviour
{
    public Slider timerBar;  // The UI slider for the power
    public float totalTime = 30f;  // Total power time
    private float timeLeft;

    void Start()
    {
        ResetPower();  // Initializes power slider to full
    }

    void Update()
    {
        // Update the slider only if power is depleting
        if (timeLeft > 0 && timerBar != null)
        {
            UpdateTimerBar();
        }
    }

    // Start power depletion after fuel runs out
    public void StartPowerDepletion()
    {
        timeLeft = totalTime;  // Reset the timer to the total power time
        UpdateTimerBar();  // Ensure the slider reflects the current value
    }

    // Decrease power gradually over time
    public void DecreasePower(float deltaTime)
    {
        if (timeLeft > 0)
        {
            timeLeft -= deltaTime;
            UpdateTimerBar();  // Update the slider based on the new power value
        }
    }

    // Check if power is fully depleted
    public bool IsPowerEmpty()
    {
        return timeLeft <= 0;
    }

    // Reset the power to full (after emergency or refuel)
    public void ResetPower()
    {
        timeLeft = totalTime;  // Reset to full power
        UpdateTimerBar();  // Reflect the full power in the UI slider
    }

    // Helper method to update the UI slider
    private void UpdateTimerBar()
    {
        if (timerBar != null)
        {
            timerBar.value = timeLeft / totalTime;  // Ensure the slider shows the correct percentage
        }
    }
}
