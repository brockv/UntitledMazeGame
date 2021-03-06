﻿using UnityEngine;

public class FragmentController : MonoBehaviour
{
    [SerializeField] Vector3 rotation;

    private MazeLoader mazeLoader;
    private TimeController timeController;

    private void Awake()
    {
        // Grab the MazeLoader and TimeController from the scene
        mazeLoader = FindObjectOfType(typeof(MazeLoader)) as MazeLoader;
        timeController = FindObjectOfType(typeof(TimeController)) as TimeController;
    }

    private void Update()
    {
        // Exit if the game is paused
        if (PauseMenu.isPaused) return;

        // Rotate the fragment
        gameObject.transform.Rotate(rotation * Time.deltaTime);

        // Disable the trigger if time is reversing
        if (timeController.isReversing)
        {
            gameObject.GetComponent<BoxCollider>().isTrigger = false;
        }
        // Otherwise, enable it
        else
        {
            gameObject.GetComponent<BoxCollider>().isTrigger = true;            
        }     
    }

    /**
     * "Collect" a fragment when moving over it.
     */
    private void OnTriggerEnter(Collider other)
    {
        // Exit if time is reversing
        if (timeController.isReversing) return;

        // Check if the player is what collided with the fragment
        if (other.CompareTag("Player"))
        {
            // Grab the player's position on pick-up
            Vector3 position = other.transform.position;
            position = new Vector3(Mathf.FloorToInt(position.x), position.y, Mathf.FloorToInt(position.z));

            // Deactivate the fragment and decrease the fragment counter
            gameObject.SetActive(false);
            mazeLoader.fragments--;

            // Add the location this was collected at to the list
            timeController.fragmentList.Add(position);
        }
    }
}