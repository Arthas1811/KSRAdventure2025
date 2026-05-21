using UnityEngine;

public class DrillSFX : MonoBehaviour // audioSourceController for the drill sfx
{
    public AudioSource sfxSource;
    public AudioClip drillSFX;

    public void SFXStart()
    {
        sfxSource.Stop();
        sfxSource.clip = drillSFX;
        sfxSource.loop = true;
        sfxSource.Play();
    }

    public void SFXEnd()
    {
        sfxSource.Stop();
    }
}
