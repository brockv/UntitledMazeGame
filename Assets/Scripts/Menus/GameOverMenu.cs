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

        // Lock the cursor and hide it again
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SceneManager.UnloadSceneAsync("GameOver");
        SceneManager.LoadScene("Game");
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
