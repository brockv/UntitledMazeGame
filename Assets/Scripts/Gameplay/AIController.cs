using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.ThirdPerson;

public class AIController : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private ThirdPersonCharacter character;

    private TimeController timeController;

    private void Start()
    {
        // Grab both the time controller and player from the scene
        timeController = FindObjectOfType(typeof(TimeController)) as TimeController;
        player = GameObject.FindGameObjectWithTag("Player");

        // Disable rotation -- the ThirdPersonCharacter script will handle it
        agent.updateRotation = false;
    }

    void Update()
    {
        // Exit if the game is paused
        if (PauseMenu.isPaused) return;

        // Only proceed if the player exists in the scene
        if (player != null)
        {
            // Only move the agent if the player isn't reversing time
            if (!timeController.isReversing)
            {
                // Move
                MoveAgent();

                // Drain the player's remaining time
                DrainPlayersRemainingTime();
            }
        }
    }

    /**
     * Move the agent towards the player.
     */
    private void MoveAgent()
    {
        // Get the player's current location and set that as the agent's new destination
        Vector3 newDestination = player.transform.position;
        agent.SetDestination(newDestination);

        // Move the agent using the ThirdPersonCharacter script
        if (agent.remainingDistance > agent.stoppingDistance)
        {
            character.Move(agent.desiredVelocity, false, false);            
        }
        else
        {
            character.Move(Vector3.zero, false, false);
        }
    }

    /**
     * Drains the player's remaining time if the agent gets close enough
     */
    private void DrainPlayersRemainingTime()
    {
        // If the agent is close enough to the player, drain their remaining time
        if (Vector3.Distance(agent.transform.position, player.transform.position) <= 2.5f)
        {
            // Adding to the time used decreases the time the player has left
            timeController.timeUsed += 1.0f;
        }
    }
}
