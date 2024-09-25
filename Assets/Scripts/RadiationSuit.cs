using UnityEngine;

public class RadiationSuit : MonoBehaviour
{
    [SerializeField] private GameObject visorUI;  // The visor UI (visible only after equipping the suit)
    [SerializeField] private GameObject interactionText;  // UI element for "Press E" interaction prompt

    private bool playerInRange = false; // Track if the player is in range to interact
    private bool suitEquipped = false;  // Track if the suit has been equipped

    public Camera playerCamera;  // Assign the player's camera in the Inspector
    public float interactionRange = 2f;  // The distance within which the player can interact

    void Start()
    {
        // Hide the visor UI and interaction text at the start
        if (visorUI != null)
        {
            visorUI.SetActive(false);
        }
        else
        {
            Debug.LogError("Visor UI not assigned in the Inspector!");
        }

        if (interactionText != null)
        {
            interactionText.SetActive(false);
        }
        else
        {
            Debug.LogError("Interaction text not assigned in the Inspector!");
        }
    }

    void Update()
    {
        if (playerInRange && !suitEquipped) // Only show interaction if suit isn't equipped yet
        {
            // Cast a ray from the center of the player's view to ensure they are looking at the radiation suit
            Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactionRange))
            {
                // Check if the ray is hitting THIS radiation suit's collider
                if (hit.collider != null && hit.collider.gameObject == gameObject)
                {
                    // Show the "Press E" interaction text
                    if (interactionText != null)
                    {
                        interactionText.SetActive(true);
                    }

                    // Check for interaction input (E key)
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        EquipSuit();
                    }
                }
                else
                {
                    // Hide the "Press E" interaction text if not looking at the radiation suit
                    if (interactionText != null)
                    {
                        interactionText.SetActive(false);
                    }
                }
            }
            else
            {
                // Hide the "Press E" interaction text if the raycast doesn't hit anything
                if (interactionText != null)
                {
                    interactionText.SetActive(false);
                }
            }
        }
        else
        {
            // Hide the "Press E" interaction text if the player leaves the range or the suit is equipped
            if (interactionText != null)
            {
                interactionText.SetActive(false);
            }
        }
    }

    // Detect when the player enters the trigger area
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))  // Ensure the player is tagged "Player"
        {
            playerInRange = true;
        }
    }

    // Detect when the player exits the trigger area
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))  // Ensure the player is tagged "Player"
        {
            playerInRange = false;
        }
    }

    // Equip the radiation suit
    void EquipSuit()
    {
        suitEquipped = true;  // Mark the suit as equipped

        // Hide the radiation suit in the game world
        gameObject.SetActive(false);

        // Show the visor UI after the suit is equipped
        if (visorUI != null)
        {
            visorUI.SetActive(true);
        }

        // Hide the interaction text after equipping
        if (interactionText != null)
        {
            interactionText.SetActive(false);
        }

        // Notify the player's health system that the suit is equipped
        PlayerHealth playerHealth = GameObject.FindWithTag("Player").GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.EquipRadiationSuit();
        }
    }
}
