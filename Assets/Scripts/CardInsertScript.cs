using UnityEngine;

public class CardInsertScript : MonoBehaviour, IInteractable
{
    public Transform cardInsertPoint; // The point where the keycard will be placed inside the reader
    private ObjectPicker objectPicker; // Reference to the player's ObjectPicker script
    private GameObject insertedKeycard = null;  // Track the inserted keycard
    public AudioSource CardInsertSound;
    public MusicManager musicManager; // Reference to the MusicManager

   public bool isKeycardInserted = false; // Track if the keycard is currently inserted



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
        if (insertedKeycard != null || isKeycardInserted) return; // Prevent double insertion or re-insertion

        Debug.Log("Inserting keycard.");
        insertedKeycard = objectPicker.pickedUpObject;
        insertedKeycard.transform.position = cardInsertPoint.position;
        insertedKeycard.transform.rotation = cardInsertPoint.rotation;

        Rigidbody rb = insertedKeycard.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // Disable physics for the inserted card
        }

        DisableColliders(insertedKeycard);  // Disable the colliders

        // Remove keycard from inventory and flag it as unavailable
        objectPicker.RemoveItemFromInventory(insertedKeycard);

        objectPicker.pickedUpObject = null;  // Unequip the keycard

        isKeycardInserted = true; // Mark the keycard as inserted
        Debug.Log("Keycard successfully inserted into the reader!");
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
            Debug.Log("Player picked up the inserted keycard.");

            insertedKeycard.transform.SetParent(null); // Unparent it from the reader

            // Only enable the necessary colliders (not the box collider)
            Collider[] colliders = insertedKeycard.GetComponentsInChildren<Collider>(true);
            foreach (Collider collider in colliders)
            {
                if (!(collider is BoxCollider)) // Ensure the box collider remains disabled
                {
                    collider.enabled = true;
                }
            }

            objectPicker.PickUpObject(insertedKeycard);  // Use ObjectPicker to pick up the object again
            objectPicker.SelectItemInInventory(insertedKeycard);  // Equip the keycard immediately

            insertedKeycard = null;  // Clear the reference once picked up

            StartCardInsertSound();

            // Update the state of music, or other actions
            if (musicManager != null)
            {
                musicManager.SwitchToNewTrack();
                Debug.Log("CardInsert switching to new track");
            }
        }
    }



    private void DisableColliders(GameObject obj)
    {
        Collider[] colliders = obj.GetComponentsInChildren<Collider>(true);
        foreach (Collider collider in colliders)
        {
            collider.enabled = false; // Disable each collider
        }
    }

    private void EnableColliders(GameObject obj)
    {
        Collider[] colliders = obj.GetComponentsInChildren<Collider>(true);
        foreach (Collider collider in colliders)
        {
            collider.enabled = true; // Re-enable each collider
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
