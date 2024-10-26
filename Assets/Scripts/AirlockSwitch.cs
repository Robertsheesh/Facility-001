using UnityEngine;

public class AirlockSwitch : MonoBehaviour, IInteractable
{
    public AirlockController airlockController; // Reference to the AirlockController
    public Transform cardInsertPoint;           // Where the keycard is inserted
    public AudioSource cardInsertSound;         // Sound for inserting the keycard

    private ObjectPicker objectPicker;          // Reference to the player's ObjectPicker
    private GameObject insertedKeycard = null;  // Track the inserted keycard
    private bool isUsed = false;                // Tracks if the switch has been used
    private bool keycardInserted = false;       // Tracks if the keycard is in place
    private bool airlockInProgress = false;     // Tracks if the airlock process is active

    void Start()
    {
        // Find the ObjectPicker on the player
        objectPicker = GameObject.FindWithTag("Player").GetComponent<ObjectPicker>();
        if (objectPicker == null)
        {
            Debug.LogError("ObjectPicker script not found on the player.");
        }
    }

    // Called when the player interacts with the switch
    public void Interact()
    {
        if (keycardInserted && !isUsed && !airlockInProgress)
        {
            ActivateAirlock();
        }
        else if (keycardInserted && !airlockInProgress)
        {
            // Allow player to pick up the keycard only after the airlock sequence has finished
            PickUpInsertedKeycard();
        }
        else if (!keycardInserted && objectPicker != null && objectPicker.pickedUpObject != null
                 && objectPicker.pickedUpObject.CompareTag("RewrittenKeycard"))
        {
            InsertKeycard();
        }
        else if (keycardInserted && airlockInProgress)
        {
            Debug.Log("The airlock process is in progress; you cannot remove the keycard.");
        }
        else if (!keycardInserted)
        {
            Debug.Log("You need a keycard to activate the airlock.");
        }
    }

    void InsertKeycard()
    {
        if (keycardInserted) return; // Prevent inserting multiple times

        Debug.Log("Inserting keycard into the airlock switch.");

        // Place the keycard at the insertion point
        insertedKeycard = objectPicker.pickedUpObject;
        insertedKeycard.transform.position = cardInsertPoint.position;
        insertedKeycard.transform.rotation = cardInsertPoint.rotation;

        // Disable keycard physics and collider
        Rigidbody rb = insertedKeycard.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
        Collider objCollider = insertedKeycard.GetComponent<Collider>();
        if (objCollider != null)
        {
            objCollider.enabled = false;
        }

        // Remove keycard from player's inventory and clear held object
        objectPicker.RemoveItemFromInventory(insertedKeycard);
        objectPicker.pickedUpObject = null;

        keycardInserted = true; // Mark the keycard as inserted
        PlayCardInsertSound();
    }

    void PickUpInsertedKeycard()
    {
        if (insertedKeycard == null) return;

        Debug.Log("Player picked up the inserted keycard.");

        // Re-enable keycard physics and collider for the player to pick it up
        Rigidbody rb = insertedKeycard.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
        Collider objCollider = insertedKeycard.GetComponent<Collider>();
        if (objCollider != null)
        {
            objCollider.enabled = false;
        }

        // Add the keycard back to the player's inventory
        objectPicker.pickedUpObject = insertedKeycard;
        objectPicker.AddItemToInventory(insertedKeycard);

        // Clear local reference after pickup
        insertedKeycard = null;
        keycardInserted = false;

        PlayCardInsertSound();
    }

    void ActivateAirlock()
    {
        if (airlockController != null && !isUsed)
        {
            airlockController.InteractWithAirlock(); // Trigger the airlock sequence
            isUsed = true; // Mark the switch as used
            airlockInProgress = true; // Start tracking the airlock process

            // Subscribe to airlock completion to allow keycard retrieval afterward
            airlockController.OnAirlockSequenceComplete += CompleteAirlockProcess;
        }
    }

    void CompleteAirlockProcess()
    {
        airlockInProgress = false; // Allow keycard retrieval once the process is complete

        // Unsubscribe from the event to avoid multiple calls
        airlockController.OnAirlockSequenceComplete -= CompleteAirlockProcess;
    }

    void PlayCardInsertSound()
    {
        if (cardInsertSound != null)
        {
            cardInsertSound.Play(); // Play card insert sound
        }
    }
}
