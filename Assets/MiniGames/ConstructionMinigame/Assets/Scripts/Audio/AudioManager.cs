// Script taken from KSRAdventure2025

using UnityEngine;
using System.Collections;

// A persistent audio manager that handles background music crossfading and one-shot sound effects
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource MusicSource;
    public AudioSource SfxSource;

    public float DefaultMusicVolume = 0.5f;

    void Awake()
    {
        // Enforce singleton pattern so audio continues seamlessly across scene loads
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            var sources = GetComponentsInChildren<AudioSource>();
            MusicSource = sources[0];
            SfxSource = sources[1];
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayMusic(AudioClip clip, float fadeTime = 1f)
    {
        if (clip == null)
        {
            StartCoroutine(FadeOutMusic(fadeTime));
            return;
        }

        StartCoroutine(FadeMusic(clip, fadeTime));
    }

    // Gradually lowers the volume to 0 and stops the track
    IEnumerator FadeOutMusic(float fadeTime)
    {
        float startVolume = MusicSource.volume;

        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            MusicSource.volume = Mathf.Lerp(startVolume, 0, t / fadeTime);
            yield return null;
        }

        MusicSource.Stop();
        MusicSource.clip = null;
        MusicSource.volume = DefaultMusicVolume;
    }

    // Crossfade logic for smooth music transitions
    IEnumerator FadeMusic(AudioClip newClip, float fadeTime)
    {
        float startVolume = DefaultMusicVolume;

        // Fade out old track
        float oldVolume = MusicSource.volume;
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            MusicSource.volume = Mathf.Lerp(oldVolume, 0, t / fadeTime);
            yield return null;
        }

        MusicSource.clip = newClip;
        MusicSource.Play();

        // Fade in new track
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            MusicSource.volume = Mathf.Lerp(0, startVolume, t / fadeTime);
            yield return null;
        }

    }

    // Play a sound effect once 
    public void PlaySFX(AudioClip clip)
    {
        SfxSource.PlayOneShot(clip);
    }
}