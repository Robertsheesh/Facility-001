using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public AudioSource audioSource;       // Reference to the AudioSource component
    public AudioClip ambientMusic;        // The ambient music clip
    public AudioClip newTrack;            // The new music track to play after card is picked up
    public float fadeDuration = 1.0f;     // Duration of fade in/out

    private void Start()
    {
        PlayAmbientMusic();
    }

    public void PlayAmbientMusic()
    {
        if (audioSource != null && ambientMusic != null)
        {
            audioSource.volume = 0.4f; // Set initial volume
            StartCoroutine(FadeInMusic(ambientMusic));
        }
    }

    // Method to switch to a new track with seamless transition
    public void SwitchToNewTrack()
    {
        if (newTrack != null)
        {
            StartCoroutine(SwitchTrackSeamlessly(newTrack));
        }
    }

    // Coroutine to switch tracks with overlapping fade-out and fade-in for seamless transition
    private IEnumerator SwitchTrackSeamlessly(AudioClip newClip)
    {
        float elapsedTime = 0;
        float startVolume = audioSource.volume;

        // Start playing the new clip, but with volume set to 0
        AudioSource newAudioSource = gameObject.AddComponent<AudioSource>();  // Add a new AudioSource for the new track
        newAudioSource.clip = newClip;
        newAudioSource.volume = 0;
        newAudioSource.Play();

        // Fade out the current track while fading in the new track simultaneously
        while (elapsedTime < fadeDuration)
        {
            float t = elapsedTime / fadeDuration;

            // Gradually lower the volume of the current track
            audioSource.volume = Mathf.Lerp(startVolume, 0, t);

            // Gradually increase the volume of the new track
            newAudioSource.volume = Mathf.Lerp(0, 0.35f, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure final volumes are set
        audioSource.volume = 0;
        newAudioSource.volume = 0.4f;

        // Stop the current track
        audioSource.Stop();

        // Destroy the old AudioSource if needed, or just reset it
        Destroy(audioSource);

        // Set the new AudioSource as the main one
        audioSource = newAudioSource;
    }

    // Fade in the ambient music or any given music clip
    private IEnumerator FadeInMusic(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.volume = 0;
        audioSource.Play();

        float targetVolume = 0.4f;
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
