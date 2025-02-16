using UnityEngine;

public class InspectableObject : MonoBehaviour, IInteractable
{
    public string itemName;
    public string description;

    public void Interact()
    {
        if (InspectManager.Instance != null)
        {
            InspectManager.Instance.InspectItem(this);
        }
    }
}
