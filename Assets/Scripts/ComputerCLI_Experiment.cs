using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ComputerCLIExperiment : MonoBehaviour
{
    public Text terminalOutput;
    public InputField inputField;
    public ExperimentDoor experimentDoor;
    public AudioSource activationSound;
    public AudioSource errorSound;
    public AudioSource successSound;

    private string welcomeText = "Aetheris Dynamics 2.21\nLogin:\n";
    private bool canType = false;
    private bool isLoggedIn = false;
    private bool isConfiguring = false;
    private bool isActivating = false;
    private Dictionary<string, string> chamberParameters = new Dictionary<string, string>();
    private List<string> requiredParameters = new List<string> { "BOLTS_SECURE", "OIL_LEVEL", "COOLANT_LEVEL", "BATTERY_INSERTED", "BATTERY_CHARGE" };

    void Start()
    {
        terminalOutput.text = welcomeText;
        DisableInput();
    }

    public void EnableInput()
    {
        StartCoroutine(EnableInputFieldAfterDelay());
    }

    public void DisableInput()
    {
        canType = false;
        inputField.interactable = false;
        inputField.DeactivateInputField();
        inputField.text = "";
    }

    private IEnumerator EnableInputFieldAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        canType = true;
        inputField.interactable = true;
        inputField.ActivateInputField();
        inputField.Select();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && canType)
        {
            SubmitInput(inputField.text);
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
            else if (isConfiguring)
            {
                HandleConfiguration(input);
            }
            else if (isActivating)
            {
                HandleActivation(input);
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
        if (input == "admin")
        {
            isLoggedIn = true;
            terminalOutput.text = "Welcome to Aetheris Dynamics System.\nType 'help' to see available commands.\n";
            successSound.Play();
        }
        else
        {
            terminalOutput.text = "Login incorrect\nLogin:\n";
            errorSound.Play();
        }
    }

    private void HandleCommands(string input)
    {
        if (input.ToLower() == "help")
        {
            terminalOutput.text = "Available commands:\n - beam_chamber\n - logs\n - logout\n";
        }
        else if (input.ToLower() == "beam_chamber")
        {
            terminalOutput.text = "Biometric Reconstitution Chamber\nOptions:\n - configuration\n - activate\n - back\n";
        }
        else if (input.ToLower() == "configuration")
        {
            terminalOutput.text = "+-------------------------+\n" +
                                 "| Parameter         | Status |\n" +
                                 "+-------------------------+\n" +
                                 FormatParameterTable() +
                                 "+-------------------------+\n" +
                                 "Type 'back' when done.\n";
            isConfiguring = true;
        }
        else if (input.ToLower() == "activate")
        {
            if (ValidateParameters())
            {
                if (experimentDoor.IsDoorClosed())
                {
                    terminalOutput.text = "Parameters:\n" + FormatParameterTable() + "\nALL SYSTEMS OPERATIONAL\nTYPE CONTINUE TO START THE PROCESS";
                    isActivating = true;
                }
                else
                {
                    terminalOutput.text = "ERROR: Close the chamber door first.\n";
                    errorSound.Play();
                }
            }
            else
            {
                terminalOutput.text = "ERROR: Missing or incorrect parameters. Check logs.\n";
                errorSound.Play();
            }
        }
        else if (input.ToLower() == "logs")
        {
            terminalOutput.text = "Chamber log:\n" + FormatParameterTable() + "\nType 'back' to return.\n";
        }
        else if (input.ToLower() == "logout")
        {
            isLoggedIn = false;
            terminalOutput.text = welcomeText;
        }
        else if (input.ToLower() == "back")
        {
            terminalOutput.text = "Returning to the main menu.\nType 'help' for available commands.\n";
            isConfiguring = false;
            isActivating = false;
        }
        else
        {
            terminalOutput.text = "Unknown command. Type 'help' for options.\n";
            errorSound.Play();
        }
    }

    private string FormatParameterTable()
    {
        int paramColumnWidth = requiredParameters.Max(param => param.Length) + 2;
        int statusColumnWidth = 6;  // Fixed width for "OK" + padding
        int totalWidth = paramColumnWidth + statusColumnWidth + 7; // Account for table borders

        string border = new string('-', totalWidth);
        string result = $"| {"Parameter".PadRight(paramColumnWidth)} | {"Status".PadRight(statusColumnWidth)} |\n";
        result += border + "\n";

        foreach (string param in requiredParameters)
        {
            result += $"| {param.PadRight(paramColumnWidth)} | {"OK".PadRight(statusColumnWidth)} |\n";
        }
        result += border + "\n";

        return result;
    }



    private IEnumerator StartChamberProcess()
    {

        activationSound.Play();
        yield return new WaitForSeconds(3f);
        terminalOutput.text = "PROCESS COMPLETE. RETRIEVE HAND SAFELY.\n";
        successSound.Play();
    }

    private bool ValidateParameters()
    {
        return true;
    }

    private void HandleActivation(string input)
    {
        if (input.ToLower() == "continue")
        {
            terminalOutput.text = "WARNING:\nEnsure the chamber is closed.\nAvoid exposure to the beam.\nContact medical immediately if exposed.\nAdverse effects include cognitive dissonance, extreme nausea, and hallucinations.\nTYPE CONTINUE TO PROCEED";
            isActivating = false;
            StartCoroutine(StartChamberProcess());
        }
        else
        {
            terminalOutput.text = "Invalid command. Type 'continue' to proceed.\n";
        }
    }

    private void HandleConfiguration(string input)
    {
        if (input.ToLower() == "back")
        {
            isConfiguring = false;
            terminalOutput.text = "Configuration complete. Type 'activate' to start.\n";
            return;
        }

        string[] parts = input.Split(' ');
        if (parts.Length == 2 && requiredParameters.Contains(parts[0].ToUpper()))
        {
            chamberParameters[parts[0].ToUpper()] = parts[1].ToUpper();
            terminalOutput.text = "Parameter set: " + parts[0].ToUpper() + " " + parts[1].ToUpper() + "\n";
        }
        else
        {
            terminalOutput.text = "Invalid input. Format: PARAMETER VALUE\n";
        }
    }

}
