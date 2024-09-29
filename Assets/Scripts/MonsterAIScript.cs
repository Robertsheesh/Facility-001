using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Cinemachine; // Import Cinemachine for virtual camera control

public class MonsterAI : MonoBehaviour
{
    public NavMeshAgent agent;
    public float patrolRange; // Radius for random patrol
    public Transform centrePoint; // Center of patrol area

    public Transform player; // Reference to the player
    public Transform monsterHand; // Reference to the monster's hand bone
    public float detectionRange = 10f; // Range within which the AI detects the player
    public float chasingDistance = 15f; // Max distance for chasing the player
    public float attackDistance = 2f; // Distance within which the monster will perform the choke lift attack
    public float chaseSpeed = 6f; // Speed while chasing the player
    public float patrolSpeed = 3.5f; // Speed during patrolling
    public float avoidWallDistance = 2f; // Minimum distance from walls while patrolling

    public CinemachineVirtualCamera playerCam; // Reference to player's main Cinemachine camera
    public CinemachineVirtualCamera chokeLiftCam; // Reference to the camera attached to the monster's hand for choke lift

    private bool isChasing = false; // Whether the AI is currently chasing the player
    private bool isAgonizing = false; // Whether the monster is currently agonizing
    private bool isAttacking = false; // Whether the monster is currently performing the attack
    private float agonizingTimer = 0f; // Timer to track how long the monster has been agonizing
    public float agonizingDuration = 10f; // Maximum duration for agonizing

    public LayerMask obstacleMask; // Layer mask for objects that block the line of sight (e.g., walls)

    private Animator animator; // Reference to the Animator component
    private float nextAgonizingTime = 30f; // Time interval for agonizing animation

    private bool chokeCooldownActive = false; // Tracks if the choke lift is on cooldown
    public float chokeCooldownDuration = 10f; // The duration of the cooldown in seconds
    public float safeDistance = 3f; // Safe distance the player needs to be away from the monster after being thrown

    private CharacterController playerController; // Reference to player's CharacterController

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = patrolSpeed; // Start patrolling with patrol speed

        animator = GetComponent<Animator>(); // Get the Animator component

        // Reference the player's CharacterController
        playerController = player.GetComponent<CharacterController>();

        if (centrePoint == null)
        {
            centrePoint = transform; // Default patrol area is around the AI itself
        }

        if (monsterHand == null)
        {
            Debug.LogError("Monster's hand bone is not assigned.");
        }

        if (playerCam == null || chokeLiftCam == null)
        {
            Debug.LogError("Cinemachine cameras are not assigned.");
        }

        // Ensure the player's main camera is active at the start
        playerCam.Priority = 10; // Higher priority means it's active
        chokeLiftCam.Priority = 0; // Ensure this is initially inactive
    }

    void Update()
    {
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

        // Handle close-range attack (Superhuman Choke Lift)
        if (distanceToPlayer <= attackDistance && !chokeCooldownActive)
        {
            StartChokeLift();
            return; // Skip other logic while the attack is performed
        }

        // Check if player is within detection range and there is line of sight
        if (distanceToPlayer <= detectionRange && HasLineOfSight())
        {
            isChasing = true; // Start chasing the player
            agent.speed = chaseSpeed; // Increase speed when chasing
        }
        else if (isChasing && distanceToPlayer > chasingDistance)
        {
            isChasing = false; // Stop chasing if player is too far
            agent.speed = patrolSpeed; // Return to normal patrol speed
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
            // Randomly patrol around the map and potentially agonize
            Patrol();

            // Handle agonizing only while patrolling
            nextAgonizingTime -= Time.deltaTime;
            if (nextAgonizingTime <= 0f)
            {
                StartAgonizing();
                nextAgonizingTime = 30f; // Reset for the next agonizing session
            }
        }
    }

    void Patrol()
    {
        if (agent.remainingDistance <= agent.stoppingDistance) // Reached patrol destination
        {
            Vector3 point;
            if (FindValidPatrolPoint(out point)) // Get a valid point that avoids walls
            {
                Debug.DrawRay(point, Vector3.up, Color.blue, 1.0f); // Visualize patrol points
                agent.SetDestination(point); // Move to the random point
            }
        }
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

        // Reposition the player a small distance away from the monster to simulate the throw
        Vector3 throwDirection = (player.position - transform.position).normalized;
        float throwDistance = 2f;
        player.position = transform.position + throwDirection * throwDistance;

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
        // Re-enable the player's main camera
        playerCam.Priority = 20;
        chokeLiftCam.Priority = 0;

        // Reset the player's camera rotation to its upright position
        playerCam.transform.localRotation = Quaternion.identity;

        // Ensure that the player's camera position matches the player's head position
        playerCam.transform.position = player.transform.position + new Vector3(0, 1.6f, 0); // Adjust the offset as necessary to align with the player's head
    }


    void StartAgonizing()
    {
        isAgonizing = true;
        agonizingTimer = 0f; // Reset the agonizing timer

        // Stop the monster from moving
        agent.isStopped = true;

        // Trigger the Agonizing animation
        animator.SetTrigger("Agonize");
    }

    void EndAgonizing()
    {
        isAgonizing = false;

        // Resume patrolling
        agent.isStopped = false;

        // Reset animation to walking
        animator.SetBool("isChasing", false);

        // Reset the Agonize animation
        animator.ResetTrigger("Agonize");

        // Resume patrol or whatever the AI was doing before
        Patrol();
    }

    void StartChokeCooldown()
    {
        chokeCooldownActive = true;
        isAgonizing = true;
        animator.SetBool("isChasing", false);
        agonizingTimer = 0f;

        // Stop the NavMeshAgent so the monster doesn't move during the agonizing animation
        agent.isStopped = true;

        // Trigger the agonize animation
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
            Patrol();
        }
        else if (HasLineOfSight()) // Player is still within range and visible
        {
            // Resume chasing the player
            isChasing = true;
            agent.speed = chaseSpeed;
            ChasePlayer();
        }
    }


    // Find a valid patrol point while avoiding walls
    bool FindValidPatrolPoint(out Vector3 result)
    {
        for (int i = 0; i < 30; i++) // Try up to 30 times to find a valid point
        {
            if (RandomPoint(centrePoint.position, patrolRange, out result))
            {
                // Check if the point is too close to a wall
                if (!IsNearWall(result))
                {
                    return true; // Point is valid
                }
            }
        }

        result = Vector3.zero;
        return false; // No valid point found
    }

    // Check if the patrol point is near a wall
    bool IsNearWall(Vector3 point)
    {
        // Perform a sphere cast to check for obstacles (like walls) within avoidWallDistance
        Collider[] hitColliders = Physics.OverlapSphere(point, avoidWallDistance, obstacleMask);
        if (hitColliders.Length > 0)
        {
            // If the sphere cast hits anything, the point is too close to a wall
            return true;
        }
        return false;
    }

    bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        Vector3 randomPoint = center + Random.insideUnitSphere * range; // Random point within the patrol range
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
        {
            result = hit.position;
            return true;
        }
        result = Vector3.zero;
        return false;
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

    void OnDrawGizmosSelected()
    {
        // Visualize the sphere cast range for avoiding walls
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, avoidWallDistance);
    }
}
