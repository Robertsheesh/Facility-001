using UnityEngine;

public class VentTrigger : MonoBehaviour
{
    public Animator ventAnimator;             // Animator for vent open animation
    public float openImpactThreshold = 5f;    // Minimum impact force required to open the vent
    public AudioSource ventOpenSound;         // Sound to play when the vent opens
    private bool isOpen = false;              // Tracks if the vent is already open

    // This method is called when a collision happens on this GameObject
    private void OnCollisionEnter(Collision collision)
    {
        // Check if the vent is already open
        if (isOpen) return;

        // Calculate the impact force using the collision's relative velocity
        float impactForce = collision.relativeVelocity.magnitude;

        // Check if the impact force is greater than or equal to the threshold
        if (impactForce >= openImpactThreshold)
        {
            OpenVent();
        }
    }

    private void OpenVent()
    {
        isOpen = true;

        // Play vent open sound, if assigned
        if (ventOpenSound != null)
        {
            ventOpenSound.Play();
        }

        // Trigger vent open animation if an animator is assigned
        if (ventAnimator != null)
        {
            ventAnimator.SetTrigger("Open");  // Assumes the Animator has a "Open" trigger parameter
        }
        else
        {
            // If no animator, we can disable the vent collider or apply a force to make it fall, etc.
            GetComponent<Collider>().enabled = false;
        }
    }
}
