using UnityEngine;
using UnityEngine.Audio;

public class Audio : MonoBehaviour
{
    public AudioClip[] clips;
    public AudioClip plug;
    public AudioClip hit;

    private AudioSource source;
    private float pitch_min = 0.8f;
    private float pitch_max = 1.2f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        source = GetComponent<AudioSource>();
    }

    public void Stretch()
    {
        source.pitch = Random.Range(pitch_min, pitch_max); // pitch randomnes
        int r = UnityEngine.Random.Range(0,3); // clip randomnes
        source.PlayOneShot(clips[r]);
    }
    public void Plug()

    {
        source.pitch = Random.Range(pitch_min, pitch_max); // pitch randomnes
        source.PlayOneShot(plug);
    }
    public void Hit()

    {
        source.pitch = Random.Range(pitch_min, pitch_max); // pitch randomnes
        source.PlayOneShot(hit);
    }
}
