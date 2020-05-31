using UnityEngine;

public class ShowFragments : MonoBehaviour
{
    [SerializeField] private MazeLoader mazeLoader;
    [SerializeField] private FirstPersonAIO player;

    private Shader defaultShader;
    private Shader outlineShader;

    void Start()
    {
        // Grab the shaders we'll be using on the fragments
        defaultShader = Shader.Find("Standard");
        outlineShader = Shader.Find("Unlit/Outline");
    }

    void Update()
    {
        // Show the fragments through the walls when the player presses the mouse button
        if (Input.GetMouseButton(0))
        {
            ShowFragmentOutlines();
        }
        // Hide them by default
        else
        {
            HideFragmentOutlines();
        }
    }

    /**
     * Shows the outlines of the fragments through the walls.
     */
     private void ShowFragmentOutlines()
     {
        // Iterate over each fragment
        foreach (GameObject fragment in mazeLoader.fragmentList)
        {
            // Grab the renderer so we can change the shader
            Renderer renderer = fragment.GetComponent<Renderer>();

            // We only need to worry about active fragments
            if (fragment.activeSelf)
            {
                // Only apply the outline shader if the player isn't in line of sight of the fragment
                if (Physics.Linecast(fragment.transform.position, player.transform.position, out RaycastHit hit))
                {
                    // Player is in line of sight -- set the default shader
                    if (hit.transform.CompareTag("Player"))
                    {
                        renderer.material.shader = defaultShader;
                    }
                    // Not in line of sight -- set the outline shader
                    else
                    {
                        renderer.material.shader = outlineShader;
                    }
                }
            }
        }        
     }

    /**
     * "Hides" the fragments by setting their default shader.
     */
    private void HideFragmentOutlines()
    {
        // Iterate over each fragment and set the default shader
        foreach (GameObject fragment in mazeLoader.fragmentList)
        {
            // We only need to worry about active fragments
            if (fragment.activeSelf)
            {
                // Grab the renderer and change the shader
                Renderer renderer = fragment.GetComponent<Renderer>();
                renderer.material.shader = defaultShader;
            }
        }
    }
}
