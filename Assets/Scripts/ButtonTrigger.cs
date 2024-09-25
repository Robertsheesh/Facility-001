using UnityEngine;

public class ButtonTrigger : MonoBehaviour
{
    private SlidingDoorController doorController;
    private bool playerInRange = false;

    void Start()
    {
        doorController = FindObjectOfType<SlidingDoorController>();
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            doorController.CanisterPickedUp(); // Close the door and spawn a new canister
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("Player is near the button.");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log("Player left the button area.");
        }
    }
}
