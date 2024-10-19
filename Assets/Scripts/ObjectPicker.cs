using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class ObjectPicker : MonoBehaviour
{
    public Camera playerCamera;         // Main player camera
    public float pickupRange = 3f;      // The range at which the player can pick up objects
    public Vector3 holdOffset = new Vector3(0f, -0.5f, 1.5f); // Position offset for other objects in front of the camera
    public GameObject pickedUpObject = null;

    [Header("Flashlight Settings")]
    public GameObject flashlightObject;  // Assign this in the inspector
    public Vector3 flashlightPositionOffset = new Vector3(0.5f, -0.5f, 1.5f); // Position offset for the flashlight
    public Vector3 flashlightRotationOffset = new Vector3(0f, 0f, 0f);        // Rotation offset for the flashlight
    private Light flashlightLight; // Reference to the flashlight's light component
    private bool isFlashlightOn = true; // Track whether the flashlight is currently on

    private bool isFlashlight = false;  // Track if the picked-up object is a flashlight

    [Header("Keycard Settings")]
    public Vector3 keycardPositionOffset = new Vector3(0.2f, -0.3f, 1.0f); // Position offset for the keycard
    public Vector3 keycardRotationOffset = new Vector3(0f, 90f, 0f);       // Rotation offset for the keycard
    private bool isKeycard = false;     // Track if the picked-up object is a keycard
    private bool isRewrittenKeycard = false;  // Track if the picked-up object is a rewritten keycard

    [Header("Rewritten Keycard Settings")]
    public Vector3 rewrittenKeycardPositionOffset = new Vector3(0.2f, -0.3f, 1.0f); // Adjust as needed
    public Vector3 rewrittenKeycardRotationOffset = new Vector3(0f, 90f, 0f);       // Adjust as needed

    [Header("Sound Settings")]
    public AudioSource audioSource;  // The audio source for playing unequip sounds
    public AudioClip unequipSound;   // The sound that will play when unequipping
    public AudioSource flashlightSound;

    // Inventory system
    private Dictionary<int, GameObject> inventory = new Dictionary<int, GameObject>();
    private int selectedSlot = 1; // Currently selected slot
    [Header("Max Inventory Slots")]
    public const int maxInventorySlots = 3; // Limit to 3 items in the inventory

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        // Ensure flashlightLight is assigned if flashlightObject is not null
        if (flashlightObject != null)
        {
            flashlightLight = flashlightObject.GetComponentInChildren<Light>();
            flashlightObject.SetActive(true); // Ensure it's inactive at the start
        }
    }

    void Update()
    {
        // Detect input for picking up the object (e.g., pressing E)
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryPickUpObject();  // Try to pick up a new object
        }

        // Detect input for dropping the object (press G to drop)
        if (Input.GetKeyDown(KeyCode.G))
        {
            DropObject();  // Drop the object
        }

        // Detect input for throwing the object (press left mouse button to throw)
        if (Input.GetMouseButtonDown(0))
        {
            ThrowObject();  // Throw the currently held object
        }

        // Detect input for switching items (number keys 1-9)
        for (int i = 1; i <= maxInventorySlots; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                SelectInventorySlot(i);
                break;
            }
        }

        // If an object is picked up (equipped), update its position and rotation
        if (pickedUpObject != null)
        {
            if (isFlashlight)
            {
                // Custom positioning for the flashlight with configurable offsets
                Vector3 holdPosition = playerCamera.transform.position
                                       + playerCamera.transform.forward * flashlightPositionOffset.z
                                       + playerCamera.transform.right * flashlightPositionOffset.x
                                       + playerCamera.transform.up * flashlightPositionOffset.y;

                pickedUpObject.transform.position = holdPosition;

                // Apply the rotation offset to the flashlight, using the camera's rotation as a base
                pickedUpObject.transform.rotation = playerCamera.transform.rotation * Quaternion.Euler(flashlightRotationOffset);

                // Toggle flashlight power on left mouse button click
                if (Input.GetMouseButtonDown(0))
                {
                    ToggleFlashlight();
                }
            }
            else if (isKeycard) // Basic keycard
            {
                Vector3 holdPosition = playerCamera.transform.position
                                       + playerCamera.transform.forward * keycardPositionOffset.z
                                       + playerCamera.transform.right * keycardPositionOffset.x
                                       + playerCamera.transform.up * keycardPositionOffset.y;

                pickedUpObject.transform.position = holdPosition;
                pickedUpObject.transform.rotation = playerCamera.transform.rotation * Quaternion.Euler(keycardRotationOffset);
            }
            else if (isRewrittenKeycard) // Rewritten keycard
            {
                Vector3 holdPosition = playerCamera.transform.position
                                       + playerCamera.transform.forward * rewrittenKeycardPositionOffset.z
                                       + playerCamera.transform.right * rewrittenKeycardPositionOffset.x
                                       + playerCamera.transform.up * rewrittenKeycardPositionOffset.y;

                pickedUpObject.transform.position = holdPosition;
                pickedUpObject.transform.rotation = playerCamera.transform.rotation * Quaternion.Euler(rewrittenKeycardRotationOffset);
            }
            else
            {
                // For non-flashlight objects, use standard positioning and rotation
                Vector3 holdPosition = playerCamera.transform.position
                                       + playerCamera.transform.forward * holdOffset.z
                                       + playerCamera.transform.right * holdOffset.x
                                       + playerCamera.transform.up * holdOffset.y;

                pickedUpObject.transform.position = holdPosition;

                // Keep the object upright and only rotate with the camera's yaw (left-right)
                pickedUpObject.transform.rotation = Quaternion.Euler(0, playerCamera.transform.eulerAngles.y, 0); // Lock rotation to yaw only
            }
        }
    }

    // Try to pick up an object in front of the player
    void TryPickUpObject()
    {
        // Check if inventory is full
        if (inventory.Count >= maxInventorySlots)
        {
            Debug.Log("Inventory is full!");
            return;
        }

        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, pickupRange))
        {
            // Check if the hit object is tagged as "Pickup", "Flashlight", "Keycard", or "RewrittenKeycard"
            if (hit.collider.CompareTag("Pickup") || hit.collider.CompareTag("Flashlight") || hit.collider.CompareTag("Keycard") || hit.collider.CompareTag("RewrittenKeycard"))
            {
                PickUpObject(hit.collider.gameObject);
            }
        }
    }

    void ThrowObject()
    {
        // Check if the player is holding an object tagged as "Pickup"
        if (pickedUpObject != null && pickedUpObject.CompareTag("Pickup"))
        {
            // Re-enable physics for the object
            Rigidbody rb = pickedUpObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false; // Re-enable physics
                rb.useGravity = true;    // Ensure gravity is applied so it falls
                

                // Apply a forward force to throw the object in the direction the player is facing
                Vector3 throwDirection = playerCamera.transform.forward;  // Throw in the direction the camera is facing
                float throwForce = 10f;  // Adjust this value to change the throw strength
                rb.AddForce(throwDirection * throwForce, ForceMode.VelocityChange);  // Apply the throw force

                DropObject();

                flashlightObject.SetActive(false);

                // Ensure the player's hands are empty
                pickedUpObject = null;  // Clear the reference so the player is empty-handed
                isFlashlight = false;   // Reset the flashlight flag
                isKeycard = false;      // Reset the keycard flag
                isRewrittenKeycard = false; // Reset the rewritten keycard flag
                flashlightLight = null; // Clear flashlight reference
            }
        }
    }

    // Pick up the object and add it to the inventory
    public void PickUpObject(GameObject obj)
    {
        // Check if inventory is full
        if (inventory.Count >= maxInventorySlots)
        {
            Debug.Log("Inventory is full! Cannot pick up more items.");
            return; // Prevent picking up if inventory is full
        }

        // Find the next available slot in the inventory
        int slot = GetNextAvailableSlot();
        if (slot == -1)
        {
            Debug.Log("Inventory is full!");
            return;
        }

        // If it's a flashlight, make sure to turn it off when picking it up
        if (obj.CompareTag("Flashlight"))
        {
            Light flashlightLight = obj.GetComponentInChildren<Light>();
            if (flashlightLight != null)
            {
                flashlightLight.enabled = false;  // Turn off flashlight
                PlayFlashlightSound();  // Play the sound when turning it off
            }
        }

        // Disable physics while holding the object
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // Disable physics so it doesn't fall or collide
        }

        // Disable only BoxColliders to prevent object from interacting with the environment
        BoxCollider[] boxColliders = obj.GetComponentsInChildren<BoxCollider>(true);
        foreach (BoxCollider boxCollider in boxColliders)
        {
            boxCollider.enabled = false;  // Disable each BoxCollider
        }

        // Deactivate the object in the scene
        obj.SetActive(false);

        // Add the object to the inventory
        inventory.Add(slot, obj);

        // If there is no currently equipped item, equip this item
        if (pickedUpObject == null)
        {
            SelectInventorySlot(slot);
        }
    }



    public void SelectInventorySlot(int slot)
    {
        // If the selected slot matches the currently equipped item, unequip it
        if (pickedUpObject != null && selectedSlot == slot)
        {
            PlayUnequipSound();
            // Unequip the current item, leaving the player's hands empty
            StartCoroutine(UnequipItemCoroutine(-1)); // Pass -1 to indicate no new item will be equipped
            return; // Exit the method after unequipping
        }

        // If there is a new item in the selected slot, equip it
        if (inventory.ContainsKey(slot))
        {
            // If an item is currently equipped, animate it moving down (unequipping)
            if (pickedUpObject != null)
            {
                StartCoroutine(UnequipItemCoroutine(slot));
            }
            else
            {
                // If no item is currently equipped, just equip the new item instantly
                EquipNewItem(slot);
            }
        }
        else
        {
            Debug.Log("No item in slot: " + slot);
        }
    }

    private IEnumerator UnequipItemCoroutine(int newSlot)
    {
        // Animate the currently equipped item moving downward (unequipping)
        yield return StartCoroutine(MoveItemDown(pickedUpObject, playerCamera));

        // Deactivate the current item after the animation
        if (pickedUpObject != null)
        {
            pickedUpObject.SetActive(false);

            pickedUpObject = null; // Clear the reference to the equipped item
        }

        // If newSlot is -1, don't equip any new item (leave hands empty)
        if (newSlot != -1)
        {
            // Add a 0.5-second delay before equipping the new item
            yield return new WaitForSeconds(0.001f);

            // Equip the new item after the delay
            EquipNewItem(newSlot);
        }
    }

    private IEnumerator MoveItemDown(GameObject item, Camera playerCamera)
    {
        if (item == null) yield break;

        // Set the duration and movement distance
        float duration = 0.3f;  // Adjust the duration as needed
        Vector3 startPos = item.transform.position;

        // Move the item backward and slightly downward relative to the camera
        Vector3 endPos = playerCamera.transform.position - playerCamera.transform.forward * 0.3f - playerCamera.transform.up * 0.8f; // Move it slightly backward and down

        float elapsedTime = 0f;

        // Animate the item moving downward and backward over time
        while (elapsedTime < duration)
        {
            item.transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        item.transform.position = endPos; // Ensure the final position is set
    }




    private void EquipNewItem(int slot)
    {
        GameObject obj = inventory[slot];
        obj.SetActive(false);
        pickedUpObject = obj;
        selectedSlot = slot;

        // Reset flags
        isFlashlight = false;
        isKeycard = false;
        isRewrittenKeycard = false;

        PlayUnequipSound();

        // Check if it's a flashlight
        if (pickedUpObject.CompareTag("Flashlight"))
        {
            isFlashlight = true;
            flashlightLight = pickedUpObject.GetComponentInChildren<Light>();

            // Initialize flashlight to be off when equipped
            isFlashlightOn = false;
            if (flashlightLight != null)
            {
                flashlightLight.enabled = false; // Ensure the light is off when picked up
            }
        }

        DisableColliders(pickedUpObject);

        // Check if it's a keycard or rewritten keycard
        isKeycard = pickedUpObject.CompareTag("Keycard");
        isRewrittenKeycard = pickedUpObject.CompareTag("RewrittenKeycard");

        // Handle colliders and physics for keycards
        if (isKeycard || isRewrittenKeycard)
        {
            DisableColliders(pickedUpObject);

            Rigidbody rb = pickedUpObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true; // Disable physics
            }
        }

        pickedUpObject.SetActive(true);
    }




    private void DisableColliders(GameObject obj)
    {
        Collider[] colliders = obj.GetComponentsInChildren<Collider>(true);
        foreach (Collider collider in colliders)
        {
            collider.enabled = false;
        }
    }

    private void EnableColliders(GameObject obj)
    {
        Collider[] colliders = obj.GetComponentsInChildren<Collider>(true);
        foreach (Collider collider in colliders)
        {
            collider.enabled = true; // Re-enable all colliders
        }
    }

    // Drop the currently held object
    void DropObject()
    {
        if (pickedUpObject != null)
        {
            // Check if the object is a "Pickup" item, otherwise prevent dropping
            if (!pickedUpObject.CompareTag("Pickup"))
            {
                Debug.Log("Cannot drop this item: " + pickedUpObject.name);
                return; // Exit the method, prevent dropping
            }
            // Re-enable physics so the object falls naturally
            Rigidbody rb = pickedUpObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false; // Re-enable physics
                rb.useGravity = true;    // Ensure gravity is applied so it falls
            }

            EnableColliders(pickedUpObject);

            pickedUpObject.transform.SetParent(null);  // Unparent the object from the camera

            // Remove from inventory
            inventory.Remove(selectedSlot);

            // Activate the item in the scene
            pickedUpObject.SetActive(true);

            Debug.Log("Dropped: " + pickedUpObject.name + " from slot " + selectedSlot);

            pickedUpObject = null;  // Clear the reference to the object so it stops floating
            isFlashlight = false;   // Reset the flashlight flag
            isKeycard = false;      // Reset the keycard flag
            isRewrittenKeycard = false; // Reset the rewritten keycard flag
            flashlightLight = null; // Clear flashlight reference

            // Automatically select another item if available
            if (inventory.Count > 0)
            {
                int nextSlot = GetFirstAvailableSlot();
                SelectInventorySlot(nextSlot);
            }
        }
    }

    // Toggle the flashlight on/off
    void ToggleFlashlight()
    {
        if (flashlightLight != null)
        {
            isFlashlightOn = !isFlashlightOn; // Toggle the state
            flashlightLight.enabled = isFlashlightOn; // Apply the state to the light
            PlayFlashlightSound(); // Play sound whenever toggling
        }
    }


    void PlayFlashlightSound()
    {
        if (flashlightSound != null)
        {
            flashlightSound.Play();  // Play the flashlight toggle sound
            Debug.Log("Flashlight sound played!");  // Add this debug to verify it gets called
        }
        else
        {
            Debug.Log("Flashlight sound is not assigned!");  // Debug if the AudioSource is null
        }
    }

    // Helper function to get the next available inventory slot
    int GetNextAvailableSlot()
    {
        for (int i = 1; i <= maxInventorySlots; i++)
        {
            if (!inventory.ContainsKey(i))
            {
                return i;
            }
        }
        return -1; // Inventory is full
    }

    // Helper function to get the first available slot (used when dropping items)
    int GetFirstAvailableSlot()
    {
        for (int i = 1; i <= maxInventorySlots; i++)
        {
            if (inventory.ContainsKey(i))
            {
                return i;
            }
        }
        return -1;
    }

    public void SelectItemInInventory(GameObject obj)
    {
        int slot = -1;
        foreach (var kvp in inventory)
        {
            if (kvp.Value == obj)
            {
                slot = kvp.Key;
                break;
            }
        }

        if (slot != -1)
        {
            SelectInventorySlot(slot);
        }
        else
        {
            Debug.LogError("Item not found in inventory.");
        }
    }


    public void RemoveItemFromInventory(GameObject obj)
    {
        int slotToRemove = -1;

        foreach (var kvp in inventory)
        {
            if (kvp.Value == obj)
            {
                slotToRemove = kvp.Key;
                break;
            }
        }

        if (slotToRemove != -1)
        {
            inventory.Remove(slotToRemove);
            Debug.Log("Removed item from inventory: " + obj.name + " from slot " + slotToRemove);

            // If the removed item was the currently selected one, clear it
            if (pickedUpObject == obj)
            {
                pickedUpObject = null;
            }

            // Log the contents of the inventory
            Debug.Log("Current Inventory Contents:");
            foreach (var kvp in inventory)
            {
                Debug.Log($"Slot {kvp.Key}: {kvp.Value.name}");
            }
        }
        else
        {
            Debug.LogError("Failed to remove item from inventory: Item not found.");
        }
    }

    private void PlayUnequipSound()
    {
        // Play the unequip sound if available
        if (audioSource != null && unequipSound != null)
        {
            audioSource.PlayOneShot(unequipSound);  // Play the unequip sound immediately
        }
    }

}
