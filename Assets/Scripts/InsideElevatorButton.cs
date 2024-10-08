using UnityEngine;
using System.Collections;

public class InsideElevatorButton : MonoBehaviour, IInteractable
{
    public ElevatorController elevator;       // Reference to the ElevatorController script
    public AudioSource buttonSound;           // Optional: Sound for when the button is pressed
    public AudioSource elevatorMovingSound;   // Sound to play when the elevator is moving
    public float moveDelay = 2f;              // Delay before the elevator starts moving
    public float elevatorTravelTime = 26f;    // Time it takes for the elevator to reach its destination
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

        // Wait for the duration of the elevator travel time before opening the doors
        yield return new WaitForSeconds(elevatorTravelTime);

        // After the movement, open the doors
        OpenElevatorDoors();
        Debug.Log("Opening elevator doors at destination");

        // Unparent the player after the elevator has stopped and doors are open
        UnparentPlayer();
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

    void UnparentPlayer()
    {
        // Unparent the player and re-enable physics
        if (player != null)
        {
            player.transform.SetParent(null);  // Reset the player's parent to null

            // Re-enable the player's Rigidbody physics
            if (playerRigidbody != null)
            {
                playerRigidbody.isKinematic = false;  // Make the player's Rigidbody non-kinematic again
            }

            Debug.Log("Player is unparented from the elevator and Rigidbody physics is restored.");
        }
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
