using System.Collections;
using System.Collections.Generic;
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
        // Only proceed if the player exists in the scene
        if (player != null)
        {
            // Only move the agent if the player isn't reversing time
            if (!timeController.isReversing)
            {
                MoveAgent();
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
}
