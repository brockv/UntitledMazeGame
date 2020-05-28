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
        instance = this;
    }

    /**
     * Adds an AudioSource to the game object for each track present.
     */
    static public void AddTracks(int numberOfTracks, GameObject gameObject)
    {
        if (numberOfTracks > 0)
        {
            for (int i = 0; i < numberOfTracks; i++)
            {
                //AudioSource source = gameObject.AddComponent<AudioSource>();
                //source.loop = true;

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
        AudioSource source = trackList[track].audioSource; //.PlayOneShot(audioClip, trackList[track].volume);
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
        keepFadingIn = true;
        keepFadingOut = false;

        trackList[track].audioSource.volume = 0;
        float audioVolume = trackList[track].audioSource.volume;

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
        keepFadingIn = false;
        keepFadingOut = true;

        float audioVolume = trackList[track].maxVolume;

        while (trackList[track].audioSource.volume >= speed && keepFadingOut)
        {
            audioVolume -= speed;
            trackList[track].audioSource.volume = audioVolume;
            yield return new WaitForSeconds(0.1f);
        }

        trackList[track].audioSource.Stop();
    }
}
