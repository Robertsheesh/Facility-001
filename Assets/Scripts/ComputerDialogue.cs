using UnityEngine;
using UnityEngine.UI;
using Ink.Runtime;
using System.Collections.Generic;
using System.Collections;

public class ComputerDialogue : MonoBehaviour
{
    public Text terminalOutput;
    public GameObject topBar;
    public GameObject bottomBar;
    public GameObject playerHUDPanel;
    public Text[] optionTexts;
    public AudioSource typingSound;
    public AudioSource typingSound_player;

    private int currentSelection = 0;
    private bool isDialogueActive = false;
    private bool isTyping = false;

    private Story inkStory;
    public TextAsset inkJSON;
    private Dictionary<string, bool> usedOptions = new Dictionary<string, bool>();
    private List<string> conversationHistory = new List<string>();
    private const int maxVisibleEntries = 2;
    private string npcName;
    private string titleHeader = "Messaging System\n\nConnected to: {npc_name}\n";


    private void Start()
    {
        if (inkJSON == null)
        {
            Debug.LogError("Ink JSON is not assigned. Please drag and drop the file in the inspector.");
            return;
        }

        inkStory = new Story(inkJSON.text);
    }

    public void StartDialogue()
    {
        if (inkStory == null)
        {
            Debug.LogError("Ink story is not initialized. Make sure to assign the JSON file.");
            return;
        }

        isDialogueActive = true;
        playerHUDPanel.SetActive(false);

        // Retrieve npc_name from Ink story and update the title header
        if (inkStory.variablesState["npc_name"] != null)
        {
            npcName = inkStory.variablesState["npc_name"].ToString();
            titleHeader = titleHeader.Replace("{npc_name}", npcName);
        }
        else
        {
            titleHeader = titleHeader.Replace("{npc_name}", "Unknown");
        }

        StartCoroutine(ShowCinematicBars());
    }

    public void OnAccessComputer()
    {
        if (!isDialogueActive)
        {
            StartDialogue();
        }
    }

    private IEnumerator ShowCinematicBars()
    {
        float duration = 1.5f;
        float targetScaleY = 1f;
        float initialScaleY = 0f;

        Transform topBarTransform = topBar.transform;
        Transform bottomBarTransform = bottomBar.transform;

        float time = 0;
        while (time < duration)
        {
            float newScale = Mathf.Lerp(initialScaleY, targetScaleY, time / duration);
            topBarTransform.localScale = new Vector3(1, newScale, 1);
            bottomBarTransform.localScale = new Vector3(1, newScale, 1);
            time += Time.deltaTime;
            yield return null;
        }

        DisplayNextDialogue();
    }

    private void DisplayNextDialogue()
    {
        if (inkStory.canContinue)
        {
            string text = inkStory.Continue().Trim();

            // Check if the text is the player's choice to prevent it from showing twice
            if (usedOptions.ContainsKey(text))
            {
                DisplayNextDialogue();  // Skip the repeated player response
                return;
            }

            StartCoroutine(TypeDialogue(text));
        }
        else
        {
            EndDialogue();
        }
    }


    // Function to check if text is a player response (use markers or known choices)
    private bool IsPlayerResponse(string text)
    {
        return text.StartsWith("> ");  // Player responses formatted this way
    }

    private IEnumerator TypeDialogue(string message)
    {
        isTyping = true;

        string formattedNPCResponse = $"\n[{npcName}]: ";

        terminalOutput.text += formattedNPCResponse;
        yield return StartCoroutine(ShowThinkingDots());

        typingSound.Play();

        foreach (char letter in message.ToCharArray())
        {
            terminalOutput.text += letter;
            yield return new WaitForSeconds(0.05f);
        }

        typingSound.Stop();
        isTyping = false;

        AppendToDialogue($"{formattedNPCResponse}{message}"); // Store NPC response in history

        if (inkStory.currentChoices.Count > 0)
        {
            ShowOptions();
        }
        else
        {
            DisplayNextDialogue();
        }
    }

    private IEnumerator ShowThinkingDots()
    {
        string dots = "";
        int dotCount = Random.Range(3, 6);  // Randomize between 3 to 5 dots

        for (int i = 0; i < dotCount; i++)
        {
            dots += ".";
            terminalOutput.text += dots;
            yield return new WaitForSeconds(Random.Range(0.3f, 0.7f));  // Randomize delay between dots
            terminalOutput.text = terminalOutput.text.TrimEnd('.');
        }

        yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));  // Random wait before NPC starts typing
    }


    private void ShowOptions()
    {
        if (inkStory == null || optionTexts == null || optionTexts.Length == 0)
        {
            return;
        }

        List<Choice> choices = inkStory.currentChoices;

        // Activate the response panel
        playerHUDPanel.SetActive(true);
        currentSelection = 0; // Reset selection to avoid out-of-range errors

        for (int i = 0; i < optionTexts.Length; i++)
        {
            if (i < choices.Count)
            {
                optionTexts[i].text = choices[i].text;
                optionTexts[i].color = usedOptions.ContainsKey(choices[i].text) ? Color.gray : Color.white;
                int choiceIndex = i;

                // Ensure button component exists before using it
                Button button = optionTexts[i].GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => SelectOption(choiceIndex));
                }
            }
            else
            {
                optionTexts[i].text = "";
            }
        }

        HighlightOption();
    }


    private void HighlightOption()
    {
        for (int i = 0; i < optionTexts.Length; i++)
        {
            optionTexts[i].color = (i == currentSelection) ? Color.white : Color.gray;
        }
    }


    private void SelectOption(int choiceIndex)
    {
        if (isTyping || !isDialogueActive) return;

        string selectedChoice = inkStory.currentChoices[choiceIndex].text;

        if (usedOptions.ContainsKey(selectedChoice)) return; // Prevent duplicate selection

        usedOptions[selectedChoice] = true;

        // Type the player's response before continuing the dialogue
        StartCoroutine(TypePlayerResponse(selectedChoice, choiceIndex));
    }

    private IEnumerator TypePlayerResponse(string response, int choiceIndex)
    {
        isTyping = true;
        playerHUDPanel.SetActive(false);
        typingSound_player.Play();

        string formattedResponse = $"\n> {response}";

        foreach (char letter in formattedResponse.ToCharArray())
        {
            terminalOutput.text += letter;
            yield return new WaitForSeconds(0.05f);
        }

        typingSound_player.Stop();
        isTyping = false;
        yield return new WaitForSeconds(1.5f);

        AppendToDialogue(formattedResponse); // Store player response in history

        inkStory.ChooseChoiceIndex(choiceIndex);
        DisplayNextDialogue();
    }

    private void AppendToDialogue(string message)
    {
        conversationHistory.Add(message);

        // Ensure the conversation history respects the maxVisibleEntries limit
        if (conversationHistory.Count > maxVisibleEntries * 2)
        {
            conversationHistory.RemoveRange(0, conversationHistory.Count - (maxVisibleEntries * 2));
        }

        // Refresh the terminal output with the limited conversation history
        terminalOutput.text = titleHeader + "\n";
        foreach (string entry in conversationHistory)
        {
            terminalOutput.text += entry + "\n";
        }
    }

    private IEnumerator DialogueEndDelay()
    {
        yield return new WaitForSeconds(5.0f);
        terminalOutput.text = "Connection terminated\n\nType 'back' to return to the menu";
    }

    private void EndDialogue()
    {
        isDialogueActive = false;
        playerHUDPanel.SetActive(false);
        StartCoroutine(HideCinematicBars());
        StartCoroutine(DialogueEndDelay()); // Start the delay coroutine
    }
    private IEnumerator HideCinematicBars()
    {
        float duration = 1.5f;
        float targetScaleY = 0f;
        float initialScaleY = 1f;

        Transform topBarTransform = topBar.transform;
        Transform bottomBarTransform = bottomBar.transform;

        float time = 0;
        while (time < duration)
        {
            float newScale = Mathf.Lerp(initialScaleY, targetScaleY, time / duration);
            topBarTransform.localScale = new Vector3(1, newScale, 1);
            bottomBarTransform.localScale = new Vector3(1, newScale, 1);
            time += Time.deltaTime;
            yield return null;
        }

        topBar.SetActive(false);
        bottomBar.SetActive(false);
    }

    private void Update()
    {
        if (isDialogueActive && !isTyping)
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
                SelectOption(currentSelection);
            }
        }
    }
}
