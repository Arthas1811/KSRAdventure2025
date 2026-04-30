using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioClip rotateSound;
    [SerializeField] private AudioClip moveSound;

    private AudioSource musicSource;
    private AudioSource soundEffectSource;

    void Awake()
    {
        musicSource = gameObject.AddComponent<AudioSource>();
        soundEffectSource = gameObject.AddComponent<AudioSource>();

        musicSource.clip = backgroundMusic;
        musicSource.loop = true;
        musicSource.volume = 0.5f;
    }
    void Start()
    {
        PlayMusic();
    }

    public void PlayMusic()
    {
        musicSource.Play();
    }
    public void StopMusic()
    {
        musicSource.Play();
    }

    public void PlayRotateSound()
    {
        soundEffectSource.PlayOneShot(rotateSound);
    }

    public void PlayMoveSound()
    {
        soundEffectSource.PlayOneShot(moveSound);
    }
}
