using UnityEngine;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour
{
    public Camera playerCamera;  // Assign the player's camera in the Inspector
    public float raycastRange = 2f; // Range of the raycast

    // UI Text elements for each type of interaction
    public Text doorInteractionText;
    public Text buttonInteractionText;
    public Text pickupInteractionText;
    public Text valveInteractionText;
    public Text fuelInteractionText;
    public Text leverInteractionText;

    // New UI Text elements for flashlight, computer, and radiation suit
    public Text flashlightInteractionText;
    public Text computerInteractionText;
    public Text radiationSuitInteractionText;
    public Text crowBarInteractionText;
    public Text keyCardInteractionText;

    public GameObject ThrowUI;

    public void Start()
    {
        // Initially hide all interaction texts
        HideAllInteractionTexts();
        ThrowUI.SetActive(false);
    }

    void Update()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        RaycastHit hit;

        // Cast a ray from the center of the camera forward
        if (Physics.Raycast(ray, out hit, raycastRange))
        {
            // Check if the object has a tag for interaction
            if (hit.collider != null)
            {
                // Check if it's a door
                if (hit.collider.CompareTag("Door"))
                {
                    ShowInteractionText(doorInteractionText, "Interact: [E]");
                    HandleDoorInteraction(hit);
                }
                // Check if it's a button
                else if (hit.collider.CompareTag("Button"))
                {
                    ShowInteractionText(buttonInteractionText, "Press button: [E]");
                    HandleButtonInteraction(hit);
                }
                // Check if it's a pickup item
                else if (hit.collider.CompareTag("Pickup"))
                {
                    ShowInteractionText(pickupInteractionText, "Pick up item: [E]");
                    HandlePickupInteraction(hit);
                }
                // Check if it's a valve
                else if (hit.collider.CompareTag("Valve"))
                {
                    ShowInteractionText(valveInteractionText, "Open valve: [E]");
                    HandleValveInteraction(hit);
                }
                // Check if it's a fuel insertable object
                else if (hit.collider.CompareTag("Fuel"))
                {
                    ShowInteractionText(fuelInteractionText, "Insert fuel: [E]");
                    HandleFuelInteraction(hit);
                }
                // Check if it's a lever
                else if (hit.collider.CompareTag("Lever"))
                {
                    ShowInteractionText(leverInteractionText, "Turn lever: [E]");
                    HandleLeverInteraction(hit);
                }
                // Check if it's a flashlight
                else if (hit.collider.CompareTag("Flashlight"))
                {
                    ShowInteractionText(flashlightInteractionText, "Pick up flashlight: [E]");
                    HandleFlashlightInteraction(hit);
                }
                // Check if it's a computer
                else if (hit.collider.CompareTag("Computer"))
                {
                    ShowInteractionText(computerInteractionText, "Use computer: [LMB]");
                    HandleComputerInteraction(hit);
                }
                // Check if it's a keycard
                else if (hit.collider.CompareTag("Keycard"))
                {
                    ShowInteractionText(keyCardInteractionText, "Pick up keycard: [E]");
                    HandlekeyCardInteraction(hit);
                }
                // Check if it's a keycard
                else if (hit.collider.CompareTag("RewrittenKeycard"))
                {
                    ShowInteractionText(keyCardInteractionText, "Pick up keycard: [E]");
                    HandlekeyCardInteraction(hit);
                }
                // Check if it's a radiation suit
                else if (hit.collider.CompareTag("RadiationSuit"))
                {
                    ShowInteractionText(radiationSuitInteractionText, "Pick up radiation suit: [E]");
                    HandleRadiationSuitInteraction(hit);
                }
                // Check if it's a crowbar
                else if (hit.collider.CompareTag("Crowbar"))
                {
                    ShowInteractionText(crowBarInteractionText, "Pick up crowbar: [E]");
                    HandlecrowBarInteraction(hit);
                }
                else
                {
                    HideAllInteractionTexts(); // Hide if not interactable
                }
            }
            else
            {
                HideAllInteractionTexts(); // Hide if raycast hits nothing
            }
        }
        else
        {
            HideAllInteractionTexts(); // Hide if raycast hits nothing
        }
    }

    // Function to handle the door interaction
    void HandleDoorInteraction(RaycastHit hit)
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Opening door: " + hit.collider.name);
            // Add door open logic here
        }
    }

    // Function to handle the button interaction
    void HandleButtonInteraction(RaycastHit hit)
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Pressing button: " + hit.collider.name);
            // Add button press logic here
        }
    }

    // Function to handle the pickup interaction
    void HandlePickupInteraction(RaycastHit hit)
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Picking up item: " + hit.collider.name);
            // Add pickup logic here
        }
    }

    // Function to handle the valve interaction
    void HandleValveInteraction(RaycastHit hit)
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Opening valve: " + hit.collider.name);
            // Add valve interaction logic here
        }
    }

    // Function to handle the fuel interaction
    void HandleFuelInteraction(RaycastHit hit)
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Inserting fuel: " + hit.collider.name);
            // Add fuel insertion logic here
        }
    }

    // Function to handle the lever interaction
    void HandleLeverInteraction(RaycastHit hit)
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Turning lever: " + hit.collider.name);
            // Add lever turn logic here
        }
    }

    // Function to handle the flashlight interaction
    void HandleFlashlightInteraction(RaycastHit hit)
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Picking up flashlight: " + hit.collider.name);
            // Add flashlight pickup logic here
        }
    }

    // Function to handle the computer interaction
    void HandleComputerInteraction(RaycastHit hit)
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Using computer: " + hit.collider.name);
            // Add computer usage logic here
        }
    }

    // Function to handle the radiation suit interaction
    void HandleRadiationSuitInteraction(RaycastHit hit)
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Picking up radiation suit: " + hit.collider.name);
            // Add radiation suit pickup logic here
        }
    }

    void HandlecrowBarInteraction(RaycastHit hit)
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Picking up crowbar: " + hit.collider.name);
        }
    }

    void HandlekeyCardInteraction(RaycastHit hit)
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Picking up keycard: " + hit.collider.name);
        }
    }

    // Show the appropriate interaction text
   public void ShowInteractionText(Text interactionText, string message)
    {
        HideAllInteractionTexts(); // Hide all other texts first
        interactionText.text = message;
        interactionText.enabled = true;  // Show the relevant interaction text
    }

    // Hide all interaction texts
    public void HideAllInteractionTexts()
    {
        doorInteractionText.enabled = false;
        buttonInteractionText.enabled = false;
        pickupInteractionText.enabled = false;
        valveInteractionText.enabled = false;
        fuelInteractionText.enabled = false;
        leverInteractionText.enabled = false;
        flashlightInteractionText.enabled = false;
        computerInteractionText.enabled = false;  // This should hide the computer interaction text
        radiationSuitInteractionText.enabled = false;
        crowBarInteractionText.enabled = false;
        keyCardInteractionText.enabled = false;
    }

    // Call this method when an object is picked up
    public void ShowPickupUI()
    {
        ThrowUI.SetActive(true);
    }

    // Call this method when an object is dropped or no longer held
    public void HidePickupUI()
    {
        ThrowUI.SetActive(false);
    }

}
