using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class SoundScript : MonoBehaviour
{
    [SerializeField] private GameObject gameObject;
    [SerializeField] private FirstPersonAIO player;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] float safeDistance;
    [SerializeField] float dangerDistance;
    [SerializeField] private AudioClip safe;
    [SerializeField] private AudioClip chaseStart;
    [SerializeField] private AudioClip chaseOngoing;
    [SerializeField] private AudioClip chaseEnd;
    [SerializeField] private AudioMixer mainMix;

    private bool inSafeZone = true;
    private bool inDangerZone = false;

    [SerializeField] private SoundManager soundManager;

    private void Start()
    {
        // Add the tracks and initialize their settings
        soundManager.AddTracks(2, gameObject);
        soundManager.TrackSettings(0, mainMix, "Safe", 0.5f, 0.5f, true);
        soundManager.TrackSettings(1, mainMix, "Danger", 0.5f, 0.5f, true);

        // Fade in and start playing the regular music
        soundManager.PlayMusic(0, safe);
        soundManager.SetFadeIn(0, 0.01f, soundManager.trackList[0].maxVolume);
    }

    private void Update()
    {
        //Debug.Log("DISTANCE: " + Vector3.Distance(agent.transform.position, player.transform.position));

        // Check if the player is in the safe zone
        if (Vector3.Distance(agent.transform.position, player.transform.position) >= safeDistance && !inSafeZone)
        {
            Debug.Log("In calm zone...");

            // Set the zone flags
            inSafeZone = true;
            inDangerZone = false;

            // Fade the regular music in and fade out the chase music
            soundManager.SetFadeOut(1, 0.1f);
            soundManager.PlayMusic(0, safe);
        }

        // Check if the player is in the danger zone
        if (Vector3.Distance(agent.transform.position, player.transform.position) <= dangerDistance && !inDangerZone)
        {
            Debug.Log("In danger zone...");

            // Set the zone flags
            inSafeZone = false;
            inDangerZone = true;

            // Play the chase music and fade out the regular music
            soundManager.SetFadeOut(0, 0.1f);
            soundManager.PlayMusic(1, chaseStart);
        }
    }
}

