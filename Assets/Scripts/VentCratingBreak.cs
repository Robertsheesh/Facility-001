using UnityEngine;
using System.Collections;

public class VentCratingTrigger : MonoBehaviour
{
    public Animator cratingAnimator; // Assign the Animator in the Inspector
    public string fallTriggerName = "StartFall"; // Name of the Animator trigger
    public AudioSource creakingSound; // Optional: Sound before breaking
    public AudioSource breakSound; // The actual breaking sound

    private bool hasTriggered = false; // Prevents multiple activations

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return; // Prevents multiple activations
        if (other.CompareTag("Player"))
        {
            hasTriggered = true; // ✅ Prevent multiple triggers
            StartCoroutine(StartCratingFall());
        }
    }

    private IEnumerator StartCratingFall()
    {
        if (creakingSound != null)
        {
            creakingSound.Play(); // 🎵 Play creaking sound before breaking
        }

        Debug.Log("Crating is creaking...");

        // Wait for 1 second before breaking
        yield return new WaitForSeconds(1f);

        if (breakSound != null)
        {
            breakSound.Play(); // 🎵 Play breaking sound when falling
        }

        Debug.Log("Crating is breaking...");

        // Trigger the animation
        cratingAnimator.SetTrigger(fallTriggerName);
    }
}
