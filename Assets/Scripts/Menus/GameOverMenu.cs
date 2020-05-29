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

        // Unlock the cursor and hide it again
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

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
