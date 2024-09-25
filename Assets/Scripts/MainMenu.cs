using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // This method is called when Start Game button is clicked
    public void StartGame()
    {
        SceneManager.LoadScene("SampleScene");  // Replace with the actual name of your game scene
    }

    // This method is called when Quit button is clicked
    public void QuitGame()
    {
        Debug.Log("Quit!");
        Application.Quit();
    }
}
