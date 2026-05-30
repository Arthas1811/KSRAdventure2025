using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public int damage = 5;
    public float attackRange = 10;
    public LayerMask bossLayer;
    public AudioClip attackSFX;
    AudioSource audioSource;


    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void DoAttack()
    {
        audioSource.PlayOneShot(attackSFX);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.up, attackRange, bossLayer);
        if (hit.collider != null)
        {
            BossHealth bh = hit.collider.GetComponentInParent<BossHealth>();
            if (bh != null)
            {
                bh.TakeDamage(damage);
            }
        }
    }

}
