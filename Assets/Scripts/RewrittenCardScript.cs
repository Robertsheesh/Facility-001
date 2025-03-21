using UnityEngine;
using System.Collections;

public class CardReaderScript : MonoBehaviour, IInteractable
{
    public Transform cardInsertPoint;  // Where the card is inserted
    private ObjectPicker objectPicker; // Reference to the player's ObjectPicker script
    public Animator doorAnimator;      // Reference to the door's Animator
    public AudioSource doorSound;      // Optional: Door sound for opening/closing
    public AudioSource CardInsertSound;
    public float openDelay = 1f;       // Delay before the door opens
    private bool doorOpened = false;   // Track if the door has been opened
    private GameObject insertedKeycard = null;  // Track the inserted keycard
    public Light StorageDoorLight;

    void Start()
    {
        // Find the player's ObjectPicker script
        objectPicker = GameObject.FindWithTag("Player").GetComponent<ObjectPicker>();
        if (objectPicker == null)
        {
            Debug.LogError("ObjectPicker script not found on the player.");
        }

        // Ensure the door starts closed
        doorAnimator.SetBool("IsOpen", false);
    }

    // This is the Interact method from IInteractable, called when the player interacts with the object
    public void Interact()
    {
        // Check if the player is holding a rewritten keycard
        if (objectPicker != null)
        {
            if (objectPicker.pickedUpObject != null && objectPicker.pickedUpObject.CompareTag("RewrittenKeycard"))
            {
                InsertKeycard();
            }
            else if (insertedKeycard != null)
            {
                PickUpInsertedKeycard();
            }
            else
            {
                Debug.Log("No keycard in hand or in reader.");
            }
        }
    }

    void InsertKeycard()
    {
        if (doorOpened) return;  // Prevent reopening the door if it's already open

        Debug.Log("Inserting keycard into the reader.");

        // Position the keycard at the insertion point
        insertedKeycard = objectPicker.pickedUpObject;
        insertedKeycard.transform.position = cardInsertPoint.position;
        insertedKeycard.transform.rotation = cardInsertPoint.rotation;

        // Disable physics and collider for the keycard once it's inserted
        Rigidbody rb = insertedKeycard.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;  // Disable physics
        }
        Collider objCollider = insertedKeycard.GetComponent<Collider>();
        if (objCollider != null)
        {
            objCollider.enabled = false;  // Disable collider
        }

        // Remove the keycard from the player's inventory
        objectPicker.RemoveItemFromInventory(insertedKeycard);

        // Clear the reference to the picked-up object so the player is no longer holding it
        objectPicker.pickedUpObject = null;
        objectPicker.isKeycard = false;  // Reset keycard flag

        // Open the door
        OpenDoor();
        doorOpened = true;  // Mark the door as opened

        StartCardInsertSound();
    }

    void PickUpInsertedKeycard()
    {
        if (insertedKeycard == null)
        {
            Debug.LogError("Inserted keycard is null.");
            return;
        }

        Debug.Log("Player picked up the inserted keycard.");

        // Unparent
        insertedKeycard.transform.SetParent(null);

        // Re-enable physics and colliders
        Rigidbody rb = insertedKeycard.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        Collider[] colliders = insertedKeycard.GetComponentsInChildren<Collider>(true);
        foreach (Collider collider in colliders)
        {
            collider.enabled = false;
        }

        objectPicker.PickUpObject(insertedKeycard);
        objectPicker.SelectItemInInventory(insertedKeycard);

        insertedKeycard = null;
        StartCardInsertSound();
    }



    // Method to open the door with a delay
    public void OpenDoor()
    {
        Debug.Log("Opening door...");
        StartCoroutine(OpenDoorWithDelay());  // Start the coroutine to open the door with a delay
        StorageDoorLight.color = Color.green;
    }

    // Coroutine to handle the door opening after the delay
    private IEnumerator OpenDoorWithDelay()
    {
        yield return new WaitForSeconds(openDelay);  // Wait for the specified delay time

        doorAnimator.SetBool("IsOpen", true);  // Set the Animator parameter to play the open animation
        PlayDoorSound();  // Play the door opening sound
    }

    // Play door opening sound
    void PlayDoorSound()
    {
        if (doorSound != null)
        {
            doorSound.Play();  // Start playing the door sound
        }
    }

    void StartCardInsertSound()
    {
        if (CardInsertSound != null)
        {
            CardInsertSound.Play();  // Start playing the door sound
        }
    }
}
