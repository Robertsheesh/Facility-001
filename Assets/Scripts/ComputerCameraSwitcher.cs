using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class ComputerCameraSwitcher : MonoBehaviour
{
    public GameObject playerUI; // Player's UI (disabled during computer interaction)
    public GameObject computerUI; // Computer's terminal UI (enabled after boot sequence)
    public GameObject interactionText; // "Press E" text

    // Boot video components
    public VideoPlayer bootVideoPlayer;  // Boot sequence video player
    public RawImage bootVideoDisplay;    // Display for the boot video
    public AudioSource bootSound;        // Boot sound effect
    public AudioSource runningSound;     // Sound after boot sequence is complete

    private bool hasBooted = false;
    private bool isPlayerNearby = false;
    private bool isUsingComputer = false;

    void Start()
    {
        // Ensure the computer UI is hidden at the start
        computerUI.SetActive(false);
        bootVideoDisplay.gameObject.SetActive(false);
        runningSound.loop = true;
        runningSound.Stop();

        if (interactionText != null)
        {
            interactionText.SetActive(false);
        }
    }

    void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))  // E key to interact
        {
            if (isUsingComputer)
            {
                ExitComputerInteraction();
            }
            else
            {
                StartComputerInteraction();
            }
        }
    }

    // Start interaction with the computer
    void StartComputerInteraction()
    {
        isUsingComputer = true;

        if (!hasBooted)
        {
            StartCoroutine(DelayedBootSequence());
        }
        else
        {
            ActivateRunningMode();
        }

        if (interactionText != null)
        {
            interactionText.SetActive(false);
        }
    }

    // Exit interaction with the computer
    void ExitComputerInteraction()
    {
        isUsingComputer = false;
        playerUI.SetActive(true);
        computerUI.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    IEnumerator DelayedBootSequence()
    {
        yield return new WaitForSeconds(1f);  // Wait for 1 second before boot video starts
        bootSound.Play();  // Play boot sound
        bootVideoDisplay.gameObject.SetActive(true);  // Show the boot video display
        bootVideoPlayer.Play();  // Start the boot video

        Debug.Log("Boot video is playing.");

        hasBooted = true;

        yield return new WaitForSeconds((float)bootVideoPlayer.clip.length);  // Wait for boot video to finish

        // TEMPORARILY REMOVE THIS TO SEE IF IT'S HIDDEN TOO EARLY
        // bootVideoDisplay.gameObject.SetActive(false);  // Hide boot video display

        StartCoroutine(PlayRunningSoundAfterBoot());
        ActivateRunningMode();  // Activate the computer UI
    }


    void ActivateRunningMode()
    {
        computerUI.SetActive(true);
        playerUI.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Play running sound after boot sound finishes
    IEnumerator PlayRunningSoundAfterBoot()
    {
        yield return new WaitForSeconds(bootSound.clip.length);
        runningSound.Play();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;

            if (interactionText != null && !isUsingComputer)
            {
                interactionText.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;

            if (interactionText != null)
            {
                interactionText.SetActive(false);
            }
        }
    }
}
