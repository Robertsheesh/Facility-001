using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ComputerCLI : MonoBehaviour
{
    public Text terminalOutput;      // The text element to display terminal output
    public InputField inputField;    // The input field for user input
    public float inputDelay = 1f;    // Delay time before input is enabled (in seconds)

    private string welcomeText = "Aetheris Dynamics 2.21\nLogin:\n"; // Welcome text
    private bool canType = false;    // Whether the player can start typing

    private bool isLoggedIn = false;       // Track if the user is logged in
    private bool isEnteringPassword = false;  // Track if we are waiting for the password
    private string username = "";          // To store the entered username

    private const string correctUsername = "employee";  // The correct username
    private const string correctPassword = "aetheris";  // The correct password

    void Start()
    {
        // Set initial terminal output
        terminalOutput.text = welcomeText;

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

    // Update is called once per frame
    void Update()
    {
        // Only allow submission with the Enter key and only when canType is true
        if (Input.GetKeyDown(KeyCode.Return) && canType)
        {
            SubmitInput(inputField.text);  // Manually submit input when pressing Enter
        }
    }

    // Method to manually submit input
    private void SubmitInput(string input)
    {
        if (!string.IsNullOrWhiteSpace(input) && canType)
        {
            // Clear the terminal after each input
            terminalOutput.text = "";

            if (!isLoggedIn)
            {
                HandleLogin(input);
            }
            else
            {
                HandleCommands(input);
            }

            // Clear the input field for the next command
            inputField.text = "";

            // Re-focus the input field so the player can continue typing
            inputField.ActivateInputField();
            inputField.Select();
        }
    }

    // Handle the login process (username and password)
    private void HandleLogin(string input)
    {
        if (!isEnteringPassword)
        {
            // User is entering the username
            username = input.ToLower();

            if (username == correctUsername)
            {
                terminalOutput.text += "Password:\n"; // Ask for the password after username is correct
                isEnteringPassword = true;    // Now expect the password
            }
            else
            {
                terminalOutput.text += "\nLogin incorrect\nLogin:\n"; // Reset to username prompt
            }
        }
        else
        {
            // User is entering the password
            if (input == correctPassword)
            {
                isLoggedIn = true; // Successfully logged in
                terminalOutput.text = "Last login: Mon Sep 17 23:22:09 UTC 2102" +
                              "\n\nType help to see all available commands.\n";
            }
            else
            {
                terminalOutput.text = "Login incorrect\nLogin:\n"; // Reset login process
                isEnteringPassword = false; // Go back to username input
            }
        }
    }

    // Handle commands after the user has logged in
    private void HandleCommands(string input)
    {
        // Handle available commands
        if (input.ToLower() == "help")
        {
            terminalOutput.text = "Available commands:\n - help\n - logout\n";
        }
        else if (input.ToLower() == "logout")
        {
            isLoggedIn = false; // Log out the user
            isEnteringPassword = false;
            terminalOutput.text = "You have been logged out.\nLogin:\n";
        }
        else
        {
            terminalOutput.text = "Command not recognized.\n";
        }
    }
}
