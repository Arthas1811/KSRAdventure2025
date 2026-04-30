using UnityEngine;

// Plays a sound effect once upon impact, then removes itself to prevent audio spam
public class CollisionSound : MonoBehaviour
{
    public AudioClip Clip;

    private void OnCollisionEnter(Collision collision)
    {
        if (Clip != null)
            AudioManager.Instance.PlaySFX(Clip);

        Destroy(this);
    }
}