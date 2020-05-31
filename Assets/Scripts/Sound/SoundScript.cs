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
    [SerializeField] private AudioClip danger;
    [SerializeField] private AudioMixer mainMix;
    [SerializeField] private SoundManager soundManager;

    public bool inSafeZone = true;
    public bool inDangerZone = false;    

    private void Start()
    {
        // Add the tracks and initialize their settings
        soundManager.AddTracks(2, gameObject);
        soundManager.TrackSettings(0, mainMix, "Safe", 0.0f, 1.0f, true);
        soundManager.TrackSettings(1, mainMix, "Danger", 0.0f, 1.0f, true);

        // Fade in and start playing the regular music
        soundManager.PlayMusic(0, safe);
        soundManager.PlayMusic(1, danger);

        soundManager.SetFadeIn(0, 0.25f);
    }

    private void Update()
    {
        // Check if the player is in the safe zone
        if (Vector3.Distance(agent.transform.position, player.transform.position) >= safeDistance && !inSafeZone)
        {
            Debug.Log("In calm zone...");

            // Set the zone flags
            inSafeZone = true;
            inDangerZone = false;

            // Fade out the chase music
            soundManager.SetFadeOut(1, 0.0f);
        }

        // Check if the player is in the danger zone
        if (Vector3.Distance(agent.transform.position, player.transform.position) <= dangerDistance && !inDangerZone)
        {
            Debug.Log("In danger zone...");

            // Set the zone flags
            inSafeZone = false;
            inDangerZone = true;

            // Fade in the chase music
            soundManager.SetFadeIn(1, 0.25f);
        }
    }
}

