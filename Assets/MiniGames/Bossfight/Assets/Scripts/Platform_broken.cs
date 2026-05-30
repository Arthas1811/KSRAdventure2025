using UnityEngine;

public class BreakablePlatform : MonoBehaviour
{
    public float breakDelay = 0.5f;
    public float respawnTime = 3f;
    public Sprite intactSprite;
    public Sprite brokenSprite;

    Rigidbody2D rb;
    BoxCollider2D col;
    SpriteRenderer sr;

    Vector3 startPos;
    Quaternion startRot;

    bool isBreaking = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        sr = GetComponent<SpriteRenderer>();

        startPos = transform.position;
        startRot = transform.rotation;
        rb.bodyType = RigidbodyType2D.Static;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player") && !isBreaking)
        {
            foreach(ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y < -0.5f)
                {
                    isBreaking = true;
                    Invoke(nameof(Break), breakDelay);
                    break;
                }
            }
            
        }
    }

    void Break()
    {
        if (brokenSprite != null)
            sr.sprite = brokenSprite;

        rb.bodyType = RigidbodyType2D.Dynamic;
        Invoke(nameof(Disappear), 0.5f);
    }

    void Disappear()
    {
        sr.enabled = false;
        col.enabled = false;
        Invoke(nameof(Respawn), respawnTime);
    }

    void Respawn()
    {
        transform.position = startPos;
        transform.rotation = startRot;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Static;

        col.enabled = true;
        sr.enabled = true;

        sr.sprite = intactSprite;
        isBreaking = false;
    }
}
