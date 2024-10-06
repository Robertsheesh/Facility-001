using UnityEngine;

public class CardInsertScript : MonoBehaviour, IInteractable
{
    public Transform cardInsertPoint; // The point where the keycard will be placed inside the reader
    private ObjectPicker objectPicker; // Reference to the player's ObjectPicker script
    private GameObject insertedKeycard = null;  // Track the inserted keycard
    public AudioSource CardInsertSound;
    public MusicManager musicManager; // Reference to the MusicManager

    void Start()
    {
        // Find the player's ObjectPicker script
        objectPicker = GameObject.FindWithTag("Player").GetComponent<ObjectPicker>();

        if (objectPicker == null)
        {
            Debug.LogError("ObjectPicker script not found on the player.");
        }
    }

    // This is the Interact method from IInteractable, called when the player interacts with the object
    public void Interact()
    {
        if (objectPicker != null)
        {
            // Check if the player is holding the keycard
            if (objectPicker.pickedUpObject != null && objectPicker.pickedUpObject.CompareTag("Keycard"))
            {
                Debug.Log("Keycard detected in hand. Attempting to insert...");
                InsertKeycard();
            }
            else if (insertedKeycard != null)
            {
                Debug.Log("Picking up the rewritten keycard.");
                PickUpRewrittenKeycard();
            }
            else
            {
                Debug.Log("No keycard detected in player's hand.");
            }
        }
    }

    void InsertKeycard()
    {
        Debug.Log("Inserting keycard.");

        // Position the keycard at the insertion point
        insertedKeycard = objectPicker.pickedUpObject;
        insertedKeycard.transform.position = cardInsertPoint.position;
        insertedKeycard.transform.rotation = cardInsertPoint.rotation;

        // Disable physics and collider for the keycard once it's inserted
        Rigidbody rb = insertedKeycard.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // Disable physics
        }

        Collider objCollider = insertedKeycard.GetComponent<Collider>();
        if (objCollider != null)
        {
            objCollider.enabled = false; // Disable collider
        }

        // Clear the reference to the picked-up object so the player is no longer holding it
        objectPicker.pickedUpObject = null;

        Debug.Log("Card successfully inserted into the reader!");
        StartCardInsertSound();
    }

    public bool HasInsertedKeycard()
    {
        return insertedKeycard != null; // Return true if there's a keycard inserted
    }

    public void MarkKeycardAsRewritten()
    {
        if (insertedKeycard != null)
        {
            Debug.Log("Keycard marked as rewritten and can now be picked up.");
            insertedKeycard.tag = "RewrittenKeycard";  // Change tag to indicate it's rewritten
        }
    }

    void PickUpRewrittenKeycard()
    {
        if (insertedKeycard != null && insertedKeycard.CompareTag("RewrittenKeycard"))
        {
            Debug.Log("Player picked up the rewritten keycard.");

            // Re-enable physics and collider for the keycard when picked up
            Rigidbody rb = insertedKeycard.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false; // Re-enable physics
            }

            Collider objCollider = insertedKeycard.GetComponent<Collider>();
            if (objCollider != null)
            {
                objCollider.enabled = true; // Re-enable collider
            }

            // Allow the player to pick up the rewritten keycard
            objectPicker.pickedUpObject = insertedKeycard;
            insertedKeycard = null;  // Clear the reference once picked up
            StartCardInsertSound();

            // Change music when keycard is picked up
            if (musicManager != null)
            {
                musicManager.SwitchToNewTrack();
                Debug.Log("CardInsert switching to new track");
            }
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
