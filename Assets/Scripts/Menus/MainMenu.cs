using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    /**
     * Starts the game from the main menu by loading the next scene.
     */
    public void StartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
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
