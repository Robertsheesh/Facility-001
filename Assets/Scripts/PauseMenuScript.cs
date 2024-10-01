using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement; // To load different scenes like Main Menu
using UnityEngine.Audio; // Import AudioMixer
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI; // For button functionality

public class PauseMenuScript : MonoBehaviour
{
    public GameObject pauseMenu; // Reference to the Pause Menu UI
    public AudioSource menuOpenSound; // Sound effect for opening the menu
    public AudioSource menuCloseSound; // Sound effect for closing the menu
    public PostProcessVolume postProcessVolume; // Post-processing volume for blurring
    private DepthOfField blurEffect; // Blur effect for background
    public AudioMixer audioMixer; // Reference to the Audio Mixer
    public GameObject inGameUI; // Reference to the in-game UI

    public SC_FPSController playerController; // Reference to the player's FPS controller script for disabling movement

    public Button resumeButton; // Reference to the Resume button
    public Button settingsButton; // Reference to the Settings button
    public Button mainMenuButton; // Reference to the Back to Main Menu button
    public Button quitButton; // Reference to the Quit button

    private bool isPaused;

    void Start()
    {
        pauseMenu.SetActive(false);
        isPaused = false;

        // Initialize blur effect if using post-processing
        if (postProcessVolume != null)
        {
            postProcessVolume.profile.TryGetSettings(out blurEffect);
            blurEffect.active = false; // Disable blur at the start
        }

        // Ensure playerController is assigned
        if (playerController == null)
        {
            playerController = FindObjectOfType<SC_FPSController>(); // Optionally find the player controller if not assigned
        }

        // Assign button click events
        resumeButton.onClick.AddListener(ResumeGame);
        settingsButton.onClick.AddListener(OpenSettings);
        mainMenuButton.onClick.AddListener(BackToMainMenu);
        quitButton.onClick.AddListener(QuitGame);
    }

    void Update()
    {
        // Toggle Pause Menu on Escape key
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        // Play sound effect for opening the menu
        if (menuOpenSound != null)
        {
            menuOpenSound.Play();
        }

        pauseMenu.SetActive(true);
        Time.timeScale = 0f; // Stop the game time
        isPaused = true;

        // Disable player movement and looking around
        if (playerController != null)
        {
            playerController.canMove = false; // Disable movement in the player controller
        }

        // Apply blur effect to the background
        if (blurEffect != null)
        {
            blurEffect.active = true;
        }

        // Apply the muffled effect using the low-pass filter
        if (audioMixer != null)
        {
            audioMixer.SetFloat("lowpass", 500f); // Lower cutoff frequency to muffle sound
        }

        // Disable the in-game UI
        if (inGameUI != null)
        {
            inGameUI.SetActive(false);
        }

        // Optionally hide the cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        // Play sound effect for closing the menu
        if (menuCloseSound != null)
        {
            menuCloseSound.Play();
        }

        // Immediately resume the game (no more animation delay)
        StartCoroutine(ResumeGameRoutine());

        // Disable blur effect
        if (blurEffect != null)
        {
            blurEffect.active = false;
        }

        // Remove the muffled effect by resetting the low-pass filter
        if (audioMixer != null)
        {
            audioMixer.SetFloat("lowpass", 22000f); // Reset cutoff frequency to normal
        }

        // Enable the in-game UI again
        if (inGameUI != null)
        {
            inGameUI.SetActive(true);
        }

        // Optionally lock the cursor again
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private IEnumerator ResumeGameRoutine()
    {
        // Wait for a moment before resuming the game (optional)
        yield return new WaitForSecondsRealtime(0f); // Small delay if needed

        pauseMenu.SetActive(false);
        Time.timeScale = 1f; // Resume the game time
        isPaused = false;

        // Enable player movement and looking around
        if (playerController != null)
        {
            playerController.canMove = true; // Enable movement in the player controller
        }
    }

    public void OpenSettings()
    {
        // Placeholder for settings menu logic
        Debug.Log("Settings button clicked! You can open a settings panel here.");
        // You can add functionality to open a separate settings menu panel
    }

    public void BackToMainMenu()
    {
        // Reset time scale and load the main menu
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu"); // Ensure you have a scene called "MainMenu" or update to the correct scene name
    }

    public void QuitGame()
    {
        // Quit the game
        Debug.Log("Quitting the game...");
        Application.Quit();
    }
}
