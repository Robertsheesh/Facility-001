using System.Collections;
using UnityEngine;

public class LightFlicker : MonoBehaviour
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
            lampMaterial = lampRenderer.material; // Get the material from the renderer
            originalEmissionColor = lampMaterial.GetColor("_EmissionColor"); // Store the original emission color
        }

        // Start the flicker coroutine
        StartCoroutine(FlickerLight());
    }

    // Coroutine to handle light flickering
    private IEnumerator FlickerLight()
    {
        while (true)
        {
            // Wait for a random interval between flickers
            float waitTime = Random.Range(minFlickerInterval, maxFlickerInterval);
            yield return new WaitForSeconds(waitTime);

            // Start flickering
            StartCoroutine(FlickerEffect());
        }
    }

    // Coroutine to simulate the flicker effect
    private IEnumerator FlickerEffect()
    {
        isFlickering = true;

        for (int i = 0; i < flickerCount; i++)
        {
            // Play the flicker sound
            if (flickerSound != null)
            {
                flickerSound.Play();  // Play the flicker sound when light flickers
            }

            // Turn off the light and change the material emission to black (flicker off)
            lightSource.enabled = false;
            if (lampMaterial != null)
            {
                SetLampEmissionColor(Color.black); // Set the emission color to black
            }
            yield return new WaitForSeconds(flickerDuration);

            // Turn the light back on and restore the original emission color
            lightSource.enabled = true;
            if (lampMaterial != null)
            {
                SetLampEmissionColor(originalEmissionColor); // Restore the original emission color
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
    }
}
