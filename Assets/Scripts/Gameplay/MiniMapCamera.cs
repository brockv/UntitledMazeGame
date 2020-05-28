using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapCamera : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private bool rotateWithPlayer = false;

    void Start()
    {
        SetPosition();
        SetRotation();
    }

    void LateUpdate()
    {
        // If the player exists in the scene, set the camera to follow them
        if (player != null)
        {
            // Follow the player
            SetPosition();

            // If rotate is enabled, rotate the map with the player
            if (rotateWithPlayer && mainCamera)
            {
                SetRotation();
            }
        }
    }

    /**
     * Rotate the map with the player.
     */
    private void SetRotation()
    {
        transform.rotation = Quaternion.Euler(90.0f, mainCamera.transform.eulerAngles.y, 0.0f);
    }

    /**
     * Move the camera with the player, keeping them in the center when possible.
     */
    private void SetPosition()
    {      
        // Make the camera follow the player, but keep it clamped to the maze boundaries
        transform.position = new Vector3(
           Mathf.Clamp(player.position.x, 6.5f, 59.5f),
           transform.position.y,
           Mathf.Clamp(player.position.z, 6.5f, 95.5f));
    }
}
