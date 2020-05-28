using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FragmentController : MonoBehaviour
{
    [SerializeField] Vector3 rotation;

    private FirstPersonAIO player;
    private MazeLoader mazeLoader;
    private TimeController timeController;

    private void Awake()
    {
        // Grab the MazeLoader and TimeController from the scene
        mazeLoader = FindObjectOfType(typeof(MazeLoader)) as MazeLoader;
        timeController = FindObjectOfType(typeof(TimeController)) as TimeController;

        // Grab the player from the MazeLoader
        player = mazeLoader.player;
    }

    private void Update()
    {
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

    private void OnTriggerEnter(Collider other)
    {
        // Exit if time is reversing
        if (timeController.isReversing) return;

        // Check if the player is what collided with the fragment
        if (other.CompareTag("Player"))
        {
            Vector3 fragmentPosition = gameObject.transform.position;
            fragmentPosition = new Vector3(fragmentPosition.x, other.transform.position.y, fragmentPosition.z);
            Debug.Log(fragmentPosition);

            Vector3 position = other.transform.position;
            Debug.Log(position);
            position = new Vector3(Mathf.FloorToInt(position.x), position.y, Mathf.FloorToInt(position.z));

            // Deactivate the fragment and decrease the fragment counter
            gameObject.SetActive(false);
            mazeLoader.fragments--;

            // Add the location this was collected to the list
            timeController.fragmentList.Add(position);

            // Increase the player's remaining time by subtracting from their time used so far.
            timeController.timeUsed = Mathf.Clamp(timeController.timeUsed - 10.0f, 0, 80);
        }
    }
}