using UnityEngine;

public class Lightning : MonoBehaviour
{
    public float speed = 12f;
    public float lifetime = 0.8f;
    public GameObject lightningPrefab;
    public int direction = 1;
    public int damage = 2;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void SetDirection(int dir)
    {
        direction = dir;
    }

    void Update()
    {
        transform.position += Vector3.right * direction * speed * Time.deltaTime;
    }
    public void DoLightning()
    {
        int side = Random.Range(0, 2); // links=0, rechts=1

        float x;
        if (side == 0) { x = -10f; }
        else { x = 10f; }

            float y = -1f;
        Vector3 pos = new Vector3(x, y, 0);

        Instantiate(lightningPrefab, pos, Quaternion.identity);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<PlayerHealth>().TakeDamage(damage);
        }
    }

}
