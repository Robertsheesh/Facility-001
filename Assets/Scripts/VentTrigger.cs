using UnityEngine;
using System.Collections;

public class VentTrigger : MonoBehaviour
{
    public float openImpactThreshold = 5f; // Minimum force required to make the vent fall
    public AudioSource ventOpenSound; // Sound played when the vent falls
    public AudioSource ventImpactSound; // Sound played when the vent hits a surface
    public bool isOpen = false; // Tracks if the vent is already open
    private Rigidbody rb;
    private Collider ventCollider;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        ventCollider = GetComponent<Collider>();

        if (rb == null)
        {
            Debug.LogError("VentTrigger requires a Rigidbody!");
        }
        else
        {
            rb.isKinematic = true; // ✅ Vent starts locked in place
        }

        if (ventCollider == null)
        {
            Debug.LogError("VentTrigger requires a Collider!");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // ✅ If already open, do nothing
        if (isOpen) return;

        // Calculate the impact force
        float impactForce = collision.relativeVelocity.magnitude;

        // ✅ If impact is strong enough, make the vent fall
        if (impactForce >= openImpactThreshold)
        {
            OpenVent();
        }
    }

    private void OpenVent()
    {
        isOpen = true; // ✅ Prevents multiple activations

        // 🎵 Play vent open sound, if assigned
        if (ventOpenSound != null)
        {
            ventOpenSound.Play();
        }

        // ✅ Enable physics so the vent falls
        if (rb != null)
        {
            rb.isKinematic = false; // ✅ Makes the vent fall
            rb.useGravity = true;   // ✅ Enables gravity so it drops naturally
        }

        // ⏳ Wait 2 seconds before making it pick-upable
        StartCoroutine(MakePickupable());

        Debug.Log("Vent has fallen!");
    }

    private IEnumerator MakePickupable()
    {
        yield return new WaitForSeconds(2f); // ✅ Waits before making it pickupable

        gameObject.tag = "Pickup"; // ✅ Now the player can pick it up!
        Debug.Log("Vent is now pickupable.");
    }

    private void OnCollisionStay(Collision collision)
    {
        // ✅ Play impact sound when vent collides with the environment
        if (ventImpactSound != null && collision.relativeVelocity.magnitude > 1.5f)
        {
            if (!ventImpactSound.isPlaying)
            {
                ventImpactSound.Play();
            }
        }
    }
}
