using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    public bool isMenuActive { get; private set; }

    void Awake()
    {
        // Ensure only one instance of GameStateManager exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // This will persist the manager across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetMenuActive(bool active)
    {
        isMenuActive = active;
        // Show or hide the cursor based on the menu state
        if (active)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
