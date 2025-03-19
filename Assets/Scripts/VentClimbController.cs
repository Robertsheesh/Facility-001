using UnityEngine;
using Cinemachine;
using System.Collections;
using UnityEngine.EventSystems;

public class VentClimb : MonoBehaviour
{
    public Transform ventEntryPoint;  // Place this at the vent entrance
    public Transform ventTargetPoint; // Place this inside the vent
    public CinemachineVirtualCamera ventCamera;
    public CinemachineVirtualCamera playerCamera;
    public CharacterController playerController; // Reference to the player's CharacterController
    public float moveSpeed = 3f; // Speed at which the player moves into the vent
    public float transitionTime = 1f; // Duration for the movement
    public VentTrigger ventTrigger; // Reference to the VentTrigger script
    public ObjectPicker objectPicker;

    private bool isClimbing = false;
    private bool isPlayerInTrigger = false;
    public AudioSource ventClimbSound;

    void Update()
    {
        if (isPlayerInTrigger && ventTrigger.isOpen && Input.GetKeyDown(KeyCode.Space) && !isClimbing)
        {
            if (objectPicker != null)
            {
                objectPicker.UnequipCurrentItem();
            }
            StartCoroutine(EnterVent());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = false;
        }
    }

    IEnumerator EnterVent()
    {
        isClimbing = true;

        ventClimbSound.Play();

        isPlayerInTrigger = false; // ✅ Prevent re-triggering

        // Disable player movement & gravity
        playerController.enabled = false;

        // Switch to vent camera
        ventCamera.Priority = 10;
        playerCamera.Priority = 0;

        // Move player to entry point first
        Transform playerTransform = playerController.transform;
        playerTransform.position = ventEntryPoint.position;
        yield return new WaitForSeconds(0.5f); // Small delay for effect

        // Smoothly move player inside the vent
        float elapsedTime = 0;
        Vector3 startPos = ventEntryPoint.position;
        Vector3 endPos = ventTargetPoint.position;

        while (elapsedTime < transitionTime)
        {
            playerTransform.position = Vector3.Lerp(startPos, endPos, elapsedTime / transitionTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure player reaches final position
        playerTransform.position = ventTargetPoint.position;

        // Switch back to player camera
        ventCamera.Priority = 0;
        playerCamera.Priority = 10;


        // Re-enable player movement
        playerController.enabled = true;
        isClimbing = false;
    }
}
