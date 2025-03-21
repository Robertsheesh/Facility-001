using UnityEngine;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour
{
    public Camera playerCamera;
    public float raycastRange = 2f; // Range of interaction

    // Crosshair images
    public Image defaultCrosshair;
    public Image interactCrosshair;
    public GameObject inspectCrosshair;
    public Text PressE;
    public Text PressLMB;

    public GameObject ThrowUI;

    public ComputerInteraction computerCameraSwitcher;
    public ComputerCameraSwitcher computerCameraSwitcher1;

    private bool wasUsingComputer = false; // Track previous computer usage state

    void Start()
    {
        // Ensure the correct crosshair is active at start
        SetCrosshairDefault();
        ThrowUI.SetActive(false);
    }

    void Update()
    {
        bool isCurrentlyUsingComputer = IsUsingComputer();

        // If the player just started using the computer, disable UI
        if (isCurrentlyUsingComputer && !wasUsingComputer)
        {
            DisableInteractionUI();
        }
        // If the player just stopped using the computer, re-enable UI
        else if (!isCurrentlyUsingComputer && wasUsingComputer)
        {
            SetCrosshairDefault();
        }

        wasUsingComputer = isCurrentlyUsingComputer; // Update state

        if (isCurrentlyUsingComputer) return; // Prevent interactions while using a computer

        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, raycastRange))
        {
            if (hit.collider != null)
            {
                if (IsInteractable(hit.collider.tag))
                {
                    SetCrosshairInteract();
                    HandleInteraction(hit);
                }
                else if (IsComputer(hit.collider.tag))
                {
                    SetCrosshairInspect();
                    HandleInteraction(hit);
                }
                else
                {
                    SetCrosshairDefault();
                }
            }
            else
            {
                SetCrosshairDefault();
            }
        }
        else
        {
            SetCrosshairDefault();
        }
    }

    bool IsInteractable(string tag)
    {
        return tag == "Door" || tag == "Button" || tag == "Pickup" || tag == "Valve" || tag == "Fuel" ||
               tag == "Lever" || tag == "Flashlight" || tag == "Keycard" ||
               tag == "RewrittenKeycard" || tag == "RadiationSuit" || tag == "Crowbar" ||
               tag == "MedSyringe" || tag == "VentObject" || tag == "AirlockLever";
    }

    bool IsComputer(string tag)
    {
        return tag == "Computer" || tag == "DoorSummer";
    }

    bool IsUsingComputer()
    {
        return (computerCameraSwitcher != null && computerCameraSwitcher.isUsingComputer) ||
               (computerCameraSwitcher1 != null && computerCameraSwitcher1.isUsingComputer);
    }

    void HandleInteraction(RaycastHit hit)
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            switch (hit.collider.tag)
            {
                case "Door":
                    Debug.Log("Opening door: " + hit.collider.name);
                    break;
                case "Button":
                    Debug.Log("Pressing button: " + hit.collider.name);
                    break;
                case "Pickup":
                    Debug.Log("Picking up item: " + hit.collider.name);
                    break;
                case "Valve":
                    Debug.Log("Opening valve: " + hit.collider.name);
                    break;
                case "Fuel":
                    Debug.Log("Inserting fuel: " + hit.collider.name);
                    break;
                case "Lever":
                    Debug.Log("Turning lever: " + hit.collider.name);
                    break;
                case "Flashlight":
                    Debug.Log("Picking up flashlight: " + hit.collider.name);
                    break;
                case "Keycard":
                case "RewrittenKeycard":
                    Debug.Log("Picking up keycard: " + hit.collider.name);
                    break;
                case "RadiationSuit":
                    Debug.Log("Picking up radiation suit: " + hit.collider.name);
                    break;
                case "Crowbar":
                    Debug.Log("Picking up crowbar: " + hit.collider.name);
                    break;
                case "MedSyringe":
                    Debug.Log("Picking up Medical: " + hit.collider.name);
                    break;
                case "VentObject":
                    Debug.Log("Pushing vent cover: " + hit.collider.name);
                    break;
                case "AirlockLever":
                    Debug.Log("Turning airlock lever: " + hit.collider.name);
                    break;
            }
        }

        if (hit.collider.CompareTag("Computer") && Input.GetMouseButtonDown(0))
        {
            Debug.Log("Using computer: " + hit.collider.name);
        }
    }

   public void SetCrosshairInteract()
    {
        if (defaultCrosshair != null) defaultCrosshair.enabled = false;
        if (interactCrosshair != null) interactCrosshair.enabled = true;
        inspectCrosshair.SetActive(false);
    }

    public void SetCrosshairInspect()
    {
        if (defaultCrosshair != null) defaultCrosshair.enabled = false;
        if (interactCrosshair != null) interactCrosshair.enabled = false;
        inspectCrosshair.SetActive(true);

    }

    public void SetCrosshairDefault()
    {
        if (defaultCrosshair != null) defaultCrosshair.enabled = true;
        if (interactCrosshair != null) interactCrosshair.enabled = false;
        inspectCrosshair.SetActive(false);
    }

    public void ShowPickupUI()
    {
        ThrowUI.SetActive(true);
    }

    public void HidePickupUI()
    {
        ThrowUI.SetActive(false);
    }

    void DisableInteractionUI()
    {
        if (defaultCrosshair != null) defaultCrosshair.enabled = false;
        if (interactCrosshair != null) interactCrosshair.enabled = false;
        if (inspectCrosshair != null) inspectCrosshair.SetActive(false);
        if (ThrowUI != null) ThrowUI.SetActive(false);
    }
}
