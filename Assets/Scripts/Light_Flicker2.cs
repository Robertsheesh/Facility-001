using System.Collections;
using UnityEngine;

public class LightFlicker2 : MonoBehaviour
{
    public Light lightSource;              // Reference to the Light component
    public AudioSource flickerSound;       // Reference to the AudioSource for flicker sound
    public Renderer lampRenderer;          // Renderer of the lamp object (which has the emissive material)
    public float minFlickerInterval = 5f;  // Minimum time between flickers (in seconds)
    public float maxFlickerInterval = 10f; // Maximum time between flickers (in seconds)
    public float flickerDuration = 0.1f;   // Duration of each flicker (in seconds)
    public int flickerCount = 3;           // Number of flickers during each flicker event

    private bool isFlickering = false;
    private Color originalEmissionColor;   // Store the original emission color of the lamp's material
    private Material lampMaterial;         // The material we will modify

    void Start()
    {
        // Get the material of the lamp and store the original emission color
        if (lampRenderer != null)
        {
            lampMaterial = lampRenderer.sharedMaterial; // Use sharedMaterial to modify the actual material
            originalEmissionColor = lampMaterial.GetColor("_EmissionColor"); // Store the original emission color
        }

        // **Ensure the light starts OFF**
        lightSource.enabled = false;
        if (lampMaterial != null)
        {
            SetLampEmissionColor(Color.black); // Start with no emission
        }

        // Start the flicker coroutine
        StartCoroutine(FlickerLight());
    }

    // Coroutine to handle light flickering
    private IEnumerator FlickerLight()
    {
        while (true)
        {
            // Wait for a random interval before flickering ON
            float waitTime = Random.Range(minFlickerInterval, maxFlickerInterval);
            yield return new WaitForSeconds(waitTime);

            // Start flickering
            StartCoroutine(FlickerEffect());
        }
    }

    // Coroutine to simulate the flicker effect (turns ON and then OFF)
    private IEnumerator FlickerEffect()
    {
        isFlickering = true;

        for (int i = 0; i < flickerCount; i++)
        {
            // Play the flicker sound when turning ON
            if (flickerSound != null)
            {
                flickerSound.Play();
            }

            // **Turn the light ON and restore original emission color**
            lightSource.enabled = true;
            if (lampMaterial != null)
            {
                SetLampEmissionColor(originalEmissionColor);
            }
            yield return new WaitForSeconds(flickerDuration);

            // **Turn the light OFF and remove emission**
            lightSource.enabled = false;
            if (lampMaterial != null)
            {
                SetLampEmissionColor(Color.black);
            }
            yield return new WaitForSeconds(flickerDuration);
        }

        isFlickering = false;
    }

    // Method to set the emission color of the lamp
    private void SetLampEmissionColor(Color color)
    {
        lampMaterial.SetColor("_EmissionColor", color);
        if (color == Color.black)
        {
            lampMaterial.DisableKeyword("_EMISSION"); // Disable emission if the color is black
        }
        else
        {
            lampMaterial.EnableKeyword("_EMISSION"); // Enable emission for non-black colors
        }

        // Apply the change to global illumination
        DynamicGI.SetEmissive(lampRenderer, color);
    }
}
