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
    public Light chamberElectricLight;
    public GameObject handObject;
    public Animator handAnimator;

    private bool canType = false;
    private bool isChecking = false;
    private bool isWarningScreen = false;
    private int checkIndex = 0;
    private List<string> requiredParameters = new List<string> { "BOLTS_SECURE", "OIL_LEVEL", "COOLANT_LEVEL", "BATTERY_INSERTED", "BATTERY_CHARGE" };

    void Start()
    {
        ShowConfigurationScreen();
        if (chamberElectricLight != null) chamberElectricLight.enabled = false;
        handAnimator.enabled = false;  // Disable at start
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

    private IEnumerator ChamberLightDelay()
    {
        yield return new WaitForSeconds(7.5f);
        if (chamberElectricLight != null) chamberElectricLight.enabled = true;
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

            if (isChecking)
            {
                DisplayNextParameter();
            }
            else if (isWarningScreen && input.ToLower() == "yes")
            {
                StartCoroutine(StartChamberProcess());
            }
            else if (input.ToLower() == "check")
            {
                isChecking = true;
                checkIndex = 0;
                DisplayNextParameter();
            }
            else if (input.ToLower() == "continue")
            {
                ShowWarningScreen();
            }
            else
            {
                terminalOutput.text = "Unknown command. Type 'check' or 'continue'.\n";
                errorSound.Play();
            }

            inputField.text = "";
            inputField.ActivateInputField();
            inputField.Select();
        }
    }

    private void ShowConfigurationScreen()
    {
        terminalOutput.text = "+-------------------------+\n" +
                             "| Parameter         | Status |\n" +
                             "+-------------------------+\n" +
                             FormatParameterTable() +
                             "+-------------------------+\n" +
                             "Type 'check' to verify parameters.\n";
    }

    private void DisplayNextParameter()
    {
        if (checkIndex < requiredParameters.Count)
        {
            terminalOutput.text += requiredParameters[checkIndex] + "____________________OK\n";
            checkIndex++;
        }
        else
        {
            terminalOutput.text += "\nALL SYSTEMS OPERATIONAL\nTYPE 'continue' TO START THE PROCESS";
            isChecking = false;
        }
    }

    private void ShowWarningScreen()
    {
        terminalOutput.text = "WARNING:\nEnsure the chamber is closed.\nAvoid exposure to the beam.\nContact medical immediately if exposed.\nAdverse effects include cognitive dissonance, extreme nausea, and hallucinations.\n\nDo you want to proceed? (Type 'yes' to proceed)";
        isWarningScreen = true;
    }

    private IEnumerator StartChamberProcess()
    {
        activationSound.Play();
        StartCoroutine(ChamberLightDelay());
        yield return new WaitForSeconds(10f);
        if (chamberElectricLight != null) chamberElectricLight.enabled = false;
        // Play hand animation
        handAnimator.enabled = true;
        if (handAnimator != null)
        {
            handAnimator.SetTrigger("ActivateHand");  // Ensure a trigger exists in Animator
        }
        terminalOutput.text = "PROCESS COMPLETE. RETRIEVE HAND SAFELY.\n";
        successSound.Play();
        if (handObject != null)
        {
            handObject.tag = "RewrittenHand";
            Debug.Log("Hand tag changed to RewrittenHand");
        }
    }

    private string FormatParameterTable()
    {
        int paramColumnWidth = requiredParameters.Max(param => param.Length) + 2;
        int statusColumnWidth = 6;
        string result = "";

        foreach (string param in requiredParameters)
        {
            result += string.Format("| {0,-18} | {1,-6} |\n", param, "OK");
        }
        return result;
    }
}
