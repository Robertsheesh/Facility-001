using UnityEngine;

public class DustTrigger : MonoBehaviour
{
    public ParticleSystem dustParticles;

    private bool hasTriggered = false; // Prevent multiple activations

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Trigger Entered by: {other.gameObject.name}"); // ✅ Debugging

        if (hasTriggered) return; // Avoid multiple activations

        if (other.CompareTag("Player")) // Ensure Player has the correct tag
        {
            hasTriggered = true;

            if (dustParticles != null)
            {
                dustParticles.gameObject.SetActive(true); // ✅ Ensure it is active
                dustParticles.Play(); // ✅ Play the particles
                Debug.Log("✅ Dust Particles Activated!");
            }
            else
            {
                Debug.LogError("❌ Dust Particle System is NOT Assigned!");
            }
        }
    }
}
