using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowFragments : MonoBehaviour
{
    [SerializeField] private MazeLoader mazeLoader;
    private Shader defaultShader;
    private Shader outlineShader;

    // Start is called before the first frame update
    void Start()
    {
        defaultShader = Shader.Find("Standard");
        outlineShader = Shader.Find("Unlit/Outline");
}

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            ShowFragmentOutlines();
        }
        else
        {
            HideFragmentOutlines();
        }
    }

    /**
     * Shows the outlines of the fragments through the walls
     */
     private void ShowFragmentOutlines()
    {
        foreach (GameObject fragment in mazeLoader.fragmentList)
        {
            if (fragment.activeSelf)
            {
                Renderer renderer = fragment.GetComponent<Renderer>();
                renderer.material.shader = outlineShader;
            }
        }
    }

    private void HideFragmentOutlines()
    {
        foreach (GameObject fragment in mazeLoader.fragmentList)
        {
            if (fragment.activeSelf)
            {
                Renderer renderer = fragment.GetComponent<Renderer>();
                renderer.material.shader = defaultShader;
            }
        }
    }
}
