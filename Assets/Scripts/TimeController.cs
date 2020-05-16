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
    public Vector3 rotation;
    public Vector3 facing;

    public FrameData(Vector3 position, Vector3 rotation, Vector3 facing)
    {
        this.position = position;
        this.rotation = rotation;
        this.facing = facing;
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
    private float rewindTime;

    private Vector3 playerCurrentPosition;
    private Vector3 playerPreviousPosition;
    private Vector3 playerCurrentRotation;
    private Vector3 playerPreviousRotation;
    private Vector3 playerCurrentFacing;
    private Vector3 playerPreviousFacing;

    private Vector3 agentCurrentPosition;
    private Vector3 agentPreviousPosition;
    private Vector3 agentCurrentRotation;
    private Vector3 agentPreviousRotation;

    private ArrayList playerKeyFrames;
    private ArrayList agentKeyFrames;

    public bool isReversing = false;
    private bool firstRun = true;

    public int keyFrame = 5;
    private int frameCounter = 0;
    private int reverseCounter = 0;

    [SerializeField] private MazeLoader mazeLoader;
    public MazeCell[,] mazeCells;

    void Start()
    {
        // Create arrays to hold the player and agent data
        playerKeyFrames = new ArrayList();
        agentKeyFrames = new ArrayList();

        // Initialize the timer
        timeLimit = 80;
        timeUsed = 0;

        rewindTime = 10;
        timeUsed = 0;

        mazeCells = new MazeCell[12, 18];
        
    }

    void Update()
    {        
        // Rewind time if the left mouse button is being pressed
        if (Input.GetMouseButton(0))
        {
            isReversing = true;
        }
        else
        {
            isReversing = false;
            firstRun = true;            
        }

        // Control the timer based on whether the player is reversing time
        if (isReversing)
        {
            // Increase the remaining time by subtracting from the time used
            timeUsed -= Time.deltaTime;
            timeLeft = (timeLimit - (int)timeUsed);

            // Update the timer label
            timeLabel.text = timeLeft.ToString();

            // Decrement the player's rewind time and update the label
            rewindTime -= Time.deltaTime;
            int time = (int)rewindTime;
            rewindTimeLabel.text = time.ToString();
        }
        else
        {
            // Reduce the remaining time by increasing the time used
            timeUsed += Time.deltaTime;
            timeLeft = (timeLimit - (int)timeUsed);

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

            // Increment the player's rewind time and update the label
            rewindTime = Mathf.Clamp((rewindTime + Time.deltaTime), 0, 10);
            int time = (int)rewindTime;
            rewindTimeLabel.text = time.ToString();
        }
        Debug.Log(rewindTime);
    }

    void FixedUpdate()
    {
        // Is time rewinding?
        if (!isReversing)
        {
            // If we're not at the 5th frame yet, increment the frame counter
            if (frameCounter < keyFrame)
            {
                frameCounter += 1;
            }
            // Otherwise, reset the frame counter and record the player's and agent's positions and rotations
            else
            {
                frameCounter = 0;
                playerKeyFrames.Add(new FrameData(player.transform.position, player.transform.localEulerAngles, camera.transform.localEulerAngles));
                agentKeyFrames.Add(new FrameData(agent.transform.position, agent.transform.localEulerAngles, agent.transform.localEulerAngles));
            }
        }
        // Time is rewinding -- move the player and agent backwards
        else
        {
            if (reverseCounter > 0)
            {
                reverseCounter -= 1;
            }
            else
            {
                reverseCounter = keyFrame;
                RestorePositions();
            }

            if (firstRun)
            {
                firstRun = false;
                RestorePositions();
            }

            // Interpolate the player's and agent's positions and rotations back to previous positions
            float interpolation = (float)reverseCounter / (float)keyFrame;
            player.transform.position = Vector3.Lerp(playerPreviousPosition, playerCurrentPosition, interpolation);
            player.transform.localEulerAngles = Vector3.Lerp(playerPreviousRotation, playerCurrentRotation, interpolation);
            camera.transform.localEulerAngles = Vector3.Lerp(playerPreviousFacing, playerCurrentFacing, interpolation);

            agent.transform.position = Vector3.Lerp(agentPreviousPosition, agentCurrentPosition, interpolation);
            agent.transform.localEulerAngles = Vector3.Lerp(agentPreviousRotation, agentCurrentRotation, interpolation);

/*            for (int r = 0; r < 12; r++)
            {
                for (int c = 0; c < 18; c++)
                {
                    if (mazeLoader.mazeCells[r, c] != mazeCells[r, c])
                    {
                        mazeLoader.mazeCells[r, c] = mazeCells[r, c];
                    }
                }
            }*/
/*            if (mazeCells != mazeLoader.mazeCells)
            {
                mazeLoader.mazeCells = mazeCells;
            }*/
        }

        // Once the array has grown to a certain size, start removing the first recorded position
        if (playerKeyFrames.Count > 64)
        {
            playerKeyFrames.RemoveAt(0);
        }

        if (agentKeyFrames.Count > 64)
        {
            agentKeyFrames.RemoveAt(0);
        }
    }

    void RestorePositions()
    {
        int playerLastIndex = playerKeyFrames.Count - 1;
        int playerSecondToLastIndex = playerKeyFrames.Count - 2;

        int agentLastIndex = agentKeyFrames.Count - 1;
        int agentSecondToLastIndex = agentKeyFrames.Count - 2;

        // Get the player's current and previous position and location
        if (playerSecondToLastIndex >= 0)
        {
            playerCurrentPosition = (playerKeyFrames[playerLastIndex] as FrameData).position;
            playerPreviousPosition = (playerKeyFrames[playerSecondToLastIndex] as FrameData).position;

            playerCurrentRotation = (playerKeyFrames[playerLastIndex] as FrameData).rotation;
            playerPreviousRotation = (playerKeyFrames[playerSecondToLastIndex] as FrameData).rotation;

            playerCurrentFacing = (playerKeyFrames[playerLastIndex] as FrameData).facing;
            playerPreviousFacing = (playerKeyFrames[playerSecondToLastIndex] as FrameData).facing;

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

        //timeLeft = Mathf.Clamp(timeLeft + 1, 0, timeLimit);
        //timeRestored++;
    }
}
