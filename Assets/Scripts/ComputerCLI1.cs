using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ComputerCLI1 : MonoBehaviour
{
    public Text terminalOutput;      // The text element to display terminal output
    public InputField inputField;    // The input field for user input
    public float inputDelay = 1f;    // Delay time before input is enabled (in seconds)

    public AudioSource keycardProcessing;
    public AudioSource ErrorSoundEffect;
    public AudioSource SuccessSoundEffect;

    // Security cameras and display
    public GameObject controlRoomCamera;   // Control Room entrance camera in hierarchy
    public GameObject machineCamera;       // Machine camera in hierarchy
    public GameObject storageCamera;       // Storage camera in hierarchy
    public RawImage securityDisplay;       // Display for security camera render textures

    public RenderTexture controlRoomTexture;  // Render texture for Control Room entrance camera
    public RenderTexture machineTexture;      // Render texture for Machine camera
    public RenderTexture storageTexture;      // Render texture for Storage camera

    private string welcomeText = "Aetheris Dynamics 2.21\nLogin:\n"; // Welcome text
    private bool canType = false;    // Whether the player can start typing

    private bool isLoggedIn = false;       // Track if the user is logged in
    private bool isEnteringPassword = false;  // Track if we are waiting for the password
    private bool isEnteringSecurityCode = false; // Track if the user is entering the security code
    private string username = "";          // To store the entered username

    private const string correctUsername = "a";  // The correct username
    private const string correctPassword = "a";  // The correct password

    private bool isConfirmingRewrite = false;  // Track if the player is confirming the rewrite action

    private ComputerLogs computerLogs;  // Reference to the ComputerLogs script

    private CardInsertScript cardInsertScript;

    // Platform puzzle variables
    private float[] platformPositions = new float[4]; // Stores current positions of platforms
    private int currentPlatform = 0; // Tracks which platform the player is aligning
    private bool isPuzzleActive = false;
    private float[] platformSpeeds; // Array to store progressive platform swing speeds
    public GameObject[] platforms;
    private Vector3[] originalPositions; // Array to store original positions of platforms
    private Quaternion[] initialRotations; // Array to store the initial rotations of platform parents
    private bool isAligning = false; // Flag to track if a platform is aligning to X-axis = 0

    private enum ComputerState { Login, HelpMenu, LogsMenu, SecurityMainMenu, CameraMenu, AccessControlMenu, ViewingLog, PlatformPuzzle }
    private ComputerState currentState = ComputerState.Login;
    private ComputerState previousState;  // Track the previous state for "back" functionality

    private bool isViewingLog = false; // Track if the user is viewing a log

    private bool inSecurityMenu = false; // Track if the user is in the security camera menu
    private int currentCameraIndex = -1; // Track the currently displayed camera (-1 means none)

    void Start()
    {
        // Find the ComputerLogs script
        computerLogs = FindObjectOfType<ComputerLogs>();

        // Set initial terminal output
        terminalOutput.text = welcomeText;

        // Disable all security cameras initially
        DeactivateAllCameras();

        // Disable the input field at the start (since player is not interacting initially)
        DisableInput();

        cardInsertScript = FindObjectOfType<CardInsertScript>();
        if (cardInsertScript == null)
        {
            Debug.LogError("CardInsertScript not found!");
        }
    }

    private void Awake()
    {
        if (initialRotations == null || initialRotations.Length != platforms.Length)
        {
            initialRotations = new Quaternion[platforms.Length];
        }

        for (int i = 0; i < platforms.Length; i++)
        {
            // Randomize the initial X-axis rotation (e.g., between -45 and 45 degrees)
            float randomAngle = Random.Range(-45f, 45f);
            platforms[i].transform.parent.localRotation = Quaternion.Euler(randomAngle, 0f, 0f);
            initialRotations[i] = platforms[i].transform.parent.localRotation; // Store randomized position
        }
    }



    // Enable input field with delay when player uses the computer
    public void EnableInput()
    {
        StartCoroutine(EnableInputFieldAfterDelay());
    }

    // Disable input field when player leaves the computer
    public void DisableInput()
    {
        canType = false;
        inputField.interactable = false;
        inputField.DeactivateInputField();
        inputField.text = ""; // Clear input when leaving the computer
    }

    // Coroutine to enable input field after a delay
    private IEnumerator EnableInputFieldAfterDelay()
    {
        yield return new WaitForSeconds(inputDelay);  // Wait for the specified delay

        canType = true;
        inputField.interactable = true;
        inputField.ActivateInputField();
        inputField.Select();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && canType)
        {
            SubmitInput(inputField.text);  // Manually submit input when pressing Enter
        }

        if (currentState == ComputerState.PlatformPuzzle && Input.GetKeyDown(KeyCode.Space))
        {
            if (!isAligning && Mathf.Abs(platformPositions[currentPlatform] - 0.5f) < 0.1f)
            {
                AlignPlatform(); // Player succeeded
            }
            else if (!isAligning)
            {
                // Player failed, restart from the first platform
                terminalOutput.text = "You failed! Restarting the puzzle...\n";
                ErrorSound();
                RestartPuzzle();
            }
        }
    }


    private void SubmitInput(string input)
    {
        if (!string.IsNullOrWhiteSpace(input) && canType)
        {
            terminalOutput.text = "";

            if (!isLoggedIn)
            {
                HandleLogin(input);
            }
            else if (isEnteringSecurityCode)
            {
                HandleSecurityCode(input);
            }
            else
            {
                // Use HandleCommands or specific security handlers based on current state
                switch (currentState)
                {
                    case ComputerState.SecurityMainMenu:
                        HandleSecurityCommands(input);  // Security options (cameras or access control)
                        break;
                    case ComputerState.CameraMenu:
                        HandleCameraCommands(input);  // Camera-specific commands
                        break;
                    case ComputerState.AccessControlMenu:
                        HandleAccessControlCommands(input);  // Access control commands
                        break;
                    default:
                        HandleCommands(input);  // General command handling (help, logs, etc.)
                        break;
                }
            }

            inputField.text = ""; // Clear the input field after each input
            inputField.ActivateInputField();
            inputField.Select();
        }
    }

    private void HandleLogin(string input)
    {
        if (!isEnteringPassword)
        {
            username = input.ToLower();
            if (username == correctUsername)
            {
                terminalOutput.text += "Password:\n";
                isEnteringPassword = true;
            }
            else
            {
                terminalOutput.text += "Login incorrect\nLogin:\n";
                ErrorSound();
            }
        }
        else
        {
            if (input == correctPassword)
            {
                isLoggedIn = true;
                terminalOutput.text = "Last login: Mon Sep 17 23:22:09 UTC 2102\n\nType help to see all available commands.\n";
                SuccessSound();
            }
            else
            {
                terminalOutput.text = "Login incorrect\nLogin:\n";
                isEnteringPassword = false;
                ErrorSound();
            }
        }
    }

    private void HandleCommands(string input)
    {
        if (input.ToLower() == "back")
        {
            // Handle going back from various states
            if (currentState == ComputerState.ViewingLog)
            {
                // Go back to the log list (logs menu)
                DisplayLogMenu();
                isViewingLog = false;
                SuccessSound();
            }
            else if (currentState == ComputerState.LogsMenu)
            {
                // Go back to the help menu from the logs menu
                terminalOutput.text = "Returning to the main menu.\nType 'help' to see all available commands.\n";
                currentState = ComputerState.HelpMenu;
                SuccessSound();
            }
            else if (currentState == ComputerState.CameraMenu || currentState == ComputerState.AccessControlMenu)
            {
                // Go back to the security main menu from either the camera or access control menu
                EnterSecurityMainMenu();
                SuccessSound();
            }
            else if (currentState == ComputerState.SecurityMainMenu)
            {
                // Go back to the help menu from the security main menu
                terminalOutput.text = "Exiting security mode.\nType 'help' to see available commands.\n";
                currentState = ComputerState.HelpMenu;
                DeactivateAllCameras();  // Ensure cameras are turned off when exiting security
                SuccessSound();
            }
            else if (currentState == ComputerState.PlatformPuzzle)
            {
                isPuzzleActive = false;
                terminalOutput.text = "Exiting the Alignment Puzzle.\nType 'help' to see available commands.\n";
                currentState = ComputerState.HelpMenu;
            }
        }
        else if (input.ToLower() == "align")
        {
            EnterPlatformPuzzle();
        }
        else if (input.ToLower() == "help")
        {
            terminalOutput.text = "Available commands:\n - help\n - logout\n - logs\n - security\n - align\n";
            currentState = ComputerState.HelpMenu;
        }
        else if (input.ToLower() == "logout")
        {
            isLoggedIn = false;
            isEnteringPassword = false;
            terminalOutput.text = "You have been logged out.\nLogin:\n";
            currentState = ComputerState.Login;
            DeactivateAllCameras();
            SuccessSound();
        }
        else if (input.ToLower() == "logs")
        {
            if (computerLogs.HasAccessToLogs())
            {
                // Display the list of logs and enter the LogsMenu state
                DisplayLogMenu();
                currentState = ComputerState.LogsMenu;
                SuccessSound();
            }
            else
            {
                terminalOutput.text = "Please enter the security code to access logs:\n";
                isEnteringSecurityCode = true;
                ErrorSound();
            }
        }
        else if (int.TryParse(input, out int logIndex) && computerLogs.HasAccessToLogs())
        {
            string log = computerLogs.GetLog(logIndex - 1); // Subtract 1 because logs are 0-indexed internally
            if (log != null)
            {
                terminalOutput.text = "Type 'back' to exit log.\n\n";
                terminalOutput.text += log; // Display the selected log content
                isViewingLog = true;
                previousState = currentState;  // Save the previous state (LogsMenu)
                currentState = ComputerState.ViewingLog;
            }
            else
            {
                terminalOutput.text = "Log not found.\nType 'help' to see available commands.\n";
                ErrorSound();
            }
        }
        else if (input.ToLower() == "security")
        {
            // Correctly route to the security main menu
            EnterSecurityMainMenu();
            SuccessSound();
        }
        else
        {
            terminalOutput.text = "Command not recognized.\nType 'help' to see available commands.\n";
            ErrorSound();
        }
    }

    // Handle going back from different states
    private void HandleBackCommand()
    {
        switch (currentState)
        {
            case ComputerState.CameraMenu:
            case ComputerState.AccessControlMenu:
                EnterSecurityMainMenu();  // Go back to the Security Main Menu from sub-menus
                break;
            case ComputerState.SecurityMainMenu:
                terminalOutput.text = "Returning to the main menu.\nType 'help' to see all available commands.";
                currentState = ComputerState.HelpMenu;  // Go back to the Help Menu
                break;
            case ComputerState.ViewingLog:
                DisplayLogMenu();  // Go back to logs menu
                isViewingLog = false;
                currentState = ComputerState.LogsMenu;
                break;
            default:
                terminalOutput.text = "Cannot go back.";
                break;
        }
    }


    private void DisplayLogMenu()
    {
        // Get the list of logs with numbers, dates, and titles
        List<string> logList = computerLogs.GetLogList();
        terminalOutput.text = "Available logs:\n";
        SuccessSound();
        foreach (string log in logList)
        {
            terminalOutput.text += log + "\n";
        }
        terminalOutput.text += "\nType a log number to view its content or 'back' to return to the main menu.\n";

        // Track the state change so we can go back from the logs menu
        previousState = currentState;
        currentState = ComputerState.LogsMenu;
    }



    private void HandleSecurityCode(string input)
    {
        if (computerLogs.EnterSecurityCode(input))
        {
            terminalOutput.text = "Security code accepted.\nType 'logs' to access available logs.\n";
        }
        else
        {
            terminalOutput.text = "Incorrect security code. Try again:\n";
        }
        isEnteringSecurityCode = false;
    }

    // Security Main Menu (Cameras or Access Control)
    private void EnterSecurityMainMenu()
    {
        isConfirmingRewrite = false;  // Reset the confirmation state
        terminalOutput.text = "Security options:\n - cameras\n - access control\nType 'back' to return to the main menu.\n";
        currentState = ComputerState.SecurityMainMenu;
    }


    // Security camera menu
    private void EnterCameraMenu()
    {
        previousState = currentState; // Track where we came from
        currentState = ComputerState.CameraMenu;
        inSecurityMenu = true;
        terminalOutput.text = "Select a camera to view:\n - control\n - machine\n - storage\nType 'back' to exit security mode.\n";
        DeactivateAllCameras();  // Disable all cameras when entering the menu
        SuccessSound();
    }

    // Access Control Menu
    private void EnterAccessControlMenu()
    {
        currentState = ComputerState.AccessControlMenu;
        SuccessSound();

        // Display guide text and options for rewriting the keycard and returning to the previous menu
        terminalOutput.text = "Access Control Menu:\n\n";
        terminalOutput.text += "This menu allows you to manage the access control system for keycard-enabled doors within the facility. Keycards are used to restrict or grant access to secure areas, and this system provides authorized personnel the ability to rewrite keycard permissions when necessary.\n";
        terminalOutput.text += "Please ensure that keycards are only rewritten by authorized individuals, and that all changes comply with company security protocols.\n\n";
        terminalOutput.text += "Instructions:\n";
        terminalOutput.text += "Ensure that the keycard is properly inserted into the card reader before proceeding.\n";
        terminalOutput.text += "Rewriting a keycard will overwrite its current access permissions with the new configuration.\n\n";
        terminalOutput.text += "\nType 'rewrite keycard' to rewrite the keycard.\n";
        terminalOutput.text += "Type 'back' to return to the security menu.\n";
    }

    private void EnterPlatformPuzzle()
    {
        terminalOutput.text = "Welcome to the Alignment Puzzle!\n\n" +
                              "Press SPACE to align each platform in the middle.\n" +
                              "Type 'exit' to leave the puzzle.\n";
        currentState = ComputerState.PlatformPuzzle;
        StartPlatformPuzzle();
    }

    private void StartPlatformPuzzle()
    {
        isPuzzleActive = true;
        currentPlatform = 0;

        // Ensure platformPositions and platformSpeeds arrays match the number of platforms
        if (platformPositions == null || platformPositions.Length != platforms.Length)
        {
            platformPositions = new float[platforms.Length];
        }

        if (platformSpeeds == null || platformSpeeds.Length != platforms.Length)
        {
            platformSpeeds = new float[platforms.Length];
        }

        // Reset platform positions to the middle and assign progressive speeds
        for (int i = 0; i < platformPositions.Length; i++)
        {
            platformPositions[i] = 0.5f; // Start each platform in the middle

            // Assign progressive speeds (e.g., baseSpeed + index * increment)
            float baseSpeed = 0.5f; // Starting speed for the first platform
            float speedIncrement = 0.2f; // Speed increment for each subsequent platform
            platformSpeeds[i] = baseSpeed + i * speedIncrement; // Progressive speed
        }

        StartCoroutine(MovePlatforms());
    }


    private IEnumerator MovePlatforms()
    {
        while (isPuzzleActive)
        {
            if (currentPlatform < platformPositions.Length)
            {
                // Align the current platform to X-axis = 0 if necessary
                if (isAligning)
                {
                    Quaternion targetRotation = Quaternion.Euler(0f, 0f, 0f); // Neutral position
                    Transform platformParent = platforms[currentPlatform].transform.parent;

                    // Smoothly align the platform to the target rotation
                    platformParent.localRotation = Quaternion.Slerp(
                        platformParent.localRotation,
                        targetRotation,
                        Time.deltaTime * 2f // Adjust speed as needed
                    );

                    // Check if alignment is complete
                    if (Quaternion.Angle(platformParent.localRotation, targetRotation) <= 0.1f)
                    {
                        // Finalize alignment
                        platformParent.localRotation = targetRotation;
                        isAligning = false; // Mark alignment as complete

                        // Start swinging the platform
                        terminalOutput.text = $"Platform {currentPlatform + 1} aligned! Starting swing...\n";
                    }

                    yield return null; // Wait until alignment is complete
                }
                else
                {
                    // Use platform-specific speed for swinging
                    platformPositions[currentPlatform] += platformSpeeds[currentPlatform] * Time.deltaTime;

                    // Wrap platformPositions[currentPlatform] around to stay within 0.0 to 1.0
                    if (platformPositions[currentPlatform] > 1.0f)
                    {
                        platformPositions[currentPlatform] -= 1.0f;
                    }

                    // Update the visual representation
                    UpdatePlatformDisplay();

                    yield return null; // Wait for the next frame
                }
            }
            else
            {
                isPuzzleActive = false;
                terminalOutput.text = "All platforms aligned! The bridge is complete.\nType 'exit' to leave the puzzle.\n";
            }
        }
    }

    private void AlignPlatform()
    {
        if (isAligning || currentPlatform >= platforms.Length)
            return; // Prevent interaction while aligning or if no platform is active

        isAligning = true;
        terminalOutput.text = $"Aligning Platform {currentPlatform + 1}...\n";

        StartCoroutine(AlignCurrentPlatform());
    }

    private IEnumerator AlignCurrentPlatform()
    {
        Transform platformParent = platforms[currentPlatform].transform.parent;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, 0f); // Neutral position

        // Smoothly align the platform to the target rotation
        while (Quaternion.Angle(platformParent.localRotation, targetRotation) > 0.1f)
        {
            platformParent.localRotation = Quaternion.Slerp(
                platformParent.localRotation,
                targetRotation,
                Time.deltaTime * 2f // Adjust speed as needed
            );

            yield return null; // Wait for the next frame
        }

        // Finalize alignment
        platformParent.localRotation = targetRotation;
        terminalOutput.text = $"Platform {currentPlatform + 1} aligned! Starting swing...\n";
        SuccessSound();

        // Mark alignment as complete and move to the next platform
        isAligning = false;
        currentPlatform++;
    }



    private void UpdatePlatformDisplay()
    {
        terminalOutput.text = "Platform Puzzle:\n";

        for (int i = 0; i < platforms.Length; i++)
        {
            if (i < currentPlatform)
            {
                // Completed platforms swing naturally
                float maxSwingOffset = 36.59f; // Maximum swing offset
                float swingProgress = platformPositions[i] * Mathf.PI * 2f; // Full sine wave cycle
                float swingOffset = maxSwingOffset * Mathf.Sin(swingProgress);

                Quaternion swingRotation = Quaternion.Euler(swingOffset, 0f, 0f);
                platforms[i].transform.parent.localRotation = swingRotation;
            }
            else if (i == currentPlatform)
            {
                if (isAligning)
                {
                    // Skip logic while the platform is aligning (handled in AlignCurrentPlatform)
                    continue;
                }
                else
                {
                    // Current platform swings naturally after alignment
                    float maxSwingOffset = 36.59f;
                    float swingProgress = platformPositions[i] * Mathf.PI * 2f;
                    float swingOffset = maxSwingOffset * Mathf.Sin(swingProgress);

                    Quaternion swingRotation = Quaternion.Euler(swingOffset, 0f, 0f);
                    platforms[i].transform.parent.localRotation = swingRotation;
                }
            }
            else
            {
                // Unaddressed platforms remain in their initial randomized rotation
                platforms[i].transform.parent.localRotation = initialRotations[i];
            }

            // Update the terminal display
            string positionDisplay = i == currentPlatform && isAligning
                ? ">> Aligning..."
                : new string(' ', Mathf.RoundToInt((Mathf.Sin(platformPositions[i] * Mathf.PI * 2f) + 1) * 10)) + "|";
            terminalOutput.text += $"Platform {i + 1}: {positionDisplay}\n";
        }
    }

    private void RestartPuzzle()
    {
        isPuzzleActive = false;
        currentPlatform = 0;

        // Reset platform positions and alignment states
        for (int i = 0; i < platformPositions.Length; i++)
        {
            platformPositions[i] = 0.5f;  // Reset all positions to the center

            // Reset platforms to their initial randomized rotation
            platforms[i].transform.parent.localRotation = initialRotations[i];
        }

        // Restart the puzzle after a short delay
        StartCoroutine(RestartDelay());
    }

    private IEnumerator RestartDelay()
    {
        yield return new WaitForSeconds(1.5f); // Delay before restart to give feedback

        terminalOutput.text = "Welcome to the Alignment Puzzle!\n\n" +
                              "Press SPACE to align each platform in the middle.\n" +
                              "Type 'exit' to leave the puzzle.\n";

        isPuzzleActive = true;
        StartCoroutine(MovePlatforms());
    }

    private void HandleCameraCommands(string input)
    {
        if (input.ToLower() == "back")
        {
            // Deactivate all cameras and clear the display when going back
            DeactivateAllCameras();  // This function will deactivate the cameras and clear the screen
            EnterSecurityMainMenu(); // Go back to the security main menu
        }
        else if (input.ToLower() == "control")
        {
            // Activate control room camera
            ActivateCamera(0, controlRoomTexture, controlRoomCamera, "Control Room camera active.");
        }
        else if (input.ToLower() == "machine")
        {
            // Activate machine camera
            ActivateCamera(1, machineTexture, machineCamera, "Machine camera active.");
        }
        else if (input.ToLower() == "storage")
        {
            // Activate storage camera
            ActivateCamera(2, storageTexture, storageCamera, "Storage camera active.");
        }
        else
        {
            terminalOutput.text = "Invalid camera selection. Type 'back' to return to the camera menu.\n";
            ErrorSound();
        }
    }

    private void HandleSecurityCommands(string input)
    {
        if (input.ToLower() == "back")
        {
            HandleBackCommand();  // Go back to the main menu
        }
        else if (input.ToLower() == "cameras")
        {
            EnterCameraMenu();  // Enter the camera menu
        }
        else if (input.ToLower() == "access control")
        {
            EnterAccessControlMenu();  // Enter the access control menu
        }
        else
        {
            terminalOutput.text = "Invalid selection. Type 'back' to return to the security menu.\n";
            ErrorSound();
        }
    }

    // Handle Access Control Commands (rewriting keycard here)
    private void HandleAccessControlCommands(string input)
    {
        if (isConfirmingRewrite)
        {
            // Handle confirmation response
            if (input.ToLower() == "yes")
            {
                if (cardInsertScript != null && cardInsertScript.HasInsertedKeycard())
                {
                    cardInsertScript.MarkKeycardAsRewritten();
                    terminalOutput.text = "Keycard has been rewritten successfully.\nType 'back' to return to the security menu.\n";
                    StartKeycardProcessingSound();
                }
                else
                {
                    terminalOutput.text = "No keycard detected in the reader.\n";
                }
                isConfirmingRewrite = false;  // Exit confirmation state
            }
            else if (input.ToLower() == "no")
            {
                terminalOutput.text = "Keycard rewrite operation cancelled.\n";
                isConfirmingRewrite = false;  // Exit confirmation state
                ErrorSound();
            }
            else
            {
                terminalOutput.text = "Invalid response. Type 'yes' to proceed or 'no' to cancel.\n";
                ErrorSound();
            }
        }
        else
        {
            if (input.ToLower() == "back")
            {
                EnterSecurityMainMenu();  // Return to the security main menu
            }
            else if (input.ToLower().Trim() == "rewrite keycard")
            {
                if (cardInsertScript != null && cardInsertScript.HasInsertedKeycard())
                {
                    // Ask for confirmation
                    terminalOutput.text = "Are you sure you want to rewrite this keycard?\n\nType 'yes' to proceed, type 'no' to cancel.\n";
                    isConfirmingRewrite = true;  // Set the confirmation state
                    ErrorSound();
                }
                else
                {
                    terminalOutput.text = "No keycard detected in the reader.\nType 'back' to return to the security menu.\n";
                    ErrorSound();
                }
            }
            else
            {
                terminalOutput.text = "Invalid selection. Type 'back' to return to the security menu.\n";
                ErrorSound();
            }
        }
    }

    // Activate a specific camera
    private void ActivateCamera(int cameraIndex, RenderTexture texture, GameObject cameraObject, string message)
    {
        DeactivateAllCameras();  // Turn off any previously active cameras
        cameraObject.SetActive(true);  // Activate the selected camera
        securityDisplay.enabled = true;
        securityDisplay.texture = texture;  // Set the texture on the RawImage
        terminalOutput.text = message + "\nType 'back' to return to the camera menu.\n";
        currentCameraIndex = cameraIndex;
    }

    // Deactivate all cameras and clear the screen
    private void DeactivateAllCameras()
    {
        controlRoomCamera.SetActive(false);
        machineCamera.SetActive(false);
        storageCamera.SetActive(false);
        securityDisplay.enabled = false;
        securityDisplay.texture = null;  // Clear the RawImage display
        currentCameraIndex = -1;  // No camera is currently active
    }

    // Keycard Processing Sound
    void StartKeycardProcessingSound()
    {
        if (keycardProcessing != null)
        {
            keycardProcessing.Play();  // Start playing the keycard processing sound
        }
    }

    // Error Sound
    void ErrorSound()
    {
        if (ErrorSoundEffect != null)
        {
            ErrorSoundEffect.Play();  // Start playing the keycard processing sound
        }
    }

    // Error Sound
    void SuccessSound()
    {
        if (SuccessSoundEffect != null)
        {
            SuccessSoundEffect.Play();  // Start playing the keycard processing sound
        }
    }
}
