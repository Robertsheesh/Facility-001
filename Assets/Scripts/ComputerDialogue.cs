using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ComputerDialogue : MonoBehaviour
{
    public Text dialogueText;         // Reference to the dialogue text UI
    public GameObject dialogueBox;    // UI container for dialogue options
    public Text[] optionTexts;        // UI options for dialogue choices
    public AudioSource typingSound;   // Typing sound effect

    private int currentSelection = 0;  // Index of the selected option
    private bool isDialogueActive = false;

    private string npcName = "Dr. Elizabeth Carter";  // NPC name for realism

    // Example dialogue data (could be expanded to JSON or scriptable objects)
    private string[] dialogueLines =
    {
        "You woke up? How is this possible??",
        "I thought the entire facility was in lockdown...",
        "Listen carefully, you don't have much time!"
    };

    private string[][] playerResponses =
    {
        new string[] { "What happened here?", "Who are you?", "Where am I?" },
        new string[] { "Tell me more.", "I don't understand.", "I need to leave!" },
        new string[] { "Got it!", "Explain further.", "What should I do?" }
    };

    private int dialogueIndex = 0;

    public void StartDialogue()
    {
        isDialogueActive = true;
        dialogueIndex = 0;
        dialogueBox.SetActive(true);
        StartCoroutine(TypeDialogue(dialogueLines[dialogueIndex]));
        UpdateOptions();
    }

    private IEnumerator TypeDialogue(string message)
    {
        dialogueText.text = "";
        typingSound.Play();

        foreach (char letter in message.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.05f);  // Adjust typing speed here
        }

        typingSound.Stop();
    }

    private void UpdateOptions()
    {
        for (int i = 0; i < optionTexts.Length; i++)
        {
            optionTexts[i].text = playerResponses[dialogueIndex][i];
        }
        HighlightOption();
    }

    private void HighlightOption()
    {
        for (int i = 0; i < optionTexts.Length; i++)
        {
            optionTexts[i].color = (i == currentSelection) ? Color.green : Color.white;
        }
    }

    private void SelectOption()
    {
        if (!isDialogueActive) return;

        string selectedResponse = playerResponses[dialogueIndex][currentSelection];

        // Simulate player's response on the computer screen
        FindObjectOfType<ComputerCLI>().DisplayPlayerResponse(selectedResponse);

        dialogueIndex++;

        if (dialogueIndex < dialogueLines.Length)
        {
            StartCoroutine(TypeDialogue(dialogueLines[dialogueIndex]));
            UpdateOptions();
        }
        else
        {
            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        isDialogueActive = false;
        dialogueBox.SetActive(false);
        FindObjectOfType<ComputerCLI>().DialogueEnded();
    }

    void Update()
    {
        if (isDialogueActive)
        {
            if (Input.GetKeyDown(KeyCode.W))  // Navigate up
            {
                currentSelection = (currentSelection > 0) ? currentSelection - 1 : optionTexts.Length - 1;
                HighlightOption();
            }
            if (Input.GetKeyDown(KeyCode.S))  // Navigate down
            {
                currentSelection = (currentSelection < optionTexts.Length - 1) ? currentSelection + 1 : 0;
                HighlightOption();
            }
            if (Input.GetKeyDown(KeyCode.Space))  // Select option
            {
                SelectOption();
            }
        }
    }
}
