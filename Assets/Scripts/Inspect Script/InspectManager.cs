using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

public class InspectManager : MonoBehaviour
{
    public static InspectManager Instance;

    public GameObject inspectUI; // UI panel to show item details
    public GameObject darkOverlay; // Dark screen background
    public Transform inspectPoint; // Position where item will be placed
    public Text itemNameText;
    public Text descriptionText;
    public float zoomSpeed = 0.5f;
    public float rotationSpeed = 3f;

    public SC_FPSController playerController; // Reference to Player Controller
    public PostProcessVolume postProcessVolume; // Reference to DOF effect
    private DepthOfField depthOfField;
    private ChromaticAberration chromaticAberration;

    private GameObject currentObject;
    private bool isInspecting;
    private Vector3 originalScale;

    private float originalDistance;
    public float minZoomDistance = 0.5f; // Prevents object from going inside camera
    public float maxZoomDistance = -5f; // Prevents object from going too far

    // Crosshair UI
    public GameObject defaultCrosshair; // Assign in Inspector
    public GameObject interactCrosshair; // Assign in Inspector
    private bool isHoveringOverInspectable = false; // Track hover state

    void Awake()
    {
        if (Instance == null)
            Instance = this;

        // Get the Depth of Field effect from the Post Process Volume
        if (postProcessVolume != null)
        {
            postProcessVolume.profile.TryGetSettings(out depthOfField);
        }

        if (postProcessVolume != null)
        {
            postProcessVolume.profile.TryGetSettings(out chromaticAberration);
        }

        SetCrosshair(false);
    }
    void Update()
    {
        if (isInspecting && currentObject != null)
        {
            // Rotate object with mouse drag
            if (Input.GetMouseButton(0)) // Left mouse button
            {
                float rotX = Input.GetAxis("Mouse X") * rotationSpeed;
                float rotY = Input.GetAxis("Mouse Y") * rotationSpeed;

                // Rotate based on camera's view direction
                currentObject.transform.Rotate(Camera.main.transform.up, -rotX, Space.World);
                currentObject.transform.Rotate(Camera.main.transform.right, rotY, Space.World);
            }

            // Zoom: Move object towards the camera with clamped distance
            float scroll = Input.GetAxis("Mouse ScrollWheel");

            if (scroll != 0)
            {
                Vector3 directionToCamera = (Camera.main.transform.position - currentObject.transform.position).normalized;
                float currentDistance = Vector3.Distance(currentObject.transform.position, Camera.main.transform.position);

                float newDistance = Mathf.Clamp(currentDistance - (scroll * zoomSpeed), minZoomDistance, maxZoomDistance);

                // Only update position if it stays within allowed range
                if (newDistance >= minZoomDistance && newDistance <= maxZoomDistance)
                {
                    currentObject.transform.position = Camera.main.transform.position - (directionToCamera * newDistance);
                }
            }

            // Adjust Depth of Field dynamically
            if (depthOfField != null)
            {
                float distanceToCamera = Vector3.Distance(currentObject.transform.position, Camera.main.transform.position);
                depthOfField.focusDistance.value = Mathf.Clamp(distanceToCamera, 0.5f, 10f);
            }

            // Exit inspect mode with Escape or Right-Click
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
            {
                ExitInspect();
            }
        }
        else
        {
            CheckForInspectableObject(); // Continuously check if looking at an inspectable object
        }
    }

    private void CheckForInspectableObject()
    {
        Ray r = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        if (Physics.Raycast(r, out RaycastHit hitInfo, 2f)) // Short range for interaction
        {
            // 🔹 Only show the interact crosshair if the object is an InspectableObject
            if (hitInfo.collider.gameObject.TryGetComponent(out InspectableObject _))
            {
                if (!isHoveringOverInspectable)
                {
                    SetCrosshair(true); // Show interact crosshair, hide default crosshair
                    isHoveringOverInspectable = true;
                }
            }
            else
            {
                if (isHoveringOverInspectable)
                {
                    SetCrosshair(false); // Restore default crosshair
                    isHoveringOverInspectable = false;
                }
            }
        }
        else
        {
            if (isHoveringOverInspectable)
            {
                SetCrosshair(false); // Restore default crosshair
                isHoveringOverInspectable = false;
            }
        }
    }


    private void SetCrosshair(bool isHovering)
    {
        if (defaultCrosshair != null && interactCrosshair != null)
        {
            defaultCrosshair.SetActive(!isHovering);
            interactCrosshair.SetActive(isHovering);
        }
    }

    public void InspectItem(InspectableObject item)
    {
        if (isInspecting) return;

        isInspecting = true;
        inspectUI.SetActive(true);
        itemNameText.text = item.itemName;
        descriptionText.text = item.description;

        SetCrosshair(false);
        defaultCrosshair.SetActive(false);

        // Pause the game
        Time.timeScale = 0f;

        // Disable player movement & looking
        if (playerController != null)
        {
            playerController.canMove = false;
        }

        // Unlock and show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Enable Depth of Field
        if (depthOfField != null)
        {
            depthOfField.active = true;
        }

        if (chromaticAberration != null)
        {
            chromaticAberration.active = true;
            chromaticAberration.intensity.value = 0.337f;

        }

        // Instantiate the object for inspection
        currentObject = Instantiate(item.gameObject);
        currentObject.transform.position = inspectPoint.position;
        currentObject.transform.LookAt(Camera.main.transform);
        currentObject.transform.Rotate(0, 180, 0); // Flip it if needed

        // If it's a Quad, make sure it's facing correctly
        if (currentObject.GetComponent<MeshFilter>().sharedMesh.name.Contains("Quad"))
        {
            currentObject.transform.Rotate(0, 0, 0);
        }

        // Fix scaling issue
        originalScale = item.transform.localScale;
        currentObject.transform.localScale = originalScale * 2f;

        originalDistance = Vector3.Distance(currentObject.transform.position, Camera.main.transform.position);
        minZoomDistance = originalDistance * 0.5f; // Allow a little closer, but not inside the camera
        maxZoomDistance = originalDistance;

        // Disable physics & collider
        if (currentObject.TryGetComponent<Rigidbody>(out Rigidbody rb))
            Destroy(rb);
        if (currentObject.TryGetComponent<Collider>(out Collider col))
            col.enabled = false;

    }




    public void ExitInspect()
    {
        isInspecting = false;
        inspectUI.SetActive(false);
        SetCrosshair(true);
        defaultCrosshair.SetActive(true);

        // Resume game
        Time.timeScale = 1f;

        // Enable player movement & looking again
        if (playerController != null)
        {
            playerController.canMove = true;
        }

        if (depthOfField != null)
        {
            depthOfField.active = false;
        }

        if (chromaticAberration != null)
        {
            chromaticAberration.active = false;
            chromaticAberration.intensity.value = 0f;
        }

        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Destroy(currentObject);
    }
}
