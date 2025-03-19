using UnityEngine;

public class DoorInteraction : MonoBehaviour, IInteractable
{
    public Transform doorTransform;       // The door that will open/close
    public float openAngle = 90f;         // The angle the door will open to
    public float doorOpenSpeed = 2f;      // Speed of the door opening
    public float closeSpeedMultiplier = 2f; // Multiplier for faster closing speed
    public bool isOpen = true;            // Whether the door is currently open

    private Quaternion closedRotation;    // Store the original rotation of the door
    private Quaternion openRotation;      // The target rotation for the door when opened

    [Header("Audio Settings")]
    public AudioSource audioSource;       // Reference to the AudioSource component
    public AudioClip openSound;           // Sound played when opening the locker
    public AudioClip closeSound;          // Sound played when closing the locker

    void Start()
    {
        // Save the initial (closed) rotation of the door
        closedRotation = doorTransform.rotation;
        // Calculate the open rotation in a single direction (e.g., always inward or outward)
        openRotation = closedRotation * Quaternion.Euler(openAngle, 0f, 0f);  // Always rotate on the Y-axis

        // Ensure AudioSource is assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogError("No AudioSource component found on " + gameObject.name);
            }
        }
    }

    void Update()
    {
        // Adjust speed based on whether the door is opening or closing
        float speed = isOpen ? doorOpenSpeed : doorOpenSpeed * closeSpeedMultiplier;

        // Smoothly rotate the door towards the target rotation (either open or closed)
        doorTransform.rotation = Quaternion.Slerp(doorTransform.rotation, isOpen ? openRotation : closedRotation, Time.deltaTime * speed);
    }

    // Toggle the door open or closed
    public void ToggleDoor()
    {
        isOpen = !isOpen;

        // Play the appropriate sound effect
        if (audioSource != null)
        {
            audioSource.PlayOneShot(isOpen ? openSound : closeSound);
        }
    }

    // Implement the Interact() method from IInteractable
    public void Interact()
    {
        ToggleDoor();
    }
}
