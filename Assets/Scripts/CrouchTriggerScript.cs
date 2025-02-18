using UnityEngine;

public class CrouchTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        SC_FPSController player = other.GetComponent<SC_FPSController>();

        if (player != null)
        {
            Debug.Log("Player entered crouch trigger.");
            player.ForceCrouch(true); // Force crouch when entering
        }
    }

    private void OnTriggerExit(Collider other)
    {
        SC_FPSController player = other.GetComponent<SC_FPSController>();

        if (player != null)
        {
            Debug.Log("Player exited crouch trigger.");
            player.ForceCrouch(false); // Stand up when exiting
        }
    }
}
