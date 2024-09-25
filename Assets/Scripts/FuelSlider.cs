using UnityEngine;
using UnityEngine.UI;

public class FuelSliderManager : MonoBehaviour
{
    public Slider fuelSlider;          // Reference to the UI slider
    public LeverManager leverManager;  // Reference to the LeverManager

    void Start()
    {
        if (fuelSlider == null)
        {
            Debug.LogError("FuelSlider reference is missing!");
        }

        if (leverManager == null)
        {
            Debug.LogError("LeverManager reference is missing!");
        }
    }

    void Update()
    {
        // Ensure the slider and leverManager are assigned
        if (fuelSlider != null && leverManager != null)
        {
            // Update the slider value based on current fuel in LeverManager
            fuelSlider.value = leverManager.currentFuelTime / leverManager.fuelMaxTime;
        }
    }
}
