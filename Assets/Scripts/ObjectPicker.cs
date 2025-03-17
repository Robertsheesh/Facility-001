using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Cinemachine;

public class ObjectPicker : MonoBehaviour
{
    public CinemachineImpulseSource impulseSource;
    public Camera playerCamera;         // Main player camera
    public float pickupRange = 3f;      // The range at which the player can pick up objects
    public Vector3 holdOffset = new Vector3(0f, -0.5f, 1.5f); // Position offset for other objects in front of the camera
    public GameObject pickedUpObject = null;

    [Header("PickUp Settings")]
    public Vector3 pickupPositionOffset = new Vector3(0.5f, -0.5f, 1.5f); // Position offset for "PickUp" tagged objects
    public Vector3 pickupRotationOffset = new Vector3(0f, 0f, 0f);        // Rotation offset for "PickUp" tagged objects
    public bool isPickupObject = false;  // Track if the picked-up object is a "PickUp"

    [Header("Flashlight Settings")]
    public GameObject flashlightObject;  // Assign this in the inspector
    public Vector3 flashlightPositionOffset = new Vector3(0.5f, -0.5f, 1.5f); // Position offset for the flashlight
    public Vector3 flashlightRotationOffset = new Vector3(0f, 0f, 0f);        // Rotation offset for the flashlight
    public Light flashlightLight; // Reference to the flashlight's light component
    public bool isFlashlightOn = true; // Track whether the flashlight is currently on

    public bool isFlashlight = false;  // Track if the picked-up object is a flashlight

    [Header("Medical Settings")]
    public GameObject medicalObject;  // Assign this in the inspector
    public Vector3 medicalPositionOffset = new Vector3(0.5f, -0.5f, 1.5f); // Position offset for the flashlight
    public Vector3 medicalRotationOffset = new Vector3(0f, 0f, 0f);        // Rotation offset for the flashlight

    public bool isMedical = false;  // Track if the picked-up object is a flashlight

    [Header("Severed Hand Settings")]
    public Vector3 severedHandPositionOffset = new Vector3(0.5f, -0.5f, 1.5f); // Position offset for the severed hand
    public Vector3 severedHandRotationOffset = new Vector3(0f, 0f, 0f);        // Rotation offset for the severed hand
    public bool isSeveredHand = false;

    [Header("Crowbar Settings")]
    public Vector3 crowbarPositionOffset = new Vector3(0.3f, -0.4f, 1.0f);  // Custom position offset for the crowbar
    public Vector3 crowbarRotationOffset = new Vector3(0f, -90f, 0f);  // Custom rotation offset for the crowbar
    public bool isCrowbar = false;  // Track if the picked-up object is a crowbar
    public bool isSwinging = false; // Prevent spamming the hit animation
    public float swingDuration = 0.01f; // How long the swing lasts
    public Vector3 swingRotationOffset = new Vector3(0f, 0f, -45f); // Swing arc on Z-axis
    public AudioSource crowbarAudioSource;  // Assign this in the inspector or dynamically
    public AudioClip swooshSound;           // The swoosh sound when the crowbar swings
    public AudioClip impactSound;           // The impact sound when the crowbar hits something


    [Header("Keycard Settings")]
    public Vector3 keycardPositionOffset = new Vector3(0.2f, -0.3f, 1.0f); // Position offset for the keycard
    public Vector3 keycardRotationOffset = new Vector3(0f, 90f, 0f);       // Rotation offset for the keycard
    public bool isKeycard = false;     // Track if the picked-up object is a keycard
    public bool isRewrittenKeycard = false;  // Track if the picked-up object is a rewritten keycard

    [Header("Rewritten Keycard Settings")]
    public Vector3 rewrittenKeycardPositionOffset = new Vector3(0.2f, -0.3f, 1.0f); // Adjust as needed
    public Vector3 rewrittenKeycardRotationOffset = new Vector3(0f, 90f, 0f);       // Adjust as needed

    [Header("Bone Saw Settings")]
    public Vector3 boneSawPositionOffset = new Vector3(0.2f, -0.3f, 1.0f); // Adjust as needed
    public Vector3 boneSawRotationOffset = new Vector3(0f, 90f, 0f);       // Adjust as needed
    public Vector3 sawingRotationOffset = new Vector3(0f, 0f, -45f); // Swing arc on Z-axis
    public AudioSource boneSawAudioSource;  // Assign this in the inspector or dynamically
    public bool isBoneSaw = false;
    public bool isSawing = false;

    [Header("Sound Settings")]
    public AudioSource audioSource;  // The audio source for playing unequip sounds
    public AudioSource audioSourcePickup;  // The audio source for playing unequip sounds
    public AudioClip unequipSound;   // The sound that will play when unequipping
    public AudioClip pickupSound;   // The sound that will play when unequipping
    public AudioSource flashlightSound;

    // Inventory system
    private Dictionary<int, GameObject> inventory = new Dictionary<int, GameObject>();
    private int selectedSlot = 1; // Currently selected slot
    [Header("Max Inventory Slots")]
    public const int maxInventorySlots = 3; // Limit to 3 items in the inventory

    public PlayerInteraction playerInteractionUI;

    void Start()
    {
        if (impulseSource == null)
        {
            // Dynamically find the Impulse Source component attached to the crowbar
            impulseSource = pickedUpObject?.GetComponent<CinemachineImpulseSource>();
            if (impulseSource == null)
            {
                Debug.LogError("Impulse Source not found on pickedUpObject!");
            }
            else
            {
                Debug.Log("Impulse Source dynamically assigned.");
            }
        }

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

    void LateUpdate()
    {
        // Only skip positioning/rotation updates if the player is currently swinging the crowbar
        if (isSwinging)
        {
            // If the crowbar is swinging, we don't want to override its animation
            return;
        }
        // Detect input for picking up the object (e.g., pressing E)
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryPickUpObject();  // Try to pick up a new object
        }

        if (Input.GetMouseButtonDown(0) && isMedical)
        {
            StartCoroutine(UseMedSyringe());
            FindObjectOfType<PlayerHealth>().UseMedSyringe();
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

        // Detect input for hitting with the crowbar (left mouse button)
        if (Input.GetMouseButtonDown(0) && isCrowbar)
        {
            HitWithCrowbar(); // Call the hitting function for the crowbar
        }

        if (Input.GetMouseButtonDown(0) && isBoneSaw)
        {
            BoneSawSawing();
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
            else if (isCrowbar)
            {
                // Custom positioning for the crowbar with configurable offsets
                Vector3 holdPosition = playerCamera.transform.position
                                       + playerCamera.transform.forward * crowbarPositionOffset.z
                                       + playerCamera.transform.right * crowbarPositionOffset.x
                                       + playerCamera.transform.up * crowbarPositionOffset.y;

                pickedUpObject.transform.position = holdPosition;

                // Apply the rotation offset to the crowbar, using the camera's rotation as a base
                pickedUpObject.transform.rotation = playerCamera.transform.rotation * Quaternion.Euler(crowbarRotationOffset);
            }
            else if (pickedUpObject != null && isPickupObject)
            {
                // Use custom "PickUp" settings for positioning
                Vector3 holdPosition = playerCamera.transform.position
                                       + playerCamera.transform.forward * pickupPositionOffset.z
                                       + playerCamera.transform.right * pickupPositionOffset.x
                                       + playerCamera.transform.up * pickupPositionOffset.y;

                pickedUpObject.transform.position = holdPosition;
                pickedUpObject.transform.rotation = playerCamera.transform.rotation * Quaternion.Euler(pickupRotationOffset);
            }
            else if (isSeveredHand)
            {

                Vector3 holdPosition = playerCamera.transform.position
                       + playerCamera.transform.forward * severedHandPositionOffset.z
                       + playerCamera.transform.right * severedHandPositionOffset.x
                       + playerCamera.transform.up * severedHandPositionOffset.y;

                pickedUpObject.transform.position = holdPosition;

                // Apply the rotation offset to the severed hand, using the camera's rotation as a base
                pickedUpObject.transform.rotation = playerCamera.transform.rotation * Quaternion.Euler(severedHandRotationOffset);
            }
            else if (isBoneSaw)
            {

                Vector3 holdPosition = playerCamera.transform.position
                                       + playerCamera.transform.forward * boneSawPositionOffset.z
                                       + playerCamera.transform.right * boneSawPositionOffset.x
                                       + playerCamera.transform.up * boneSawPositionOffset.y;

                pickedUpObject.transform.position = holdPosition;
                pickedUpObject.transform.rotation = playerCamera.transform.rotation * Quaternion.Euler(boneSawRotationOffset);

            }
            else if (isMedical) // Medical syringe
            {
                Vector3 holdPosition = playerCamera.transform.position
                                       + playerCamera.transform.forward * medicalPositionOffset.z
                                       + playerCamera.transform.right * medicalPositionOffset.x
                                       + playerCamera.transform.up * medicalPositionOffset.y;

                pickedUpObject.transform.position = holdPosition;
                pickedUpObject.transform.rotation = playerCamera.transform.rotation * Quaternion.Euler(medicalRotationOffset);
            }
        }
    }

    // Try to pick up an object in front of the player
    void TryPickUpObject()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, pickupRange))
        {
            GameObject hitObject = hit.collider.gameObject;
            // Ensure the hit object is valid before proceeding
            if (hitObject == null)
            {
                Debug.LogError("Raycast hit a null object!");
                return;
            }
            // Check if the hit object is tagged as "Pickup", "Flashlight", "Keycard", "RewrittenKeycard", or "Crowbar"
            if (hit.collider.CompareTag("Pickup") || hit.collider.CompareTag("Flashlight") || hit.collider.CompareTag("Keycard") || hit.collider.CompareTag("RewrittenKeycard") || hit.collider.CompareTag("Crowbar") || hit.collider.CompareTag("SeveredHand") || hit.collider.CompareTag("RewrittenHand") || hit.collider.CompareTag("BoneSaw") || hit.collider.CompareTag("MedSyringe"))
            {
                PickUpObject(hit.collider.gameObject);
            }
        }
    }

    private void BoneSawSawing()
    {
        if (!isBoneSaw) return; // Only proceed if the saw is equipped

        isSawing = true;

        // Define motion parameters
        float sawSpeed = 1f; // Speed of the motion
        float sawRange = 9f; // Range of the motion along the Z-axis (forward and backward)
        Vector3 sawInitialPosition = playerCamera.transform.position
                                     + playerCamera.transform.forward * boneSawPositionOffset.z
                                     + playerCamera.transform.right * boneSawPositionOffset.x
                                     + playerCamera.transform.up * boneSawPositionOffset.y;

        Vector3 forwardPosition = sawInitialPosition + playerCamera.transform.forward * sawRange;
        Vector3 backwardPosition = sawInitialPosition - playerCamera.transform.forward * sawRange;

        // Back-and-forth motion logic
        float pingPong = Mathf.PingPong(Time.time * sawSpeed, 1); // Creates smooth oscillation between 0 and 1
        pickedUpObject.transform.position = Vector3.Lerp(backwardPosition, forwardPosition, pingPong);

        Debug.Log($"Saw Position: {pickedUpObject.transform.position}");

        // Detect objects in range for sawing
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, 2.5f))
        {
            Debug.Log("Sawing: " + hit.collider.name);

            // Play sawing sound
            if (boneSawAudioSource != null && !boneSawAudioSource.isPlaying)
            {
                boneSawAudioSource.Play();
            }
        }
        else
        {
            // Stop sound if no object is in range
            if (boneSawAudioSource != null && boneSawAudioSource.isPlaying)
            {
                boneSawAudioSource.Stop();
            }
        }

        // Stop the manual motion when the mouse button is released
        if (!Input.GetMouseButton(0))
        {
            StopSawMotion();
        }
    }

    private void StopSawMotion()
    {
        if (pickedUpObject != null)
        {
            // Reset the saw to its original position when stopping
            pickedUpObject.transform.localPosition = boneSawPositionOffset;
            Debug.Log("Saw motion stopped.");
        }

        isSawing = false;
    }



    // Crowbar hitting mechanic
    void HitWithCrowbar()
    {
        if (!isCrowbar || isSwinging) return;  // Don't swing if not holding the crowbar or already swinging

        // Start the swing animation
        StartCoroutine(CrowbarSwingAnimation());

        RaycastHit hit;
        Vector3 rayOrigin = playerCamera.transform.position;
        Vector3 rayDirection = playerCamera.transform.forward;

        // Raycast to detect objects in front of the player
        if (Physics.Raycast(rayOrigin, rayDirection, out hit, 2.5f)) // Adjust range as needed
        {
            Debug.Log("Hit: " + hit.collider.name);

            if (crowbarAudioSource != null && impactSound != null)
            {
                crowbarAudioSource.PlayOneShot(impactSound);  // Play the impact sound
                Debug.Log("Playing crowbar impact sound.");
            }

            // Apply force if the object has a Rigidbody
            Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 hitForce = playerCamera.transform.forward * 1000f;  // Adjust force as needed
                rb.AddForce(hitForce);
            }
            // Play hit sound or effects here (optional)
        }
    }

    private IEnumerator UseMedSyringe()
    {
        // Safety check
        if (pickedUpObject == null)
        {
            Debug.LogError("MedSyringe is null. Cannot use.");
            yield break;
        }

        Debug.Log("Using MedSyringe...");
        float animationDuration = 0.5f; // Duration of the downward movement
        Vector3 startPos = pickedUpObject.transform.position;
        Vector3 endPos = startPos + Vector3.down * 5f; // Move downward

        float elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            if (pickedUpObject == null) yield break; // If it gets destroyed, exit

            pickedUpObject.transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / animationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure it reaches final position
        if (pickedUpObject != null)
        {
            pickedUpObject.transform.position = endPos;
        }


        // Ensure it reaches final position
        if (pickedUpObject != null)
        {
            pickedUpObject.transform.position = endPos;
        }

        yield return new WaitForSeconds(0.2f); // Small delay before removing
        pickedUpObject.SetActive(false);

        // Remove from inventory safely
        if (pickedUpObject != null)
        {
            RemoveItemFromInventory(pickedUpObject);
            Destroy(pickedUpObject);
            pickedUpObject = null;
        }

        isMedical = false;

        Debug.Log("MedSyringe used and removed.");
    }



    void ThrowObject()
    {
        // Check if the player is holding an object tagged as "Pickup"
        if (pickedUpObject != null && pickedUpObject.CompareTag("Pickup"))
        {
            // Store a reference to the object before clearing pickedUpObject
            GameObject objectToThrow = pickedUpObject;
            DropObject();

            // Re-enable physics for the object
            Rigidbody rb = objectToThrow.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false; // Re-enable physics
                rb.useGravity = true;    // Ensure gravity is applied so it falls 

                // Re-enable all colliders on the object
                Collider[] colliders = objectToThrow.GetComponentsInChildren<Collider>(true);
                foreach (Collider collider in colliders)
                {
                    collider.enabled = true;  // Enable each collider
                }
                PlayUnequipSound();

                // Apply a forward force to throw the object in the direction the player is facing
                Vector3 throwDirection = playerCamera.transform.forward;  // Throw in the direction the camera is facing
                float throwForce = 10f;  // Adjust this value to change the throw strength
                rb.AddForce(throwDirection * throwForce, ForceMode.VelocityChange);  // Apply the throw force

                // Clear the pickedUpObject reference (player hands are now empty)
                pickedUpObject = null;

                // Now call DropObject to handle the item being removed from the player's hands
                DropObject();
            }
        }
        else
        {
            Debug.Log("No object to throw.");
        }
    }



    public void PickUpObject(GameObject obj)
    {
        // Check if the object is tagged as "PickUp"
        if (obj.CompareTag("Pickup"))
        {
            isPickupObject = true;

            if (playerInteractionUI != null)
            {
                playerInteractionUI.ShowPickupUI();
            }
            // If the player is already holding something (e.g., crowbar or flashlight), deactivate it first
            if (pickedUpObject != null)
            {
                // Deactivate non-throwable items
                if (pickedUpObject.CompareTag("Crowbar"))
                {
                    pickedUpObject.SetActive(false);
                    isCrowbar = false; // Reset the crowbar flag
                }
                else if (pickedUpObject.CompareTag("Flashlight"))
                {
                    flashlightObject.SetActive(false);
                    isFlashlight = false;   // Reset the flashlight flag, but keep reference to flashlightObject
                }
                else if (pickedUpObject.CompareTag("Keycard"))
                {
                    pickedUpObject.SetActive(false);
                    isKeycard = false;   // Reset the flag, but keep reference to tObject
                }
                else if (pickedUpObject.CompareTag("RewrittenKeycard"))
                {
                    pickedUpObject.SetActive(false);
                    isRewrittenKeycard = false;   // Reset the flag, but keep reference to Object
                }
                else if (pickedUpObject.CompareTag("SeveredHand"))
                {
                    pickedUpObject.SetActive(false);
                    isSeveredHand = false;   // Reset the flag, but keep reference to Object
                }
                else if (pickedUpObject.CompareTag("RewrittenHand"))
                {
                    pickedUpObject.SetActive(false);
                    isSeveredHand = false;
                }
                else if (pickedUpObject.CompareTag("BoneSaw"))
                {
                    pickedUpObject.SetActive(false);
                    isBoneSaw = false;
                }
                else if (pickedUpObject.CompareTag("MedSyringe"))
                {
                    pickedUpObject.SetActive(false);
                    isMedical = false;
                }
                pickedUpObject = null;

            }

            // Now directly assign the "Pickup" object to the player's hands (bypass inventory)
            pickedUpObject = obj;

            // Disable physics while holding the object
            Rigidbody rb = pickedUpObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true; // Disable physics so it doesn't fall or collide
            }

            // Disable only BoxColliders to prevent object from interacting with the environment
            Collider[] colliders = pickedUpObject.GetComponentsInChildren<Collider>(true);
            foreach (Collider collider in colliders)
            {
                collider.enabled = false;  // Disable each collider
            }

            // Activate the object in the player's hand
            pickedUpObject.SetActive(true);

            Debug.Log("Picked up object directly into hands: " + pickedUpObject.name);

            return; // Skip the rest of the inventory logic
        }

        // For non-PickUp objects (like flashlight, crowbar, etc.), handle as usual and add them to the inventory
        PlayPickupSound();
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
            flashlightObject = obj; // Save the flashlight reference for future toggling
        }

        // Disable physics while holding the object
        Rigidbody rbObj = obj.GetComponent<Rigidbody>();
        if (rbObj != null)
        {
            rbObj.isKinematic = true; // Disable physics so it doesn't fall or collide
        }

        // Disable only BoxColliders to prevent object from interacting with the environment
        Collider[] objColliders = obj.GetComponentsInChildren<Collider>(true);
        foreach (Collider collider in objColliders)
        {
            collider.enabled = false;  // Disable each BoxCollider
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
        // If the crowbar is currently swinging, stop the swing animation
        if (isCrowbar && isSwinging)
        {
            StopCoroutine(CrowbarSwingAnimation()); // Stop any running swing animation
            isSwinging = false;  // Reset the swing flag
        }

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
            EquipNewItem(newSlot);
        }
    }


    private IEnumerator MoveItemDown(GameObject item, Camera playerCamera)
    {
        if (item == null) yield break;

        // Set the duration and movement distance
        float duration = 0f;  // Adjust the duration as needed
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

    private IEnumerator CrowbarSwingAnimation()
    {
        Debug.Log("CrowbarSwingAnimation: Starting swing animation.");

        // Ensure that the crowbar is still equipped before proceeding
        if (pickedUpObject == null || !isCrowbar)
        {
            Debug.LogWarning("CrowbarSwingAnimation aborted: No crowbar equipped.");
            isSwinging = false;
            yield break; // Exit the coroutine early if there's no crowbar equipped
        }

        isSwinging = true;  // Prevent swinging again during animation

        // Play the swoosh sound when the swing starts
        if (crowbarAudioSource != null && swooshSound != null)
        {
            crowbarAudioSource.PlayOneShot(swooshSound);  // Play the swoosh sound
            Debug.Log("Playing crowbar swoosh sound.");
        }

        // Trigger the camera shake immediately when the swing starts
        if (impulseSource != null)
        {
            impulseSource.GenerateImpulse();
            Debug.Log("CrowbarSwingAnimation: Triggering camera shake.");
        }
        else
        {
            Debug.LogError("Impulse Source is not assigned!");
        }

        // Get the current local rotation of the crowbar
        Quaternion startRotation = pickedUpObject.transform.localRotation;

        // Calculate the target rotation for the forward swing (adjust the swing arc here)
        Quaternion swingRotation = startRotation * Quaternion.Euler(swingRotationOffset);

        float elapsedTime = 0f;
        float swingDurationFull = swingDuration;  // Full duration for the forward motion

        // Animate the swing forward (focus on forward swing)
        while (elapsedTime < swingDurationFull)
        {
            // Smooth transition to swing rotation (forward swing)
            pickedUpObject.transform.localRotation = Quaternion.Slerp(startRotation, swingRotation, elapsedTime / swingDurationFull);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure it reaches the target forward rotation
        pickedUpObject.transform.localRotation = swingRotation;

        // Reset to the original position immediately after the swing
        pickedUpObject.transform.localRotation = startRotation;

        Debug.Log("CrowbarSwingAnimation: Completed forward swing animation.");

        isSwinging = false;  // Allow another swing
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
        isCrowbar = false;
        isSeveredHand = false;
        isBoneSaw = false;
        isMedical= false;

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
        // Check if it's a crowbar
        else if (pickedUpObject.CompareTag("Crowbar"))
        {
            isCrowbar = true;  // Now we know the crowbar is equipped
        }
        else if (pickedUpObject.CompareTag("SeveredHand"))
        {
            isSeveredHand = true;
        }
        else if (pickedUpObject.CompareTag("RewrittenHand"))
        {
            isSeveredHand = true;
        }
        else if (pickedUpObject.CompareTag("BoneSaw"))
        {
            isBoneSaw = true;
        }
        else if (pickedUpObject.CompareTag("MedSyringe"))
        {
            isMedical = true;
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
            // Check if the object is a "Pickup" item (dropable), otherwise unequip the item instead of dropping
            if (pickedUpObject.CompareTag("Pickup"))
            {
                if (playerInteractionUI != null)
                {
                    playerInteractionUI.HidePickupUI();
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

                // Activate the item in the scene
                pickedUpObject.SetActive(true);

                Debug.Log("Dropped: " + pickedUpObject.name);

                pickedUpObject = null;  // Clear the reference to the object so the player is empty-handed

            }
            else
            {
                // For non-PickUp items, unequip instead of dropping
                if (pickedUpObject.CompareTag("Flashlight"))
                {
                    pickedUpObject.SetActive(false);  // Deactivate the flashlight
                    flashlightLight = null;  // Clear flashlight reference
                    isFlashlight = false;    // Reset flashlight flag
                }
                else if (pickedUpObject.CompareTag("Crowbar"))
                {
                    pickedUpObject.SetActive(false);  // Deactivate the crowbar
                    isCrowbar = false;  // Reset crowbar flag
                }
                else if (pickedUpObject.CompareTag("Keycard") || pickedUpObject.CompareTag("RewrittenKeycard"))
                {
                    pickedUpObject.SetActive(false);  // Deactivate keycards
                    isKeycard = false;      // Reset keycard flags
                    isRewrittenKeycard = false;
                }
                else if (pickedUpObject.CompareTag("SeveredHand"))
                {
                    pickedUpObject.SetActive(false);
                    isSeveredHand = false;
                }
                else if (pickedUpObject.CompareTag("RewrittenHand"))
                {
                    pickedUpObject.SetActive(false);
                    isSeveredHand = false;
                }
                else if (pickedUpObject.CompareTag("BoneSaw"))
                {
                    pickedUpObject.SetActive(false);
                    isBoneSaw = false;
                }
                else if (pickedUpObject.CompareTag("MedSyringe"))
                {
                    pickedUpObject.SetActive(false);
                    isMedical = false;
                }

                Debug.Log("Unequipped: " + pickedUpObject.name);

                // Clear the reference to the unequipped object
                pickedUpObject = null;
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
            Debug.Log("Removed item from inventory: " + obj.name);

            // If the removed item was the currently selected one, clear it
            if (pickedUpObject == obj)
            {
                pickedUpObject = null;
            }
        }
        else
        {
            Debug.LogError("Failed to remove MedSyringe: Not found in inventory.");
        }
    }

    public void AddItemToInventory(GameObject obj)
    {
        // Find the next available slot in the inventory
        int slot = GetNextAvailableSlot();
        if (slot == -1)
        {
            Debug.Log("Inventory is full!");
            return;
        }

        // Add the object to the inventory
        inventory.Add(slot, obj);

        Debug.Log("Added object to inventory: " + obj.name + " in slot " + slot);
    }


    private void PlayUnequipSound()
    {
        // Play the unequip sound if available
        if (audioSource != null && unequipSound != null)
        {
            audioSource.PlayOneShot(unequipSound);  // Play the unequip sound immediately
        }
    }

    private void PlayPickupSound()
    {
        // Play the unequip sound if available
        if (audioSourcePickup != null)
        {
            audioSource.PlayOneShot(pickupSound);  // Play the unequip sound immediately
        }
    }

}