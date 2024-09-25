using UnityEngine;
using System.Collections;

public class PressureValve : MonoBehaviour
{
    public LeverManager leverManager;  // Reference to the LeverManager
    public Animator valveAnimator;     // Reference to the Animator for valve animations
    public float closeDelay = 1f;      // Delay before the valve closes after opening
    private bool isOpen = false;       // Boolean to track if the valve is open

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && Input.GetKeyDown(KeyCode.E))
        {
            if (!isOpen)  // If the valve is not open, allow interaction
            {
                // Release pressure when the player interacts
                Debug.Log("Player released pressure by interacting with the valve.");
                leverManager.ReleasePressure();  // Call the method in LeverManager
                OpenValve();  // Play the Valve_Open animation
            }
        }
    }

    // Method to open the valve (play the Valve_Open animation)
    void OpenValve()
    {
        Debug.Log("Opening Valve...");
        isOpen = true;  // Set the valve to open
        valveAnimator.SetBool("IsOpen", true);  // Set the IsOpen boolean in the animator to true
        StartCoroutine(CloseValveAfterDelay());  // Start the coroutine to close the valve after a delay
    }

    // Coroutine to wait for the delay and then close the valve
    IEnumerator CloseValveAfterDelay()
    {
        yield return new WaitForSeconds(closeDelay);  // Wait for 1 second (or the specified delay)
        CloseValve();
    }

    // Method to close the valve (play the Valve_Close animation)
    void CloseValve()
    {
        Debug.Log("Closing Valve...");
        isOpen = false;  // Set the valve to closed
        valveAnimator.SetBool("IsOpen", false);  // Set the IsOpen boolean in the animator to false
    }
}
