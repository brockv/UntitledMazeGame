using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/**
 * Class used to keep track of information about the player and agent(s).
 */
public class FrameData
{
    public Vector3 position;
    public Quaternion rotation;
    public Quaternion cameraFacing;

    public FrameData(Vector3 _position, Quaternion _rotation, Quaternion _cameraFacing)
    {
        this.position = _position;
        this.rotation = _rotation;
        this.cameraFacing = _cameraFacing;
    }
}

public class TimeController : MonoBehaviour
{
    #region Editor
    
    [SerializeField] private FirstPersonAIO player;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private int timeLimit;
    [SerializeField] private Camera camera;
    [SerializeField] private Text timeLabel;
    [SerializeField] private MazeLoader mazeLoader;
    [SerializeField] private Image rewindMeter;
    [SerializeField] private Image rewindMeterBG;
    public float smoothRefRewind;

    #endregion

    #region Variables
    
    public int timeLeft;
    public float timeUsed;    

    private Vector3 playerCurrentPosition;
    private Vector3 playerPreviousPosition;    
    private Quaternion playerCurrentRotation;
    private Quaternion playerPreviousRotation;
    private Quaternion playerCurrentFacing;
    private Quaternion playerPreviousFacing;
    private float playerCurrentStamina;
    private float playerPreviousStamina;

    private Vector3 agentCurrentPosition;
    private Vector3 agentPreviousPosition;    
    private Quaternion agentCurrentRotation;
    private Quaternion agentPreviousRotation;

    private ArrayList playerKeyFrames;
    private ArrayList agentKeyFrames;
    private ArrayList playerStamina;

    public bool isReversing = false;
    private bool firstCycle = true;
    private bool isLayoutReverting = false;

    public int keyFrame = 5;
    private int frameCounter = 0;
    private int rewindCounter = 0;
    private float allowedRecordTime = 10.0f;
    public float rewindTime;

    private LensDistortion lensDistortionLayer = null;
    private ChromaticAberration chromaticAberrationLayer = null;
    private ColorGrading colorGradingLayer = null;
    
    public ArrayList mazeList;
    public ArrayList layoutList;

    public List<Vector3> fragmentList;

    #endregion

    void Start()
    {
        // Create arrays to hold the player and agent data
        playerKeyFrames = new ArrayList();
        agentKeyFrames = new ArrayList();
        playerStamina = new ArrayList();

        // Create an array to hold the maze layouts
        layoutList = new ArrayList();
        fragmentList = new List<Vector3>();

        // Initialize timer variables
        //timeLimit = 80;
        timeUsed = 0;
        rewindTime = 0;

        // We want the rewind gauage to start empty
        rewindMeter.transform.localScale = new Vector3(0, 1, 1);

        // Grab the post-processing information from the camera
        PostProcessVolume volume = camera.GetComponent<PostProcessVolume>();
        volume.profile.TryGetSettings(out lensDistortionLayer);
        volume.profile.TryGetSettings(out chromaticAberrationLayer);
        volume.profile.TryGetSettings(out colorGradingLayer);
    }

    void Update()
    {
        // Exit if the game is paused
        if (PauseMenu.isPaused) return;

        // Rewind time if the left mouse button is being pressed
        if (rewindTime > 1.0f)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                // Set the reversing flag and the camera effects
                isReversing = true;
                lensDistortionLayer.intensity.value = -70.0f;
                chromaticAberrationLayer.enabled.value = true;
                colorGradingLayer.enabled.value = true;
            }
        }        
        else
        {
            // Set the reversing flag and disable the camera effects
            isReversing = false;
            chromaticAberrationLayer.enabled.value = false;
            colorGradingLayer.enabled.value = false;
        }

        // Update the timers
        UpdateTimers();        
    }

    void FixedUpdate()
    {
        // Exit if the game is paused
        if (PauseMenu.isPaused) return;

        // Is time rewinding?
        if (isReversing)
        {            
            RewindTime();            
        }
        // Time isn't rewinding
        else
        {
            // Reduce the lens distortion and record the player and agent's positions
            lensDistortionLayer.intensity.value = Mathf.Lerp(lensDistortionLayer.intensity.value, 0.0f, 0.1f);
            RecordPosition();
        }        
    }

    /**
     * Updates both the exit and rewind timers.
     */
    private void UpdateTimers()
    {
        // Control the timer based on whether the player is reversing time
        if (isReversing)
        {
            // Only rewind the escape timer if it hasn't already reached zero
            if (timeLeft > 0)
            {
                // Increase the remaining time by subtracting from the time used
                timeUsed = Mathf.Clamp(timeUsed - Time.deltaTime, 0, timeLimit);
                timeLeft = Mathf.Clamp((timeLimit - Mathf.RoundToInt(timeUsed)), 0, timeLimit);

                // Update the timer label
                timeLabel.text = timeLeft.ToString();
            }            

            // Decrease the player's available rewind time
            rewindTime = Mathf.Clamp((rewindTime - Time.deltaTime), 0, 10);

            // Drain the rewind meter
            float rewindX = Mathf.Clamp(Mathf.SmoothDamp(
                rewindMeter.transform.localScale.x, 
                (rewindTime / allowedRecordTime) * rewindMeterBG.transform.localScale.x,
                ref smoothRefRewind,
                (1) * Time.deltaTime, 1),
                0.001f,
                rewindMeterBG.transform.localScale.x);
            rewindMeter.transform.localScale = new Vector3(rewindX, 1, 1);

            // Hide the stamina meter if it's completely refilled
            if (player.staminaInternal == player.staminaLevel)
            {
                player.StaminaMeterBG.color = Vector4.MoveTowards(player.StaminaMeterBG.color, new Vector4(0, 0, 0, 0), 0.15f);
                player.StaminaMeter.color = Vector4.MoveTowards(player.StaminaMeter.color, new Vector4(1, 1, 1, 0), 0.15f);
            }

            // Refill the player's stamina
            float staminaX = Mathf.Clamp(Mathf.SmoothDamp(
                player.StaminaMeter.transform.localScale.x,
                (player.staminaInternal / player.staminaLevel) * player.StaminaMeterBG.transform.localScale.x,
                ref player.smoothRef,
                (1) * Time.deltaTime, 1),
                0.001f,
                player.StaminaMeterBG.transform.localScale.x);
            player.StaminaMeter.transform.localScale = new Vector3(staminaX, 1, 1);            
        }
        else
        {
            // Reduce the remaining time by increasing the time used
            timeUsed = Mathf.Clamp(timeUsed + Time.deltaTime, 0, timeLimit);
            timeLeft = Mathf.Clamp((timeLimit - (int)timeUsed), 0, timeLimit);

            // If there is still time remaining, display the updated value
            if (timeLeft > 0)
            {
                timeLabel.text = timeLeft.ToString();
            }
            // Time has run out
            else
            {
                Debug.Log("GAME OVER");

                // Unlock the cursor and make it visible again
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                // Load the Game Over scene
                SceneManager.UnloadSceneAsync("Game");
                SceneManager.LoadScene("GameOver");
            }

            // Increase the player's available rewind time
            rewindTime = Mathf.Clamp((rewindTime + Time.deltaTime), 0, 10);

            // Refill the rewind gauge
            float rewindX = Mathf.Clamp(Mathf.SmoothDamp(rewindMeter.transform.localScale.x, (rewindTime / allowedRecordTime) * rewindMeterBG.transform.localScale.x, ref smoothRefRewind, (1) * Time.deltaTime, 1), 0.001f, rewindMeterBG.transform.localScale.x);
            rewindMeter.transform.localScale = new Vector3(rewindX, 1, 1);            
        }

        // Show the rewind meter if it's not full
        if (rewindTime != allowedRecordTime)
        {
            rewindMeterBG.color = Vector4.MoveTowards(rewindMeterBG.color, new Vector4(0, 0, 0, 0.5f), 0.15f);
            rewindMeter.color = Vector4.MoveTowards(rewindMeter.color, new Vector4(0, 1, 1, 1), 0.15f);
        }
        // Hide the rewind meter if it's completely refilled
        else
        {
            rewindMeterBG.color = Vector4.MoveTowards(rewindMeterBG.color, new Vector4(0, 0, 0, 0), 0.15f);
            rewindMeter.color = Vector4.MoveTowards(rewindMeter.color, new Vector4(0, 0, 0, 0), 0.15f);
        }
    }

    /**
     * Records both the player's and agent's positions, and trims their respective arrays when they grow too large.
     */
    private void RecordPosition()
    {
        // If the arrays have more than 10 seconds worth of information in them, remove the oldest recorded data

        // Player's recorded positions
        if (playerKeyFrames.Count > (allowedRecordTime / Time.fixedDeltaTime))
        {
            playerKeyFrames.RemoveAt(0);            
        }

        // Agent's recorded positions
        if (agentKeyFrames.Count > (allowedRecordTime / Time.fixedDeltaTime))
        {
            agentKeyFrames.RemoveAt(0);
        }

        // Fragment's recorded positions
        if (fragmentList.Count > (allowedRecordTime / Time.fixedDeltaTime))
        {
            fragmentList.RemoveAt(0);
        }

        // If we're not at the 5th frame yet, increment the frame counter
        if (frameCounter < keyFrame)
        {
            frameCounter += 1;
        }
        // Otherwise, reset the frame counter and record the player's and agent's positions and rotations
        else
        {
            frameCounter = 0;

            // Player
            playerKeyFrames.Add(new FrameData(player.transform.position, player.transform.rotation, camera.transform.rotation));
            playerStamina.Add(player.staminaInternal);

            // Agent
            agentKeyFrames.Add(new FrameData(agent.transform.position, agent.transform.rotation, agent.transform.rotation));
        }
    }

    /**
     * Rewinds time, moving both the player and agent(s) to previously recorded positions.
     */
    private void RewindTime()
    {
        // Decrease the counter that controls the position restoration 
        if (rewindCounter > 0)
        {
            rewindCounter -= 1;
        }
        // Restore the player and agent's positions at every frame interval
        else
        {
            rewindCounter = keyFrame;
            RestorePositions();
        }

        // If this is the first cycle of the rewind, restore both the player's and agent's positions
        if (firstCycle)
        {
            firstCycle = false;
            RestorePositions();
        }

        // Interpolate the player's position, rotation, camera facing, and stamina back to previously recorded positions
        float interpolation = (float)rewindCounter / (float)keyFrame;
        player.transform.position = Vector3.Lerp(playerPreviousPosition, playerCurrentPosition, interpolation);
        player.transform.rotation = Quaternion.Lerp(playerPreviousRotation, playerCurrentRotation, interpolation);
        camera.transform.rotation = Quaternion.Lerp(playerPreviousFacing, playerCurrentFacing, interpolation);
        player.staminaInternal = Mathf.Lerp(playerPreviousStamina, playerCurrentStamina, interpolation);

        // Interpolate the agent's position and rotation back to previous positions
        agent.transform.position = Vector3.Lerp(agentPreviousPosition, agentCurrentPosition, interpolation);
        agent.transform.rotation = Quaternion.Lerp(agentPreviousRotation, agentCurrentRotation, interpolation);

        // Reactivate fragments if the player passes over a spot they collected one at
        foreach (Vector3 position in fragmentList)
        {
            foreach (GameObject go in mazeLoader.fragmentList)
            {
                Debug.Log(Vector3.Distance(player.transform.position, position));
                if (Vector3.Distance(player.transform.position, position) <= 2.5f)
                //if (Mathf.FloorToInt(player.transform.position.x) == position.x && Mathf.FloorToInt(player.transform.position.z) == position.z)
                {
                    Debug.Log("PLAYER IS WHERE THEY COLLECTED A FRAGMENT");
                    // If the fragment isn't active, make it active and increase the fragment count
                    if (!go.activeSelf && Vector3.Distance(player.transform.position, go.transform.position) <= 2.5f)
                    {
                        go.SetActive(true);
                        mazeLoader.fragments++;
                    }
                }
            }
        }

        // If the remaining time isn't divisible by 5, reset the flag
        if (timeLeft % 5 != 0) isLayoutReverting = false;

        // If there are recorded layouts and the remaining time is on a change interval, revert the maze
        if (layoutList.Count > 0 && timeLeft % 5 == 0 && !isLayoutReverting)
        {
            // Set the flag and revert the maze
            isLayoutReverting = true;
            CreateLayoutFromCopy();

            // Update the nav mesh
            mazeLoader.mazeChanged = true;

            // Remove the layout data we just swapped in
            layoutList.RemoveAt(layoutList.Count - 1);
        }        
    }

    /**
     * Grabs the player's and agent's current and last recorded positions in order to interpolate between them.
     */
    void RestorePositions()
    {
        if (playerKeyFrames.Count <= 0) return;

        int playerLastIndex = playerKeyFrames.Count - 1;
        int playerSecondToLastIndex = playerKeyFrames.Count - 2;

        int agentLastIndex = agentKeyFrames.Count - 1;
        int agentSecondToLastIndex = agentKeyFrames.Count - 2;                

        // Get the player's current and previous position and location
        if (playerSecondToLastIndex >= 0)
        {            
            // Position
            playerCurrentPosition = (playerKeyFrames[playerLastIndex] as FrameData).position;
            playerPreviousPosition = (playerKeyFrames[playerSecondToLastIndex] as FrameData).position;

            // Rotation
            playerCurrentRotation = (playerKeyFrames[playerLastIndex] as FrameData).rotation;
            playerPreviousRotation = (playerKeyFrames[playerSecondToLastIndex] as FrameData).rotation;

            // Camera facing
            playerCurrentFacing = (playerKeyFrames[playerLastIndex] as FrameData).cameraFacing;
            playerPreviousFacing = (playerKeyFrames[playerSecondToLastIndex] as FrameData).cameraFacing;

            // Stamina
            playerCurrentStamina = (float)playerStamina[playerLastIndex];
            playerPreviousStamina = (float)playerStamina[playerSecondToLastIndex];

            playerKeyFrames.RemoveAt(playerLastIndex);                   
        }

        // Get the agent's current and previous position and location
        if (agentSecondToLastIndex >= 0)
        {
            agentCurrentPosition = (agentKeyFrames[agentLastIndex] as FrameData).position;
            agentPreviousPosition = (agentKeyFrames[agentSecondToLastIndex] as FrameData).position;

            agentCurrentRotation = (agentKeyFrames[agentLastIndex] as FrameData).rotation;
            agentPreviousRotation = (agentKeyFrames[agentSecondToLastIndex] as FrameData).rotation;

            agentKeyFrames.RemoveAt(agentLastIndex);
        }
    }

    /**
     * Reverts the maze to the most recent previous version by rebuilding the walls.
     */
    public void CreateLayoutFromCopy()
    {
        // Destroy all the walls currently inside the maze (their y-value will always be 0)
        foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
        {
            // Make sure we're only destroying the walls
            if (go.CompareTag("SectionOne") || go.CompareTag("SectionTwo") || go.CompareTag("SectionThree") || go.CompareTag("SectionFour"))
            {
                // Make sure the y-value is actually 0
                if (go.transform.position.y == 0.0f)
                {
                    Destroy(go);
                }
            }
        }

        // Grab the most recent previous version from the stored copies
        MazeCell[,] temp = layoutList[layoutList.Count - 1] as MazeCell[,];

        // Grab these here for better readability down below
        GameObject wall = mazeLoader.wall;
        float size = mazeLoader.size;

        // Iterate over the current maze in order to make a copy of it
        for (int r = 0; r < mazeLoader.mazeRows; r++)
        {
            for (int c = 0; c < mazeLoader.mazeColumns; c++)
            {
                // Initialize the current cell and determine which section it is in (so we can tag it appropriately)
                mazeLoader.mazeCells[r, c] = new MazeCell();
                mazeLoader.mazeCells[r, c].visited = true;
                string sectionTag = GetSectionTag(r, c);

                // If there's a north wall in this cell, create one in the copy
                if (temp[r, c].northWall != null)
                {
                    mazeLoader.mazeCells[r, c].northWall = Instantiate(wall, new Vector3((r * size) - (size / 2f), 0, c * size), Quaternion.identity) as GameObject;
                    mazeLoader.mazeCells[r, c].northWall.name = "North Wall " + r + "," + c;
                    mazeLoader.mazeCells[r, c].northWall.transform.Rotate(Vector3.up * 90f);
                    mazeLoader.mazeCells[r, c].northWall.tag = sectionTag;
                    
                    DestroyWallIfItExists(temp[r, c].northWall);
                    temp[r, c].northWall = null;
                }

                // If there's a south wall in this cell, create one in the copy
                if (temp[r, c].southWall != null)
                {
                    mazeLoader.mazeCells[r, c].southWall = Instantiate(wall, new Vector3((r * size) + (size / 2f), 0, c * size), Quaternion.identity) as GameObject;
                    mazeLoader.mazeCells[r, c].southWall.name = "South Wall " + r + "," + c;
                    mazeLoader.mazeCells[r, c].southWall.transform.Rotate(Vector3.up * 90f);
                    mazeLoader.mazeCells[r, c].southWall.tag = sectionTag;
                    
                    DestroyWallIfItExists(temp[r, c].southWall);
                    temp[r, c].southWall = null;
                }

                // If there's a east wall in this cell, create one in the copy
                if (temp[r, c].eastWall != null)
                {
                    mazeLoader.mazeCells[r, c].eastWall = Instantiate(wall, new Vector3(r * size, 0, (c * size) + (size / 2f)), Quaternion.identity) as GameObject;
                    mazeLoader.mazeCells[r, c].eastWall.name = "East Wall " + r + "," + c;
                    mazeLoader.mazeCells[r, c].eastWall.tag = sectionTag;
                   
                    DestroyWallIfItExists(temp[r, c].eastWall);
                    temp[r, c].eastWall = null;
                }

                // If there's a west wall in this cell, create one in the copy
                if (temp[r, c].westWall != null)
                {
                    mazeLoader.mazeCells[r, c].westWall = Instantiate(wall, new Vector3(r * size, 0, (c * size) - (size / 2f)), Quaternion.identity) as GameObject;
                    mazeLoader.mazeCells[r, c].westWall.name = "West Wall " + r + "," + c;
                    mazeLoader.mazeCells[r, c].westWall.tag = sectionTag;
                    
                    DestroyWallIfItExists(temp[r, c].westWall);
                    temp[r, c].westWall = null;
                }
            }
        }
    }

    /**
     * Destroys the given wall.
     */
    private void DestroyWallIfItExists(GameObject wall)
    {
        if (wall != null)
        {
            GameObject.Destroy(wall);
        }
    }

    /**
	 * Returns the tag associated with the given row and column.
	 */
    public string GetSectionTag(int row, int col)
    {
        string tag = "";
        if (row < 6 && col < 9) // Section One
        {
            tag = "SectionOne";
        }
        else if (row < 6 && col >= 9) // Section Two
        {
            tag = "SectionTwo";
        }
        else if (row >= 6 && col < 9) // Section Three
        {
            tag = "SectionThree";
        }
        else if (row >= 6 && col >= 9) // Section Four
        {
            tag = "SectionFour";
        }

        return tag;
    }
}
