using UnityEngine;
using System.Collections;

public class SlidingDoorController : MonoBehaviour
{
    public Animator doorAnimator;
    public GameObject canisterPrefab;
    public Transform spawnPoint;
    public float openDelay = 1f;
    private GameObject currentCanister;

    void Start()
    {
        CloseDoor(); // Start with door closed
        SpawnCanister(); // Spawn the first canister
    }

    public void OpenDoor()
    {
        doorAnimator.SetBool("IsOpen", true);
    }

    public void CloseDoor()
    {
        doorAnimator.SetBool("IsOpen", false);
    }

    public void CanisterPickedUp()
    {
        StartCoroutine(CloseAndSpawnNewCanister());
    }

    IEnumerator CloseAndSpawnNewCanister()
    {
        CloseDoor();
        yield return new WaitForSeconds(openDelay);
        SpawnCanister(); // Spawn a new canister
        OpenDoor();
    }

    void SpawnCanister()
    {
        if (currentCanister != null)
        {
            Destroy(currentCanister); // Remove old canister
        }
        currentCanister = Instantiate(canisterPrefab, spawnPoint.position, spawnPoint.rotation); // Spawn new canister
    }
}
