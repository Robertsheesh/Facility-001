using UnityEngine;

public class LeverControl : MonoBehaviour
{
    public Animator leverAnimator;
    public int leverState = 1; // Start at position 1 (Top)
    public LeverManager leverManager; // Reference to the lever manager
    public AudioSource leverSound;

    private bool playerInRange = false; // Track if the player is in range to interact
    private bool leverUsed = false; // Track if the lever has already been used
    public Camera playerCamera; // Assign the player's camera in the Inspector
    public float interactionRange = 2f; // Range of the interaction raycast

    [SerializeField] private GameObject interactionText; // "Press E" interaction UI

    void Start()
    {
        if (leverAnimator == null)
        {
            Debug.LogError("No Animator assigned to the LeverControl script.");
        }

        if (leverManager == null)
        {
            leverManager = FindObjectOfType<LeverManager>(); // Automatically find the manager
        }

        // Hide the interaction text at the start
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
        // Only allow interaction if the player is in range
        if (playerInRange && !leverUsed)
        {
            // Cast a ray from the center of the player's view to ensure they are looking at the lever
            Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactionRange))
            {
                // Check if the ray is hitting THIS lever's collider
                if (hit.collider != null && hit.collider.gameObject == gameObject)
                {
                    // Show the "Press E" interaction text
                    if (interactionText != null)
                    {
                        interactionText.SetActive(true);
                    }

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        // Cycle through the lever states
                        leverState = (leverState % 3) + 1; // Cycle between 1, 2, and 3 (Top, Middle, Bottom)
                        leverAnimator.SetInteger("LeverState", leverState);

                        // Notify the manager of the state change
                        if (leverManager != null)
                        {
                            leverManager.UpdateLeverState(this);
                        }

                        StartLeverSound();
                        leverUsed = true; // Mark the lever as used

                        // Hide the interaction text once the lever has been used
                        if (interactionText != null)
                        {
                            interactionText.SetActive(false);
                        }
                    }
                }
                else
                {
                    // Hide the interaction text if the player is not looking at the lever
                    if (interactionText != null)
                    {
                        interactionText.SetActive(false);
                    }
                }
            }
            else
            {
                // Hide the interaction text if the raycast doesn't hit anything
                if (interactionText != null)
                {
                    interactionText.SetActive(false);
                }
            }
        }
        else
        {
            // Hide the "Press E" interaction text if the player leaves the range or the lever has been used
            if (interactionText != null)
            {
                interactionText.SetActive(false);
            }
        }
    }

    // Detect when the player enters the trigger area
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))  // Assuming the player is tagged as "Player"
        {
            playerInRange = true;
        }
    }

    // Detect when the player exits the trigger area
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))  // Assuming the player is tagged as "Player"
        {
            playerInRange = false;

            // Hide the interaction text when leaving the range
            if (interactionText != null)
            {
                interactionText.SetActive(false);
            }
        }
    }

    void StartLeverSound()
    {
        if (leverSound != null)
        {
            leverSound.Play();  // Start playing the lever sound
        }
    }
}
