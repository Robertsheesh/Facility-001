using UnityEngine;

public class TriggerZone : MonoBehaviour
{
    public FontSwitchManager fontSwitchManager;  // Reference to the FontSwitchManager script
    public GameObject playerObject;              // Reference to the player GameObject

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == playerObject)
        {
            Debug.Log("Player entered trigger zone: " + gameObject.name);
            fontSwitchManager.PlayerEnteredControlRoom();  // Notify the FontSwitchManager
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == playerObject)
        {
            Debug.Log("Player exited trigger zone: " + gameObject.name);
            fontSwitchManager.PlayerExitedControlRoom();  // Notify the FontSwitchManager
        }
    }
}
