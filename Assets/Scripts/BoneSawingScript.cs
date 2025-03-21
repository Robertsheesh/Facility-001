using UnityEngine;
using System.Collections;

public class BoneSawScript : MonoBehaviour
{
    public Transform targetSawingPoint;  // Assign in Inspector
    public Transform playerHandPoint;    // Assign in Inspector
    public AudioSource boneSawAudioSource;
    public float sawSpeed = 5f;
    public float sawingDistance = 0.2f;
    public bool isSawing = false;
    public bool isAtTarget = false;

    public GameObject handObject; // Assign in Inspector (the object being sawed)
    public float requiredSawTime = 5f; // 5 seconds required to sever the hand
    private float sawElapsedTime = 0f;
    private bool isHandSevered = false; // Prevent multiple triggers

    public SC_FPSController sC_FPSController;

    private Coroutine sawingCoroutine;

    private void Start()
    {
        Debug.Log("BoneSawScript is active!");
    }

    private void Update()
    {
        if (isAtTarget && Input.GetMouseButtonDown(0) && !isSawing)
        {
            Debug.Log("Left Click Detected: Starting Sawing!");
            StartSawing();
        }
        else if (isSawing && Input.GetMouseButtonUp(0))
        {
            Debug.Log("Left Click Released: Stopping Sawing!");
            StopSawing();
        }
        // Toggle: Exit sawing mode on E if already at target
        if (isAtTarget && Input.GetKeyDown(KeyCode.E) && !isSawing)
        {
            Debug.Log("E pressed while at target — exiting sawing mode.");
            ReturnToPlayer();
        }
    }

    public void MoveToSawingPoint()
    {
        Debug.Log("MoveToSawingPoint() called!");
        StartCoroutine(MoveSaw(targetSawingPoint.position, () =>
        {
            isAtTarget = true;
            sC_FPSController.EnterSawingMode();
            transform.position = targetSawingPoint.position;
            transform.rotation = targetSawingPoint.rotation;
            transform.SetParent(targetSawingPoint); // Lock to target
            Debug.Log($"Saw reached target! isAtTarget = {isAtTarget}, Parent = {transform.parent.name}");
        }));
    }

    public void ReturnToPlayer()
    {
        Debug.Log("Returning saw to player");

        StopSawing();

        StartCoroutine(MoveSaw(playerHandPoint.position, () =>
        {
            isAtTarget = false;
            sC_FPSController.ExitSawingMode();
            transform.SetParent(playerHandPoint); // Attach to player
            Debug.Log("Saw returned to player");
        }));
    }

    private IEnumerator MoveSaw(Vector3 targetPosition, System.Action onComplete)
    {
        float elapsedTime = 0f;
        Vector3 startPosition = transform.position;

        while (elapsedTime < 1f)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime);
            elapsedTime += Time.deltaTime * sawSpeed;
            yield return null;
        }

        transform.position = targetPosition;
        onComplete?.Invoke();
    }

    private void StartSawing()
    {
        if (sawingCoroutine == null)
        {
            isSawing = true;
            sawElapsedTime = 0f; // Reset timer when sawing starts
            sawingCoroutine = StartCoroutine(SawingMotion());
            Debug.Log("Actually sawing");
            if (boneSawAudioSource != null)
            {
                boneSawAudioSource.loop = true;
                boneSawAudioSource.Play();
            }
        }
    }

    private void StopSawing()
    {
        if (sawingCoroutine != null)
        {
            StopCoroutine(sawingCoroutine);
            sawingCoroutine = null;
        }
        isSawing = false;
        if (boneSawAudioSource != null)
        {
            boneSawAudioSource.loop = false;
            boneSawAudioSource.Stop();
        }
    }

    private IEnumerator SawingMotion()
    {
        Debug.Log("Sawing motion started");

        while (isSawing)
        {
            float t = 0f;
            Vector3 forwardPos = Vector3.forward * sawingDistance;
            Vector3 backwardPos = -Vector3.forward * sawingDistance;

            // Move forward
            while (t < 1f && isSawing)
            {
                transform.localPosition = Vector3.Lerp(Vector3.zero, forwardPos, t);
                t += Time.deltaTime * sawSpeed;
                sawElapsedTime += Time.deltaTime;
                yield return null;
            }

            // Move backward
            t = 0f;
            while (t < 1f && isSawing)
            {
                transform.localPosition = Vector3.Lerp(forwardPos, backwardPos, t);
                t += Time.deltaTime * sawSpeed;
                sawElapsedTime += Time.deltaTime;
                yield return null;
            }

            // Return to center
            transform.localPosition = Vector3.zero;

            // Check if sawing has reached the required time
            if (sawElapsedTime >= requiredSawTime && !isHandSevered)
            {
                SeverHand();
            }
        }

        Debug.Log("Sawing motion stopped");
    }

    private void SeverHand()
    {
        if (handObject != null)
        {
            isHandSevered = true;

            // Update tag and physics
            handObject.tag = "SeveredHand";
            Rigidbody rb = handObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.AddForce(transform.forward * 5f, ForceMode.Impulse);
            }

            Debug.Log("Hand severed and pushed forward!");

            StopSawing(); // Stop sawing motion

            // Exit sawing mode and return the saw to player
            sC_FPSController.ExitSawingMode();
            ReturnToPlayer();

            // Disable this script after returning to player
            StartCoroutine(DisableSawScriptAfterDelay(0.1f)); // Give time for ReturnToPlayer to complete
        }
    }

    private IEnumerator DisableSawScriptAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        this.enabled = false;
        targetSawingPoint.GetComponent<BoxCollider>().enabled = false;
        Debug.Log("BoneSawScript disabled after sawing complete.");
    }

}
