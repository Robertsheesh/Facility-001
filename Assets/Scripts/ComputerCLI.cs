using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ComputerTerminal : MonoBehaviour
{
    public Text terminalText;  // The text element that displays the terminal output
    public InputField commandInput;  // The input field where the player types
    public ScrollRect terminalScrollRect;  // ScrollRect for scrolling the terminal content
    public RectTransform terminalContent;  // Reference to the Content RectTransform of the ScrollRect

    private bool loggedIn = false;  // Track if the player is logged in
    private bool waitingForPassword = false;  // Track if the player is inputting the password
    private string username = "";  // Store the typed username
    private string password = "";  // Store the typed password
    private string currentView = "terminal";  // Track what the player is viewing (terminal, logMenu, logView)

    private string validUsername = "employee";  // The correct username
    private string validPassword = "aetheris";  // The correct password

    // Log data
    private string[] logEntries = {
        "Log #1 - Sat 15 18:15:23 UTC 2102\n\n",
        "Log #2 - Fri 14 10:02:19 UTC 2102\n\n",
        "Log #3 - Thu 13 21:45:33 UTC 2100\n\n",
        "Log #4 - Wed 12 17:27:49 UTC 2100\n\n",
        "Log #5 - Tue 11 09:30:41 UTC 2100\n\n"
    };

    private string[] logContents = {
        "Content of Log #1",
        "Content of Log #2",
        "Content of Log #3",
        "Content of Log #4",
        "Content of Log #5"
    };

    private string bootSequenceText =
        "Initializing system...\n" +
        "Loading modules...\n" +
        "Configuring network...\n" +
        "System online...\n\n";
    private string welcomeMessage = "Aetheris Dynamics 2.21\nLogin: ";  // Initial message
    private string prompt = "\nroot@factory:~$ ";  // Prompt after login

    void Start()
    {
        commandInput.gameObject.SetActive(false);  // Disable input initially
        StartCoroutine(PlayBootSequence());  // Start the boot-up animation

        terminalContent.anchorMin = new Vector2(0, 1);
        terminalContent.anchorMax = new Vector2(1, 1);
        terminalContent.pivot = new Vector2(0.5f, 1);
    }

    // Coroutine to simulate the boot-up animation
    IEnumerator PlayBootSequence()
    {
        terminalText.text = "";  // Clear terminal at start
        string[] bootLines = bootSequenceText.Split('\n');  // Split boot text into lines

        foreach (string line in bootLines)
        {
            terminalText.text += line + "\n";
            AdjustContentHeight();
            ScrollToBottom();  // Ensure the text scrolls as the lines are added
            yield return new WaitForSeconds(0.5f);  // Delay between boot lines
        }

        terminalText.text += welcomeMessage;
        AdjustContentHeight();
        commandInput.gameObject.SetActive(true);  // Enable input field after boot sequence
        commandInput.ActivateInputField();  // Focus the input field
    }

    void Update()
    {
        // Only process input when the "Enter" key is pressed
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            HandleInput(commandInput.text);
        }
    }

    void HandleInput(string userInput)
    {
        if (currentView == "logMenu")
        {
            HandleLogMenuInput(userInput);  // Handle log menu input
        }
        else if (currentView == "logView")
        {
            HandleLogViewInput(userInput);  // Handle log view input
        }
        else
        {
            if (waitingForPassword)
            {
                password = userInput;

                if (username == validUsername && password == validPassword)
                {
                    ShowAfterLoginScreen();
                    waitingForPassword = false;
                    loggedIn = true;
                }
                else
                {
                    terminalText.text = "";  // Clear the terminal
                    terminalText.text += "\nLogin incorrect.\n\nLogin: ";
                    username = "";
                    waitingForPassword = false;
                }
            }
            else if (!loggedIn)
            {
                username = userInput;
                terminalText.text = "";  // Clear the terminal before displaying the new message
                terminalText.text += username + "\nPassword: ";
                commandInput.text = "";
                waitingForPassword = true;
            }
            else
            {
                // Clear the terminal before showing the new output
                terminalText.text = "";
                HandleCommand(userInput);
            }
        }

        commandInput.text = "";
        commandInput.ActivateInputField();
        ScrollToBottom();
    }

    void ShowAfterLoginScreen()
    {
        terminalText.text = "";  // Clear the terminal before displaying the new screen
        terminalText.text += password +
                            "\nLast login: Mon Sep 17 23:22:09 UTC 2102" +
                            "\n\nType help to see all available commands." + prompt;
        AdjustContentHeight();
        ScrollToBottom();
    }

    void HandleCommand(string command)
    {
        terminalText.text = "\n\n" +
                            "\n\n";  // Add a new line to prevent the top from clipping
        switch (command.ToLower())
        {
            case "help":
                terminalText.text += "\n\n" +
                                     "\n>LOGS (Review employee activity logs)\n\n" +
                                     ">STORAGE (Access storage unit data)\n\n" +
                                     ">CLEAR (Clear the terminal display)" +
                                     "\n\n" + prompt;
                break;

            case "logs":
                StartCoroutine(DisplayLogs());  // Display the log menu
                break;

            case "storage":
                terminalText.text += "\nAccessing storage..." + prompt;
                break;

            case "clear":
                ExitTerminal();
                break;

            default:
                terminalText.text += "\nUnknown command: " + command + prompt;
                break;
        }

        AdjustContentHeight();
        ScrollToBottom();
    }

    // Coroutine to display the logs after showing a "loading" message for 0.5 seconds
    IEnumerator DisplayLogs()
    {
        terminalText.text += "\nShowing logs...";
        AdjustContentHeight();
        ScrollToBottom();
        yield return new WaitForSeconds(0.5f);

        // Show the log menu with log entries and dates
        terminalText.text = "\n--- Available Logs ---\n";
        for (int i = 0; i < logEntries.Length; i++)
        {
            terminalText.text += $"{logEntries[i]}\n";
        }

        terminalText.text += "\nType the log number to view or 'exit' to return to the terminal.";
        AdjustContentHeight();
        ScrollToBottom();

        // Switch to log menu input mode
        currentView = "logMenu";
    }

    // Handle input in the log menu
    void HandleLogMenuInput(string input)
    {
        int logNumber;
        if (input.ToLower() == "exit")
        {
            terminalText.text += "\nExiting logs..." + prompt;
            currentView = "terminal";  // Return to the terminal
        }
        else if (int.TryParse(input, out logNumber) && logNumber >= 1 && logNumber <= logEntries.Length)
        {
            ShowLogContent(logNumber - 1);  // Show the selected log
        }
        else
        {
            terminalText.text += "\nInvalid input. Type the log number or 'exit' to return.";
        }

        AdjustContentHeight();
        ScrollToBottom();
    }

    // Show the content of the selected log
    void ShowLogContent(int logIndex)
    {
        terminalText.text = $"\n--- {logEntries[logIndex]} ---\n{logContents[logIndex]}\n\nType 'back' to return to the logs menu.";
        AdjustContentHeight();
        ScrollToBottom();

        // Switch to log view input mode
        currentView = "logView";
    }

    // Handle input when viewing a log
    void HandleLogViewInput(string input)
    {
        if (input.ToLower() == "back")
        {
            StartCoroutine(DisplayLogs());  // Go back to the log menu
        }
        else
        {
            terminalText.text += "\nType 'back' to return to the logs menu.";
        }

        AdjustContentHeight();
        ScrollToBottom();
    }

    // Scroll the ScrollRect to the bottom to show the latest text
    void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();
        terminalScrollRect.verticalNormalizedPosition = 0f;
    }

    // Adjust the content height dynamically
    void AdjustContentHeight()
    {
        // Ensure the content height adjusts to fit the text
        terminalContent.sizeDelta = new Vector2(terminalContent.sizeDelta.x, terminalText.preferredHeight);
    }

    // Clear the terminal and reset the login screen
    void ExitTerminal()
    {
        terminalText.text = welcomeMessage;
        username = "";
        password = "";
        loggedIn = false;
        waitingForPassword = false;
        currentView = "terminal";
        AdjustContentHeight();
        ScrollToBottom();
    }
}
