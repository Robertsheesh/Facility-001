using UnityEngine;
using System.Collections;  // Necessary for IEnumerator

public class AudioZone : MonoBehaviour
{
    public AudioSource audioSource;  // Assign the AudioSource component in the Inspector
    public float fadeOutDuration = 2.0f; // Duration for the sound fade out

    void Start()
    {
        if (audioSource == null)
        {
            Debug.LogError("AudioSource is not assigned to AudioZone script.");
        }
    }

    // When the player enters the trigger zone
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Assuming the player is tagged "Player"
        {
            if (audioSource != null && !audioSource.isPlaying)
            {
                audioSource.Play();  // Start playing the audio when the player enters the zone
            }
        }
    }

    // When the player exits the trigger zone
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();  // Stop playing the audio when the player leaves the zone
            }
        }
    }

    IEnumerator FadeOutAudio(float fadeDuration)
    {
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / fadeDuration;
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume; // Reset volume for next play
    }
}
