using UnityEngine;

public class Robot : MonoBehaviour
{
    public float speed = 2.5f;
    public int damage = 1;
    public float lifeTime = 7f;

    Transform player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        Destroy(gameObject, lifeTime);
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 dir = (player.position - transform.position).normalized;
        transform.position += (Vector3)dir * speed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<PlayerHealth>().TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
