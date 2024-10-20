using UnityEngine;

public class ThrownObject : MonoBehaviour
{
    public AudioSource audioSource;   // Reference to the AudioSource component
    public AudioClip impactSound;     // The sound clip to play when the object hits the ground

    void Start()
    {
        // Ensure the AudioSource is assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        // Optionally assign the impact sound from the AudioSource component (if already set in the Inspector)
        if (audioSource != null && audioSource.clip != null)
        {
            impactSound = audioSource.clip;
        }
    }

    // Detect collision with the ground or other objects
    private void OnCollisionEnter(Collision collision)
    {
        // Calculate impact strength based on the collision force
        float impactForce = collision.relativeVelocity.magnitude;

        // Adjust the volume based on the impact force (0.0 to 1.0)
        float volume = Mathf.Clamp(impactForce / 10f, 0.1f, 1.0f);  // Scale the volume based on impact strength

        if (audioSource != null && impactSound != null)
        {
            // Play the sound with adjusted volume
            audioSource.PlayOneShot(impactSound, volume);
        }
    }
}
