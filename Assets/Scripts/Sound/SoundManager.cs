using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public List<Track> trackList = new List<Track>();
    private SoundManager instance;

    private bool keepFadingIn;
    private bool keepFadingOut;

    private void Awake()
    {
        // Create an instance of the SoundManager for the coroutines
        instance = this;
    }

    /**
     * Adds an AudioSource to the game object for each track present.
     */
    public void AddTracks(int numberOfTracks, GameObject gameObject)
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
    public void TrackSettings(int track, AudioMixer mainMix, string audioGroup, float volume, float maxVolume, bool loop = false)
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
    public void PlayMusic(int track, AudioClip audioClip)
    {
        // Grab the audio source for this track
        AudioSource source = trackList[track].audioSource;

        // Set the clip, volume, and loop properties, then play it
        source.clip = audioClip;
        source.volume = trackList[track].volume;
        source.loop = true;
        source.Play();
    }

    /**
     * Starts the coroutine that fades a track in.
     */
    public void SetFadeIn(int track, float volume)
    {
        instance.StartCoroutine(FadeIn(track, volume));
    }

    /**
     * Starts the coroutine that fades a track out.
     */
    public void SetFadeOut(int track, float volume)
    {
        instance.StartCoroutine(FadeOut(track, volume));
    }

    /**
     * Fades a track in.
     */
    private IEnumerator FadeIn(int track, float volume)
    {
        // Fade the track in
        while (trackList[track].audioSource.volume < volume)
        {
            trackList[track].audioSource.volume += (Time.deltaTime / (5 + 1));
            yield return null;
        }
    }

    /**
     * Fades a track out.
     */
    private IEnumerator FadeOut(int track, float volume)
    {
        // Fade the track out
        while (trackList[track].audioSource.volume > volume)
        {
            trackList[track].audioSource.volume -= (Time.deltaTime / (5 - 1));
            yield return null;
        }
    }
}
