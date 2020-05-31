using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuUI;

    public static bool isPaused = false;

    void Update()
    {
        // Check if the 'Escape' key has been pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Unpause the game if it was already paused
            if (isPaused)
            {
                ResumeGame();
            }
            // Otherwise, pause it
            else
            {
                PauseGame();
            }
        }
    }

    /**
     * Pauses the game.
     */
    private void PauseGame()
    {
        // Activate the pause menu, set the time scale to 0, and set the pause flag to true
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0.0f;
        isPaused = true;

        // Unlock the cursor and make it visible again
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /**
     * Unpauses the game.
     */
    public void ResumeGame()
    {
        // Deactivate the pause menu, set the time scale back to 1, and set the pause flag to false
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1.0f;
        isPaused = false;

        // Lock the cursor and hide it again
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /**
     * Returns to the main menu.
     */
    public void ReturnToMenu()
    {
        // Set the time scale back to 1 and set the pause flag to false
        Time.timeScale = 1.0f;
        isPaused = false;

        SceneManager.UnloadSceneAsync("Game");
        SceneManager.LoadScene("MainMenu");
    }

    /**
     * Exits the game.
     */
    public void QuitGame()
    {
        Application.Quit();
    }
}
