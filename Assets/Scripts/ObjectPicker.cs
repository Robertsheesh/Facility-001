using UnityEngine;

public class ObjectPicker : MonoBehaviour
{
    public Camera playerCamera;         // Main player camera
    public float pickupRange = 3f;      // The range at which the player can pick up objects
    public Vector3 holdOffset = new Vector3(0f, -0.5f, 1.5f); // Position offset for other objects in front of the camera
    public GameObject pickedUpObject = null;

    [Header("Flashlight Settings")]
    public Vector3 flashlightPositionOffset = new Vector3(0.5f, -0.5f, 1.5f); // Position offset for the flashlight
    public Vector3 flashlightRotationOffset = new Vector3(0f, 0f, 0f);        // Rotation offset for the flashlight
    private Light flashlightLight; // Reference to the flashlight's light component
    private bool isFlashlightOn = true; // Track whether the flashlight is currently on

    private bool isFlashlight = false;  // Track if the picked-up object is a flashlight

    public AudioSource flashlightSound;

    void Update()
    {
        // Detect input for picking up the object (e.g., pressing E)
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (pickedUpObject == null)
            {
                TryPickUpObject();  // Try to pick up a new object
            }
        }

        // Detect input for dropping the object (press G to drop)
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (pickedUpObject != null)
            {
                DropObject();  // Drop the object
            }
        }

        // If an object is picked up, update its position and rotation
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
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, pickupRange))
        {
            // Check if the hit object is tagged as "Pickup" or "Flashlight"
            if (hit.collider.CompareTag("Pickup") || hit.collider.CompareTag("Flashlight"))
            {
                PickUpObject(hit.collider.gameObject);
            }
        }
    }

    // Pick up the object and place it in front of the player's view
    void PickUpObject(GameObject obj)
    {
        pickedUpObject = obj;

        // Check if it's a flashlight and store the result in isFlashlight
        isFlashlight = pickedUpObject.CompareTag("Flashlight");

        // Disable physics while holding the object
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // Disable physics so it doesn't fall or collide
        }

        // Disable the collider to prevent the object from pushing the player
        Collider objCollider = obj.GetComponent<Collider>();
        if (objCollider != null)
        {
            objCollider.enabled = false;  // Disable collider while holding
        }

        // Set the object's position manually relative to the camera, not as a child
        obj.transform.SetParent(null); // Ensure it isn't parented to the camera

        // If it's a flashlight, get the Light component
        if (isFlashlight)
        {
            flashlightLight = obj.GetComponentInChildren<Light>(); // Assuming the flashlight has a Light component
            isFlashlightOn = flashlightLight != null && flashlightLight.enabled;
        }

        // Adjust object's position to be in front of the camera
        Vector3 holdPosition = playerCamera.transform.position + playerCamera.transform.forward * 1.5f; // 1.5 units in front of the camera
        obj.transform.position = holdPosition;

        // Custom handling for the flashlight
        if (isFlashlight)
        {
            // Ensure the flashlight is aligned with the camera and applies custom offsets
            obj.transform.rotation = playerCamera.transform.rotation * Quaternion.Euler(flashlightRotationOffset);
        }
        else
        {
            // Make sure the object stays upright and doesn't rotate with the camera's pitch
            obj.transform.rotation = Quaternion.Euler(0, playerCamera.transform.eulerAngles.y, 0); // Only align with the camera's yaw (left-right)
        }
    }

    // Drop the currently held object
    void DropObject()
    {
        if (pickedUpObject != null)
        {
            // Re-enable physics so the object falls naturally
            Rigidbody rb = pickedUpObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false; // Re-enable physics
            }

            // Re-enable the collider when dropping the object
            Collider objCollider = pickedUpObject.GetComponent<Collider>();
            if (objCollider != null)
            {
                objCollider.enabled = true;  // Re-enable collider when dropped
            }

            pickedUpObject.transform.SetParent(null);  // Unparent the object from the camera

            pickedUpObject = null;  // Clear the reference to the object so it stops floating
            isFlashlight = false;   // Reset the flashlight flag
            flashlightLight = null; // Clear flashlight reference
        }
    }

    // Toggle the flashlight on/off
    void ToggleFlashlight()
    {
        if (flashlightLight != null)
        {
            isFlashlightOn = !isFlashlightOn;
            flashlightLight.enabled = isFlashlightOn;
            PlayFlashlightSound();
        }
    }

    void PlayFlashlightSound()
    {
        if (flashlightSound != null)
        {
            flashlightSound.Play();  // play the flashlight toggle sound
        }
    }
}
