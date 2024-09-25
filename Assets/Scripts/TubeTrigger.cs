using UnityEngine;

public class TubeTrigger : MonoBehaviour
{
    public Transform tubeInsertPoint; // The point where the canister will be placed inside the tube
    private ObjectPicker objectPicker; // Reference to the player's ObjectPicker script

    void Start()
    {
        // Find the player's ObjectPicker script
        objectPicker = GameObject.FindWithTag("Player").GetComponent<ObjectPicker>();

        if (objectPicker == null)
        {
            Debug.LogError("ObjectPicker script not found on the player.");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Check if the player is in the trigger zone and pressing E
        if (other.CompareTag("Player") && objectPicker != null)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("Player pressed E while in the tube trigger zone.");

                // Check if the player is holding the canister
                if (objectPicker.pickedUpObject != null && objectPicker.pickedUpObject.CompareTag("Pickup"))
                {
                    Debug.Log("Canister detected in hand. Attempting to insert...");
                    InsertCanister();
                }
                else
                {
                    Debug.Log("No canister detected in player's hand.");
                }
            }
        }
    }

    void InsertCanister()
    {
        Debug.Log("Inserting canister into the tube.");

        LeverManager leverManager = FindObjectOfType<LeverManager>();
        if (leverManager != null)
        {
            leverManager.RefuelMachine();  // Reset the fuel level
        }

        // Position the canister at the insertion point inside the tube
        objectPicker.pickedUpObject.transform.position = tubeInsertPoint.position;
        objectPicker.pickedUpObject.transform.rotation = tubeInsertPoint.rotation;

        // Disable physics and collider for the canister once it's inserted
        Rigidbody rb = objectPicker.pickedUpObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // Disable physics
        }

        Collider objCollider = objectPicker.pickedUpObject.GetComponent<Collider>();
        if (objCollider != null)
        {
            objCollider.enabled = false; // Disable collider
        }

        // Clear the reference to the picked-up object so the player is no longer holding it
        objectPicker.pickedUpObject = null;

        Debug.Log("Canister successfully inserted into the tube!");
    }
}
