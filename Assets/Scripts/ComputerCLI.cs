using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ComputerCLI : MonoBehaviour
{
    public Text terminalOutput;      // The text element to display terminal output
    public InputField inputField;    // The input field for user input
    public float inputDelay = 1f;    // Delay time before input is enabled (in seconds)

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

    private const string correctUsername = "employee";  // The correct username
    private const string correctPassword = "aetheris";  // The correct password

    private ComputerLogs computerLogs;  // Reference to the ComputerLogs script

    private enum ComputerState { Login, HelpMenu, LogsMenu, SecurityMenu, ViewingLog }
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
            else if (inSecurityMenu)
            {
                HandleSecurityCommands(input);
            }
            else
            {
                HandleCommands(input);
            }

            inputField.text = "";
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
            }
        }
        else
        {
            if (input == correctPassword)
            {
                isLoggedIn = true;
                terminalOutput.text = "Last login: Mon Sep 17 23:22:09 UTC 2102\n\nType help to see all available commands.\n";
            }
            else
            {
                terminalOutput.text = "Login incorrect\nLogin:\n";
                isEnteringPassword = false;
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
            }
            else if (currentState == ComputerState.LogsMenu)
            {
                // Go back to the help menu from the logs menu
                terminalOutput.text = "Returning to the main menu.\nType 'help' to see all available commands.\n";
                currentState = ComputerState.HelpMenu;
            }
            else if (currentState == ComputerState.SecurityMenu)
            {
                // Go back to the help menu from the security menu
                terminalOutput.text = "Exiting security mode.\nType 'help' to see available commands.\n";
                currentState = ComputerState.HelpMenu;
                DeactivateAllCameras();
            }
        }
        else if (input.ToLower() == "help")
        {
            terminalOutput.text = "Available commands:\n - help\n - logout\n - logs\n - security\n";
            currentState = ComputerState.HelpMenu;
        }
        else if (input.ToLower() == "logout")
        {
            isLoggedIn = false;
            isEnteringPassword = false;
            terminalOutput.text = "You have been logged out.\nLogin:\n";
            currentState = ComputerState.Login;
            DeactivateAllCameras();
        }
        else if (input.ToLower() == "logs")
        {
            if (computerLogs.HasAccessToLogs())
            {
                // Display the list of logs and enter the LogsMenu state
                DisplayLogMenu();
                currentState = ComputerState.LogsMenu;
            }
            else
            {
                terminalOutput.text = "Please enter the security code to access logs:\n";
                isEnteringSecurityCode = true;
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
                terminalOutput.text = "Log not found.\n";
            }
        }
        else if (input.ToLower() == "security")
        {
            EnterSecurityMenu();
        }
        else
        {
            terminalOutput.text = "Command not recognized.\n";
        }
    }

    private void HandleBackCommand()
    {
        switch (currentState)
        {
            case ComputerState.LogsMenu:
            case ComputerState.SecurityMenu:
            case ComputerState.ViewingLog:
                terminalOutput.text = "Returning to help menu.\nType 'help' to see available commands.";
                currentState = ComputerState.HelpMenu;
                break;

            case ComputerState.HelpMenu:
                // Can't go back from the help menu
                terminalOutput.text = "You are already at the main menu. Type 'help' to see available commands.";
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

    // Security camera menu
    private void EnterSecurityMenu()
    {
        previousState = currentState; // Track where we came from
        currentState = ComputerState.SecurityMenu;
        inSecurityMenu = true;
        terminalOutput.text = "Select a camera to view:\n - control\n - machine\n - storage\nType 'back' to exit security mode.\n";
        DeactivateAllCameras();  // Disable all cameras when entering the menu
    }


    private void HandleSecurityCommands(string input)
    {
        if (input.ToLower() == "back")
        {
            inSecurityMenu = false;
            terminalOutput.text = "Returning to the main menu.\nType 'help' to see available commands.\n";
            currentState = ComputerState.HelpMenu; // Return to the help menu
            DeactivateAllCameras();  // Disable cameras when exiting
        }
        else if (input.ToLower() == "control")
        {
            ActivateCamera(0, controlRoomTexture, controlRoomCamera, "Control Room entrance camera active.");
        }
        else if (input.ToLower() == "machine")
        {
            ActivateCamera(1, machineTexture, machineCamera, "Machine camera active.");
        }
        else if (input.ToLower() == "storage")
        {
            ActivateCamera(2, storageTexture, storageCamera, "Storage camera active.");
        }
        else
        {
            terminalOutput.text = "Invalid selection. Type 'back' to return to the main menu.\n";
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
}
