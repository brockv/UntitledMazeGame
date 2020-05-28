using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public static List<Track> trackList = new List<Track>();
    public static SoundManager instance;

    private static bool keepFadingIn;
    private static bool keepFadingOut;

    private void Awake()
    {
        // Create an instance of the SoundManager for the coroutines
        instance = this;
    }

    /**
     * Adds an AudioSource to the game object for each track present.
     */
    static public void AddTracks(int numberOfTracks, GameObject gameObject)
    {
        if (numberOfTracks > 0)
        {
            // Add each audio source to the game object and store it in the list
            for (int i = 0; i < numberOfTracks; i++)
            {
                Track track = new Track { id = i, audioSource = gameObject.AddComponent<AudioSource>() };
                trackList.Add(track);
            }
        }
    }

    /**
     * Initializes the settings for a given track.
     */
    static public void TrackSettings(int track, AudioMixer mainMix, string audioGroup, float volume, float maxVolume, bool loop = false)
    {
        // Set the properties for this track
        trackList[track].audioSource.outputAudioMixerGroup = mainMix.FindMatchingGroups(audioGroup)[0];
        trackList[track].volume = volume;
        trackList[track].maxVolume = maxVolume;
        trackList[track].loop = loop;
    }

    /**
     * Plays the audio clip at the given track id.
     */
    static public void PlayMusic(int track, AudioClip audioClip)
    {
        // Grab the audio source for this track
        AudioSource source = trackList[track].audioSource;

        // Set the clip, volume, and loop properties, then play it
        source.clip = audioClip;
        source.volume = trackList[track].maxVolume;
        source.loop = true;
        source.Play();
    }

    /**
     * Starts the coroutine that fades a track in.
     */
    public static void SetFadeIn(int track, float speed, float maxVolume)
    {
        instance.StartCoroutine(FadeIn(track, speed, maxVolume));
    }

    /**
     * Starts the coroutine that fades a track out.
     */
    public static void SetFadeOut(int track, float speed)
    {
        instance.StartCoroutine(FadeOut(track, speed));
    }

    /**
     * Fades a track in.
     */
    static IEnumerator FadeIn(int track, float speed, float maxVolume)
    {
        // Set the fade flags
        keepFadingIn = true;
        keepFadingOut = false;

        // Set the volume to zero and store it in a variable
        trackList[track].audioSource.volume = 0;
        float audioVolume = trackList[track].audioSource.volume;

        // Fade the track in
        while (trackList[track].audioSource.volume < maxVolume && keepFadingIn)
        {
            audioVolume += speed;
            trackList[track].audioSource.volume = audioVolume;
            yield return new WaitForSeconds(0.1f);
        }
    }

    /**
     * Fades a track out.
     */
    static IEnumerator FadeOut(int track, float speed)
    {
        // Set the fade flags
        keepFadingIn = false;
        keepFadingOut = true;

        // Grab the current volume of the track
        float audioVolume = trackList[track].maxVolume;

        // Fade the track out
        while (trackList[track].audioSource.volume >= speed && keepFadingOut)
        {
            audioVolume -= speed;
            trackList[track].audioSource.volume = audioVolume;
            yield return new WaitForSeconds(0.1f);
        }

        // Stop it once the fade is complete -- prevents some weird audio issues
        trackList[track].audioSource.Stop();
    }
}
