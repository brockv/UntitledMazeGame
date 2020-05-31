using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverMenu : MonoBehaviour
{
    /**
     * Starts the game over.
     */
    public void RestartGame()
    {
        // Unlock the cursor and hide it again
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.UnloadSceneAsync("GameOver");
        SceneManager.LoadScene("Game");
    }

    /**
     * Exits the game.
     */
    public void QuitGame()
    {
        Application.Quit();
    }
}
