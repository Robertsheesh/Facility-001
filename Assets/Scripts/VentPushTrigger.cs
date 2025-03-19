using UnityEngine;
using System.Collections;

public class VentInteract : MonoBehaviour, IInteractable
{
    public float pushForce = 3f; // Adjust this force to control how far the vent moves
    public AudioSource ventSound; // 🎵 Sound for vent push
    public AudioSource ventImpactSound;

    private Rigidbody rb;
    private bool hasBeenPushed = false; // Ensure it only moves once

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Vent object needs a Rigidbody!");
        }
        else
        {
            rb.isKinematic = true; // Start as kinematic so it doesn't move before interaction
        }

        if (ventSound == null || ventImpactSound == null)
        {
            AudioSource[] audioSources = GetComponents<AudioSource>();
            if (audioSources.Length >= 2)
            {
                ventSound = audioSources[0];
                ventImpactSound = audioSources[1];
            }
            else
            {
                Debug.LogWarning("Missing AudioSources! Assign them in the inspector.");
            }
        }
    }

    public void Interact()
    {
        if (!hasBeenPushed)
        {
            hasBeenPushed = true; // Prevent multiple interactions
            rb.isKinematic = false; // Enable physics
            rb.useGravity = true;   // Enable gravity

            // Apply a force away from the player
            Vector3 pushDirection = (transform.position - Camera.main.transform.position).normalized;
            rb.AddForce(pushDirection * pushForce, ForceMode.Impulse);

            // 🎵 Play vent push sound
            if (ventSound != null)
            {
                ventSound.Play();
            }

            // ❌ Remove tag immediately
            gameObject.tag = "Untagged";

            // ⏳ Start coroutine to re-tag after 2 seconds
            StartCoroutine(SetTagToPickup());

            Debug.Log("Vent has been pushed!");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 🎵 Play impact sound *every time* the vent collides with anything
        if (ventImpactSound != null && collision.relativeVelocity.magnitude > 0.5f) // Avoid micro-collisions
        {
            ventImpactSound.Play();
        }
    }

    IEnumerator SetTagToPickup()
    {
        yield return new WaitForSeconds(2f); // ⏳ Wait for 2 seconds
        gameObject.tag = "Pickup"; // ✅ Set tag to "Pickup"
        Debug.Log("Vent is now a Pickup object!");
    }
}
