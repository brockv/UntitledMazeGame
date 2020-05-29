using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseController : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Unlock the cursor and make it visible
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Cursor.visible && Input.GetMouseButtonDown(0))
        {
            // Lock the cursor and hide it again
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
