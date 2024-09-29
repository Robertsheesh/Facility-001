using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Cinemachine;

public class MonsterAI : MonoBehaviour
{
    public NavMeshAgent agent;
    public List<Transform> waypoints; // List of waypoints for patrolling
    private int currentWaypointIndex = -1; // Index to track the current waypoint

    public Transform player; // Reference to the player
    public Transform monsterHand; // Reference to the monster's hand bone
    public float detectionRange = 10f; // Range within which the AI detects the player
    public float chasingDistance = 15f; // Max distance for chasing the player
    public float attackDistance = 2f; // Distance within which the monster will perform the choke lift attack
    public float agonizingRange = 20f;  // Range within which the monster starts agonizing
    public float chaseSpeed = 6f; // Speed while chasing the player
    public float patrolSpeed = 3.5f; // Speed during patrolling
    public float avoidWallDistance = 2f; // Minimum distance from walls while patrolling

    public CinemachineVirtualCamera playerCam; // Reference to player's main Cinemachine camera
    public CinemachineVirtualCamera chokeLiftCam; // Reference to the camera attached to the monster's hand for choke lift

    private bool isChasing = false; // Whether the AI is currently chasing the player
    private bool isAgonizing = false; // Whether the monster is currently agonizing
    private bool hasAgonized = false; // Ensure monster agonizes only once per encounter
    private bool isAttacking = false; // Whether the monster is currently performing the attack
    private float agonizingTimer = 0f; // Timer to track how long the monster has been agonizing
    public float agonizingDuration = 10f; // Maximum duration for agonizing

    public LayerMask obstacleMask; // Layer mask for objects that block the line of sight (e.g., walls)

    private Animator animator; // Reference to the Animator component

    public float agonizingDamageRange = 10f; // Range in which the player is affected by agonizing
    public AudioSource screamAudioSource; // The audio source for the monster's scream
    public AudioClip agonizeClip; // The sound clip for agonizing

    private PlayerAgonizeEffect playerAgonizeEffect; // Reference to the PlayerAgonizeEffect script

    private bool chokeCooldownActive = false; // Tracks if the choke lift is on cooldown
    public float chokeCooldownDuration = 10f; // The duration of the cooldown in seconds
    public float safeDistance = 3f; // Safe distance the player needs to be away from the monster after being thrown

    private CharacterController playerController; // Reference to player's CharacterController

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = patrolSpeed; // Start patrolling with patrol speed

        animator = GetComponent<Animator>(); // Get the Animator component

        // Ensure the PlayerAgonizeEffect is assigned
        playerAgonizeEffect = player.GetComponent<PlayerAgonizeEffect>();
        if (playerAgonizeEffect == null)
        {
            Debug.LogError("PlayerAgonizeEffect script not found on the player.");
        }

        if (monsterHand == null)
        {
            Debug.LogError("Monster's hand bone is not assigned.");
        }

        if (playerCam == null || chokeLiftCam == null)
        {
            Debug.LogError("Cinemachine cameras are not assigned.");
        }

        // Assign the agonize clip to the AudioSource if not done through the Inspector
        if (screamAudioSource != null && agonizeClip != null)
        {
            screamAudioSource.clip = agonizeClip;
        }

        // Ensure the player's main camera is active at the start
        playerCam.Priority = 10; // Higher priority means it's active
        chokeLiftCam.Priority = 0; // Ensure this is initially inactive

        // Start patrolling to a random waypoint
        GotoNextWaypoint();
    }

    void Update()
    {
        AvoidWallHugging();

        if (isAgonizing || isAttacking || chokeCooldownActive)
        {
            // If agonizing, count the time and end it after the agonizing duration
            if (isAgonizing)
            {
                agonizingTimer += Time.deltaTime;
                if (agonizingTimer >= agonizingDuration)
                {
                    EndAgonizing();
                }
            }

            // Skip all other logic while agonizing, attacking, or in cooldown
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Trigger agonizing if the player enters detection range and the monster hasn't agonized yet
        if (distanceToPlayer <= detectionRange && !hasAgonized)
        {
            StartAgonizing();
            return;
        }

        // After agonizing, if the player is within chasing distance, start chasing the player
        if (hasAgonized && !isChasing && distanceToPlayer <= chasingDistance && distanceToPlayer > safeDistance)
        {
            isChasing = true; // Start chasing the player
            agent.speed = chaseSpeed; // Increase speed when chasing
        }

        // If the player runs away beyond chasing distance or after the chase begins, stop chasing and return to patrol
        if (isChasing && distanceToPlayer > chasingDistance)
        {
            isChasing = false; // Stop chasing if the player runs away
            agent.speed = patrolSpeed; // Return to normal patrol speed
        }

        // Handle close-range attack (Superhuman Choke Lift)
        if (isChasing && distanceToPlayer <= attackDistance && !chokeCooldownActive)
        {
            StartChokeLift();
            return; // Skip other logic while the attack is performed
        }

        // Update the Animator based on the chasing state
        animator.SetBool("isChasing", isChasing);

        if (isChasing)
        {
            // Chase the player
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    void Patrol()
    {
        // If the agent has reached the current waypoint, go to the next one
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            GotoNextWaypoint();
        }
    }

    void GotoNextWaypoint()
    {
        if (waypoints.Count == 0)
            return;

        // Choose a random waypoint from the list
        currentWaypointIndex = Random.Range(0, waypoints.Count);

        // Set the agent to go to the selected random waypoint
        agent.SetDestination(waypoints[currentWaypointIndex].position);
    }

    void ChasePlayer()
    {
        if (player != null)
        {
            agent.SetDestination(player.position); // Chase the player
        }
    }

    void StartChokeLift()
    {
        // Stop the agent and trigger the choke lift animation
        isAttacking = true;
        agent.isStopped = true;

        // Trigger the choke lift animation
        animator.SetTrigger("ChokeLift");

        // Disable player movement (but don't physically move the player)
        DisablePlayerMovement();

        // Switch to the ChokeLiftCam to simulate the choke (the player model stays on the ground)
        SwitchToChokeLiftCam();

        // Schedule the end of the attack (adjust timing based on your animation length)
        Invoke("EndChokeLift", 3f); // Adjust this duration based on the animation length
    }

    void EndChokeLift()
    {
        isAttacking = false;

        // Unparent the player from the monster's hand
        player.SetParent(null);

        // Reposition the player a small distance away from the monster to simulate the throw
        Vector3 throwDirection = (player.position - transform.position).normalized;
        float throwDistance = 2f;

        // Use CharacterController.Move to move the player properly
        if (playerController != null)
        {
            playerController.enabled = true;  // Make sure the CharacterController is enabled
            playerController.Move(throwDirection * throwDistance); // Apply the movement
        }

        // Switch back to the player's main camera
        SwitchToPlayerCam();

        // Re-enable player movement
        EnablePlayerMovement();

        // Start cooldown and trigger the agonize animation
        StartChokeCooldown();

        // Resume monster behavior (resume NavMeshAgent movement)
        agent.isStopped = false;

        // Reset the choke lift animation trigger
        animator.ResetTrigger("ChokeLift");
    }

    // Temporarily disable player movement when picked up
    void DisablePlayerMovement()
    {
        SC_FPSController fpsController = player.GetComponent<SC_FPSController>();
        if (fpsController != null)
        {
            fpsController.canMove = false; // Disable player movement
        }

        if (playerController != null)
        {
            playerController.enabled = false; // Freeze player movement by disabling CharacterController
        }
    }

    void EnablePlayerMovement()
    {
        SC_FPSController fpsController = player.GetComponent<SC_FPSController>();
        if (fpsController != null)
        {
            fpsController.canMove = true; // Re-enable player movement
        }

        // If the player uses a CharacterController or Rigidbody, make sure it's enabled again
        CharacterController playerController = player.GetComponent<CharacterController>();
        if (playerController != null)
        {
            playerController.enabled = true; // Re-enable CharacterController to restore movement
        }
    }

    // Switch to the choke lift camera
    void SwitchToChokeLiftCam()
    {
        Debug.Log("Switching to chokecamera");
        chokeLiftCam.Priority = 20; // Higher priority activates the choke lift camera
        playerCam.Priority = 0;     // Lower priority disables the player's main camera
    }

    // Switch back to the player's main camera
    void SwitchToPlayerCam()
    {
        // Ensure player camera is active and the choke camera is deactivated
        playerCam.Priority = 20;  // Ensure player camera is active
        chokeLiftCam.Priority = 0; // Disable choke lift camera
    }

    void StartAgonizing()
    {
        isAgonizing = true;
        hasAgonized = true; // Ensure it only happens once per encounter
        agonizingTimer = 0f; // Reset the agonizing timer

        // Notify the PlayerAgonizeEffect script to apply agonizing effects
        if (playerAgonizeEffect != null)
        {
            playerAgonizeEffect.StartAgonizing(transform, agonizingDamageRange);
        }

        // Stop the monster from moving
        agent.isStopped = true;

        // Trigger the Agonizing animation
        animator.SetTrigger("Agonize");

        // Play the agonizing sound
        if (screamAudioSource != null && screamAudioSource.clip != null)
        {
            screamAudioSource.Play();
        }
    }

    void EndAgonizing()
    {
        isAgonizing = false;
        // Stop the monster from moving
        agent.isStopped = false;

        if (playerAgonizeEffect != null)
        {
            playerAgonizeEffect.StopAgonizing();
        }

        // Check if the player is still in the chase range or attack range
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= chasingDistance)
        {
            isChasing = true; // Resume chasing
            agent.speed = chaseSpeed; // Set to chase speed
        }
        else
        {
            // Resume patrolling if the player is out of range
            agent.isStopped = false;
            GotoNextWaypoint();
        }

        // Reset animation to walking
        animator.SetBool("isChasing", isChasing);

        // Reset the Agonize animation
        animator.ResetTrigger("Agonize");

        // Stop the agonizing sound
        if (screamAudioSource != null)
        {
            screamAudioSource.Stop();
        }
    }

    void StartChokeCooldown()
    {
        chokeCooldownActive = true;

        // Reset animation to walking
        animator.SetBool("isChasing", false);

        // Stop the monster from moving
        agent.isStopped = true;

        // Trigger the Agonizing animation
        animator.SetTrigger("Agonize");

        // Set a cooldown period (e.g., 5 seconds)
        Invoke("EndChokeCooldown", chokeCooldownDuration);
    }

    void EndChokeCooldown()
    {
        chokeCooldownActive = false;
        isAgonizing = false;

        // Reset the Agonize animation trigger
        animator.ResetTrigger("Agonize");
        animator.SetBool("isChasing", false);

        // Resume monster behavior by re-enabling the NavMeshAgent
        agent.isStopped = false;

        // Determine whether to patrol or chase the player after the cooldown
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > detectionRange) // Player is far enough
        {
            // Return to patrolling
            agent.speed = patrolSpeed;
            GotoNextWaypoint();
        }
        else if (HasLineOfSight()) // Player is still within range and visible
        {
            // Resume chasing the player
            isChasing = true;
            agent.speed = chaseSpeed;
            ChasePlayer();
        }
    }

    // Check if there is line of sight to the player
    bool HasLineOfSight()
    {
        RaycastHit hit;
        Vector3 directionToPlayer = (player.position - transform.position).normalized;

        // Cast a ray towards the player and check if it hits anything in the obstacleMask
        if (Physics.Raycast(transform.position, directionToPlayer, out hit, detectionRange, obstacleMask))
        {
            // If the ray hits something that is not the player, we have no line of sight
            if (hit.transform != player)
            {
                return false; // Obstacle is blocking the line of sight
            }
        }

        return true; // Player is visible
    }

    void AvoidWallHugging()
    {
        RaycastHit hit;
        // Cast a ray forward from the monster's position to check for nearby walls
        if (Physics.Raycast(transform.position, transform.forward, out hit, avoidWallDistance, obstacleMask))
        {
            // If a wall is detected, rotate the monster slightly away from the wall
            Vector3 avoidDirection = Vector3.Cross(hit.normal, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(avoidDirection), Time.deltaTime * 5f);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Visualize the sphere cast range for avoiding walls
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, avoidWallDistance);
    }
}
