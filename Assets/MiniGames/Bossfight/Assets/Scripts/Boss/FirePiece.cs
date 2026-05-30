using System.Collections;
using UnityEngine;

public class FirePiece : MonoBehaviour
{
    public int damage = 2;
    Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        StartCoroutine(EnableGravity());
    }

    IEnumerator EnableGravity()
    {
        yield return new WaitForSeconds(0.1f);
        rb.gravityScale = 3f;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<PlayerHealth>().TakeDamage(damage);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)

    {
        if (collision.collider.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
