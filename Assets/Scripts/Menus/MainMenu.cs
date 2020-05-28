using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    /**
     * Starts the game from the main menu by loading the next scene.
     */
    public void StartGame()
    {
        Debug.Log("GAME STARTED");
        SceneManager.UnloadSceneAsync("MainMenu");
        SceneManager.LoadScene("Game");
    }

    /**
     * Exits the game from the main menu.
     */
    public void QuitGame()
    {
        Debug.Log("QUIT!");
        Application.Quit();
    }
}
