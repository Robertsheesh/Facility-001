using UnityEngine;
using System.Collections;  // Necessary for IEnumerator
using UnityEngine.Rendering.PostProcessing;

public class HazardAreaSound : MonoBehaviour
{
    public AudioSource radiationSound;  // Geiger counter or radiation sound
    public PostProcessVolume postProcessVolume;
    private Grain grainEffect;
    public float fadeOutDuration = 2.0f; // Duration for the sound fade out
    public float radiationDamagePerSecond = 2.0f;  // Health damage per second in the radiation zone

    private PlayerHealth playerHealth;
    private bool isInRadiationZone = false;  // Track whether the player is in the zone

    void Start()
    {
        // Attempt to retrieve the Grain effect from the post-process volume
        if (postProcessVolume.profile.TryGetSettings(out Grain grain))
        {
            grainEffect = grain;
            grainEffect.active = false;  // Ensure the grain effect is initially disabled
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))  // Ensure the player has a tag "Player"
        {
            playerHealth = other.GetComponent<PlayerHealth>();  // Retrieve PlayerHealth script

            if (playerHealth != null && !playerHealth.IsWearingSuit())  // Only play sound if not wearing suit
            {
                radiationSound.Play();
                StopAllCoroutines();  // Stop any ongoing fade coroutines
                radiationSound.volume = 1;  // Ensure full volume if re-entering quickly

                if (grainEffect != null)
                    grainEffect.active = true;  // Enable the grain effect

                isInRadiationZone = true;  // Player is in the radiation zone
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && playerHealth != null)
        {
            // Continuously damage the player while in the radiation zone if not wearing the suit
            if (isInRadiationZone && !playerHealth.IsWearingSuit())  // Check if the suit is equipped
            {
                playerHealth.ModifyHealth(-radiationDamagePerSecond * Time.deltaTime);
            }

            // Stop radiation sound if player is wearing the suit
            if (playerHealth.IsWearingSuit() && radiationSound.isPlaying)
            {
                StartCoroutine(FadeOutSound(radiationSound, fadeOutDuration));
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(FadeOutSound(radiationSound, fadeOutDuration));

            if (grainEffect != null)
                grainEffect.active = false;  // Disable the grain effect

            isInRadiationZone = false;  // Player has left the radiation zone
        }
    }

    IEnumerator FadeOutSound(AudioSource audioSource, float fadeTime)
    {
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / fadeTime;
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume;  // Reset volume to original for next play
    }
}
