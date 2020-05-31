using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    /**
     * Starts the game.
     */
    public void StartGame()
    {
        Debug.Log("GAME STARTED");
        SceneManager.UnloadSceneAsync("MainMenu");
        SceneManager.LoadScene("Game");
    }

    /**
     * Exits the game.
     */
    public void QuitGame()
    {
        Debug.Log("QUIT!");
        Application.Quit();
    }
}
