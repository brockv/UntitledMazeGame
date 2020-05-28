using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverMenu : MonoBehaviour
{
    /**
     * Starts the game from the game over menu by loading the previous scene.
     */
    public void RestartGame()
    {
        Debug.Log("GAME RESTARTED");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    /**
     * Exits the game from the game over menu.
     */
    public void QuitGame()
    {
        Debug.Log("QUIT!");
        Application.Quit();
    }
}
