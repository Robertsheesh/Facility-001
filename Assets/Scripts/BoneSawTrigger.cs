using UnityEngine;

public class SawingTriggerScript : MonoBehaviour, IInteractable
{
    private void OnTriggerEnter(Collider other)
    {
        BoneSawScript sawScript = other.GetComponent<BoneSawScript>();

        if (sawScript != null)
        {
            Debug.Log("Player is in the sawing area.");
        }
    }

    public void Interact()
    {
        BoneSawScript sawScript = FindObjectOfType<BoneSawScript>();

        if (sawScript != null && !sawScript.isAtTarget)
        {
            sawScript.MoveToSawingPoint();
            Debug.Log("Sawing trigger interacted, moving saw to sawing point.");
        }
    }
}
