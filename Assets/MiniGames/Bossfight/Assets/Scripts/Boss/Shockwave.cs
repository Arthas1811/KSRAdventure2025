using UnityEngine;

public class Shockwave : MonoBehaviour
{
    public float speed = 11f;
    public float lifetime = 5f;
    public int damage = 2;


    void Start()
    {
        Destroy(gameObject, lifetime);
    }


    void Update()
    {
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<PlayerHealth>().TakeDamage(damage);
        }
    }
}
