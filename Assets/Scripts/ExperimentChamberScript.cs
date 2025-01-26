using UnityEngine;

public class ExperimentChamberScript : MonoBehaviour, IInteractable
{
    public Transform handPlacement;
    private ObjectPicker objectPicker;
    private GameObject insertedHand = null;
    public ExperimentDoor experimentDoor;

    public bool isHandInserted = false;

    void Start()
    {
        // Find the player's ObjectPicker script
        objectPicker = GameObject.FindWithTag("Player").GetComponent<ObjectPicker>();

        if (objectPicker == null)
        {
            Debug.LogError("ObjectPicker script not found on the player.");
        }
    }

    public void Interact()
    {
        if (objectPicker != null)
        {
            // Check if the player is holding the keycard
            if (objectPicker.pickedUpObject != null && objectPicker.pickedUpObject.CompareTag("SeveredHand"))
            {
                Debug.Log("SeveredHand detected in hand. Attempting to insert...");
                InsertSeveredHand();
            }
            else if (insertedHand != null)
            {
                Debug.Log("Picking up the SeveredHand.");
                PickUpRewrittenHand();
            }
            else
            {
                Debug.Log("No SeveredHand detected in player's hand.");
            }
        }
    }

    void InsertSeveredHand()
    {
        if (experimentDoor.IsDoorOpen())
        {
            if (insertedHand != null || isHandInserted) return; // Prevent double insertion or re-insertion

            Debug.Log("Inserting keycard.");
            insertedHand = objectPicker.pickedUpObject;
            insertedHand.transform.position = handPlacement.position;
            insertedHand.transform.rotation = handPlacement.rotation;

            Rigidbody rb = insertedHand.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true; // Disable physics for the inserted card
            }

            DisableColliders(insertedHand);  // Disable the colliders

            // Remove keycard from inventory and flag it as unavailable
            objectPicker.RemoveItemFromInventory(insertedHand);

            objectPicker.pickedUpObject = null;  // Unequip the keycard

            isHandInserted = true; // Mark the keycard as inserted
            Debug.Log("Hand successfully inserted into the experiment chamber!");
        }
        else
        {
            Debug.Log("Cannot insert Hand");
        }
    }

    public bool HasInsertedSeveredHand()
    {
        return insertedHand != null; // Return true if there's a keycard inserted
    }

    public void MarkSeveredHandAsRewritten()
    {
        if (insertedHand != null)
        {
            Debug.Log("Keycard marked as rewritten and can now be picked up.");
            insertedHand.tag = "RewrittenHand";  // Change tag to indicate it's rewritten
        }
    }

    void PickUpRewrittenHand()
    {
        if (experimentDoor.IsDoorOpen())
        {
            if (insertedHand != null && insertedHand.CompareTag("RewrittenHand"))
            {
            Debug.Log("Player picked up the inserted Hand.");

            insertedHand.transform.SetParent(null); // Unparent it from the reader

            // Only enable the necessary colliders (not the box collider)
            Collider[] colliders = insertedHand.GetComponentsInChildren<Collider>(true);
            foreach (Collider collider in colliders)
            {
                if (!(collider is BoxCollider)) // Ensure the box collider remains disabled
                {
                    collider.enabled = true;
                }
            }

            objectPicker.PickUpObject(insertedHand);  // Use ObjectPicker to pick up the object again
            insertedHand = null;  // Clear the reference once picked up

            objectPicker.SelectItemInInventory(objectPicker.pickedUpObject);  // Equip the keycard immediately
            }
        }
        else
        {
            Debug.Log("Door is closed, hand can't be placed");
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
}



