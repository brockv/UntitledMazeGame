using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;

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

    private void Start()
    {
        // Add the tracks and initialize their settings
        SoundManager.AddTracks(2, gameObject);
        SoundManager.TrackSettings(0, mainMix, "Safe", 0.5f, 0.5f, true);
        SoundManager.TrackSettings(1, mainMix, "Danger", 0.5f, 0.5f, true);

        // Fade in and start playing music
        SoundManager.PlayMusic(0, safe);
        SoundManager.SetFadeIn(0, 0.01f, SoundManager.trackList[0].maxVolume);

        //SoundManager.PlayMusic(1, chaseStart);
        //SoundManager.SetFadeOut(1, 1.0f);
    }

    private void Update()
    {
        Debug.Log("DISTANCE: " + Vector3.Distance(agent.transform.position, player.transform.position));

        // Calm zone
        if (Vector3.Distance(agent.transform.position, player.transform.position) >= safeDistance && !inSafeZone)
        {
            Debug.Log("In calm zone...");
            inSafeZone = true;
            inDangerZone = false;
            
            SoundManager.PlayMusic(0, safe);
            SoundManager.SetFadeIn(0, 0.01f, SoundManager.trackList[0].maxVolume);

            SoundManager.SetFadeOut(1, 0.01f);
        }

        // Danger zone
        if (Vector3.Distance(agent.transform.position, player.transform.position) <= dangerDistance && !inDangerZone)
        {
            Debug.Log("In danger zone...");
            inSafeZone = false;
            inDangerZone = true;

            SoundManager.PlayMusic(1, chaseStart);
            SoundManager.SetFadeOut(0, 0.01f);
        }
    }
}

