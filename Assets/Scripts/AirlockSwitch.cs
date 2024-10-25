using UnityEngine;

public class AirlockSwitch : MonoBehaviour, IInteractable
{
    public AirlockController airlockController; // Reference to the AirlockController

    // Called when the player interacts with the switch
    public void Interact()
    {
        if (airlockController != null)
        {
            airlockController.InteractWithAirlock(); // Trigger the airlock sequence
        }
    }
}
