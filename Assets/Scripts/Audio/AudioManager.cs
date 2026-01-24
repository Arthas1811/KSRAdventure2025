using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource musicSource;
    public AudioSource sfxSource;

    public float defaultMusicVolume = 0.5f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            var sources = GetComponentsInChildren<AudioSource>();
            musicSource = sources[0];
            sfxSource = sources[1];
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

    IEnumerator FadeOutMusic(float fadeTime)
    {
        float startVolume = musicSource.volume;

        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0, t / fadeTime);
            yield return null;
        }

        musicSource.Stop();
        musicSource.clip = null;
        musicSource.volume = defaultMusicVolume;
    }


    IEnumerator FadeMusic(AudioClip newClip, float fadeTime)
    {
        float startVolume = defaultMusicVolume;

        // Fade out old track
        float oldVolume = musicSource.volume;
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(oldVolume, 0, t / fadeTime);
            yield return null;
        }

        musicSource.clip = newClip;
        musicSource.Play();

        // Fade in new track
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(0, startVolume, t / fadeTime);
            yield return null;
        }

    }

    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }
}
