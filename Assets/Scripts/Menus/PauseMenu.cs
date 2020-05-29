using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuUI;
    public static bool isPaused = false;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    private void PauseGame()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0.0f;
        isPaused = true;

        // Unlock the cursor and make it visible again
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1.0f;
        isPaused = false;

        // Lock the cursor and hide it again
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1.0f;
        isPaused = false;

        SceneManager.UnloadSceneAsync("Game");
        SceneManager.LoadScene("MainMenu");
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
