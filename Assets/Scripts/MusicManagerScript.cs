using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public AudioSource audioSource; // Reference to the AudioSource component
    public AudioClip ambientMusic; // The ambient music clip

    public float fadeDuration = 1.0f; // Duration of fade in/out

    private void Start()
    {
        PlayAmbientMusic();
    }

    public void PlayAmbientMusic()
    {
        if (audioSource != null && ambientMusic != null)
        {
            audioSource.volume = 0.18f; // Set initial volume (lower than the default full volume)
            StartCoroutine(FadeInMusic());
        }
    }

    // Fade in the ambient music
    private IEnumerator FadeInMusic()
    {
        audioSource.clip = ambientMusic;
        audioSource.volume = 0;
        audioSource.Play();

        float targetVolume = 0.18f; // Lower the target volume
        float elapsedTime = 0;
        while (elapsedTime < fadeDuration)
        {
            audioSource.volume = Mathf.Lerp(0, targetVolume, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        audioSource.volume = targetVolume; // Set to final target volume
    }

    // Call this method if you want to stop the music with a fade-out effect
    public void StopMusic()
    {
        StartCoroutine(FadeOutMusic());
    }

    // Fade out the music
    private IEnumerator FadeOutMusic()
    {
        float startVolume = audioSource.volume;

        float elapsedTime = 0;
        while (elapsedTime < fadeDuration)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume; // Reset volume for next time
    }
}
