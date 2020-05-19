using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

public class FrameData
{
    public Vector3 position;
    public Quaternion rotation;
    public Quaternion cameraFacing;

    public FrameData(Vector3 position, Quaternion rotation, Quaternion cameraFacing)
    {
        this.position = position;
        this.rotation = rotation;
        this.cameraFacing = cameraFacing;
    }
}


public class TimeController : MonoBehaviour
{
    [SerializeField] private FirstPersonAIO player;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Camera camera;
    [SerializeField] private Text timeLabel;
    [SerializeField] private Text rewindTimeLabel;

    private int timeLimit;
    private int timeLeft;
    private float timeUsed;    

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

    public int keyFrame = 5;
    private int frameCounter = 0;
    private int rewindCounter = 0;
    private float allowedRecordTime = 10f;
    public float rewindTime;

    private LensDistortion lensDistortionLayer = null;
    private ChromaticAberration chromaticAberrationLayer = null;

    [SerializeField] private MazeLoader mazeLoader;
    public MazeCell[,] mazeCells;

    void Start()
    {
        // Create arrays to hold the player and agent data
        playerKeyFrames = new ArrayList();
        agentKeyFrames = new ArrayList();
        playerStamina = new ArrayList();

        // Initialize timer variables
        timeLimit = 80;
        timeUsed = 0;
        rewindTime = 0;

        // Initialize the array we'll store the maze data in
        mazeCells = new MazeCell[12, 18];

        // Grab the post-processing information from the camera
        PostProcessVolume volume = camera.GetComponent<PostProcessVolume>();
        volume.profile.TryGetSettings(out lensDistortionLayer);
        volume.profile.TryGetSettings(out chromaticAberrationLayer);
    }

    void Update()
    {
        // Rewind time if the left mouse button is being pressed
        if (rewindTime > 1.0f)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                // Set the reversing flag and the camera effects
                isReversing = true;
                lensDistortionLayer.intensity.value = -70.0f;
                chromaticAberrationLayer.enabled.value = true;
            }
        }        
        else
        {
            // Set the reversing flag and disable the camera effects
            isReversing = false;
            chromaticAberrationLayer.enabled.value = false;
        }

        // Update the timers
        UpdateTimers();        
    }

    void FixedUpdate()
    {
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
                timeUsed = Mathf.Clamp(timeUsed - Time.deltaTime, 0, 80);
                timeLeft = Mathf.Clamp((timeLimit - (int)timeUsed), 0, 80);

                // Update the timer label
                timeLabel.text = timeLeft.ToString();
            }            

            // Decrement the player's rewind time and update the label
            rewindTime = Mathf.Clamp((rewindTime - Time.deltaTime), 0, 10);
            rewindTimeLabel.text = ((int)rewindTime).ToString();

            // Refill the player's stamina
            if (player.staminaInternal == player.staminaLevel)
            {
                player.StaminaMeterBG.color = Vector4.MoveTowards(player.StaminaMeterBG.color, new Vector4(0, 0, 0, 0), 0.15f);
                player.StaminaMeter.color = Vector4.MoveTowards(player.StaminaMeter.color, new Vector4(1, 1, 1, 0), 0.15f);
            }
            float x = Mathf.Clamp(Mathf.SmoothDamp(player.StaminaMeter.transform.localScale.x, (player.staminaInternal / player.staminaLevel) * player.StaminaMeterBG.transform.localScale.x, ref player.smoothRef, (1) * Time.deltaTime, 1), 0.001f, player.StaminaMeterBG.transform.localScale.x);
            player.StaminaMeter.transform.localScale = new Vector3(x, 1, 1);
        }
        else
        {
            // Reduce the remaining time by increasing the time used
            timeUsed = Mathf.Clamp(timeUsed + Time.deltaTime, 0, 80);
            timeLeft = Mathf.Clamp((timeLimit - (int)timeUsed), 0, 80);

            // If there is still time remaining, display the updated value
            if (timeLeft > 0)
            {
                timeLabel.text = timeLeft.ToString();
            }
            // Time has run out
            else
            {
                timeLabel.text = "FIND THE EXIT";
            }

            rewindTime = Mathf.Clamp((rewindTime + Time.deltaTime), 0, 10);
            rewindTimeLabel.text = ((int)rewindTime).ToString();
        }
    }

    /**
     * Records both the player's and agent's positions, and trims their respective arrays when they grow too large.
     */
    private void RecordPosition()
    {
        if (Input.GetMouseButton(0)) return;

        // If the arrays have more than 10 seconds worth of information in them, remove the oldest recorded position

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

        // If we're not at the 5th frame yet, increment the frame counter
        if (frameCounter < keyFrame)
        {
            frameCounter += 1;
        }
        // Otherwise, reset the frame counter and record the player's and agent's positions and rotations
        else
        {
            frameCounter = 0;
            playerKeyFrames.Add(new FrameData(player.transform.position, player.transform.rotation, camera.transform.rotation));
            agentKeyFrames.Add(new FrameData(agent.transform.position, agent.transform.rotation, agent.transform.rotation));

            playerStamina.Add(player.staminaInternal);
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

        // If this is the first cycle of the rewind, restore the player and agent's positions
        if (firstCycle)
        {
            firstCycle = false;
            RestorePositions();
        }

        // Interpolate the player's position, rotation, camera facing, and stamina back to previous positions
        float interpolation = (float)rewindCounter / (float)keyFrame;
        player.transform.position = Vector3.Lerp(playerPreviousPosition, playerCurrentPosition, interpolation);
        player.transform.rotation = Quaternion.Lerp(playerPreviousRotation, playerCurrentRotation, interpolation);
        camera.transform.rotation = Quaternion.Lerp(playerPreviousFacing, playerCurrentFacing, interpolation);
        player.staminaInternal = Mathf.Lerp(playerPreviousStamina, playerCurrentStamina, interpolation);

        // Interpolate the agent's position and rotation back to previous positions
        agent.transform.position = Vector3.Lerp(agentPreviousPosition, agentCurrentPosition, interpolation);
        agent.transform.rotation = Quaternion.Lerp(agentPreviousRotation, agentCurrentRotation, interpolation);
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
}
