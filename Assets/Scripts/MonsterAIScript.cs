using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterAI : MonoBehaviour
{
    public NavMeshAgent agent;
    public float patrolRange; // Radius for random patrol
    public Transform centrePoint; // Center of patrol area

    public Transform player; // Reference to the player
    public float detectionRange = 10f; // Range within which the AI detects the player
    public float chasingDistance = 15f; // Max distance for chasing the player
    public float chaseSpeed = 6f; // Speed while chasing the player
    public float patrolSpeed = 3.5f; // Speed during patrolling
    public float avoidWallDistance = 2f; // Minimum distance from walls while patrolling

    private bool isChasing = false; // Whether the AI is currently chasing the player
    private bool isAgonizing = false; // Whether the monster is currently agonizing
    private float agonizingTimer = 0f; // Timer to control the agonizing interval
    private float agonizingDuration = 10f; // Maximum duration for agonizing

    public LayerMask obstacleMask; // Layer mask for objects that block the line of sight (e.g., walls)

    private Animator animator; // Reference to the Animator component
    private float nextAgonizingTime = 30f; // Time interval for agonizing animation

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = patrolSpeed; // Start patrolling with patrol speed

        animator = GetComponent<Animator>(); // Get the Animator component
        if (centrePoint == null)
        {
            centrePoint = transform; // Default patrol area is around the AI itself
        }
    }

    void Update()
    {
        if (isAgonizing)
        {
            // If agonizing, count the time and return to patrolling when finished
            agonizingTimer += Time.deltaTime;
            if (agonizingTimer >= agonizingDuration)
            {
                EndAgonizing();
            }
            return; // Skip normal logic while agonizing
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

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

        animator.ResetTrigger("Agonize");

        // Resume patrol or whatever the AI was doing before
        Patrol();
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
