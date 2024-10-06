using UnityEngine;
using System.Collections;

public class InsideElevatorButton : MonoBehaviour, IInteractable
{
    public ElevatorController elevator;       // Reference to the ElevatorController script
    public AudioSource buttonSound;           // Optional: Sound for when the button is pressed
    public AudioSource elevatorMovingSound;   // Sound to play when the elevator is moving
    public float moveDelay = 2f;              // Delay before the elevator starts moving
    private bool buttonPressed = false;       // Track if the button has been pressed

    public Animator elevatorAnimator;         // Animator to control the elevator movement (upward animation)
    public Transform elevatorTransform;       // Reference to the elevator's Transform (the moving platform)

    private GameObject player;                // Reference to the player
    private Rigidbody playerRigidbody;        // Reference to the player's Rigidbody

    public void Interact()
    {
        if (!buttonPressed)  // Only allow interaction once
        {
            buttonPressed = true;

            if (buttonSound != null)
            {
                buttonSound.Play();  // Play button press sound
            }

            // Parent the player to the elevator immediately after pressing the button
            if (player != null)
            {
                player.transform.SetParent(elevatorTransform);  // Set the player's parent to the elevator

                // Set the player's Rigidbody to kinematic to avoid physics issues
                if (playerRigidbody != null)
                {
                    playerRigidbody.isKinematic = true;  // Make the player's Rigidbody kinematic
                }

                Debug.Log("Player has been parented to the elevator and Rigidbody is now kinematic.");
            }

            // Start the elevator movement with a delay
            StartCoroutine(MoveElevatorWithDelay());
        }
    }

    IEnumerator MoveElevatorWithDelay()
    {
        elevator.CloseDoors();

        // Wait for the delay before starting the elevator movement
        yield return new WaitForSeconds(moveDelay);

        // Start the elevator movement (trigger animation)
        StartElevatorMovement();

        // Wait for the elevator to reach its destination, then open the doors
        yield return new WaitUntil(() => ElevatorHasReachedDestination());

        // Stop the elevator moving sound when the elevator reaches the destination
        if (elevatorMovingSound != null && elevatorMovingSound.isPlaying)
        {
            elevatorMovingSound.Stop();
        }

        // After the movement, open the doors
        OpenElevatorDoors();
    }

    void StartElevatorMovement()
    {
        // Trigger the elevator moving upwards animation
        elevatorAnimator.SetTrigger("MoveUp");
        Debug.Log("Elevator is moving upwards.");

        // Play the elevator moving sound when the elevator starts moving
        if (elevatorMovingSound != null)
        {
            elevatorMovingSound.Play();
        }
    }

    void OpenElevatorDoors()
    {
        Debug.Log("Opening elevator doors...");

        // Play the door opening animation
        elevator.OpenDoors();
    }

    // Helper method to determine when the elevator has finished moving
    private bool ElevatorHasReachedDestination()
    {
        // Check if the elevator movement animation has finished
        return elevatorAnimator.GetCurrentAnimatorStateInfo(0).IsName("ElevatorIdleAtDestination");
    }

    // Assign the player when the button script is loaded
    private void Start()
    {
        player = GameObject.FindWithTag("Player");  // Find the player in the scene by tag
        if (player == null)
        {
            Debug.LogError("Player not found! Make sure the player has a 'Player' tag.");
        }
        else
        {
            playerRigidbody = player.GetComponent<Rigidbody>();  // Get the player's Rigidbody
        }
    }
}
