using UnityEngine;
using UnityEngine.SceneManagement;

public class SuccessMenu : MonoBehaviour
{
    /**
     * Starts the game over.
     */
    public void RestartGame()
    {
        // Unlock the cursor and hide it again
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.UnloadSceneAsync("SuccessMenu");
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
