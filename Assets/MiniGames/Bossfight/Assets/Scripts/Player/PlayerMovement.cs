using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5;
    Rigidbody2D rb;
    Animator anim;
    private SpriteRenderer sprite;

    public float jumpForce = 15f;
    private bool isGrounded = false;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    Camera cam;

    float minX;
    float maxX;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        cam = Camera.main;
    }

    void Start()
    {
        Vector3 left = cam.ScreenToWorldPoint(new Vector3(0, 0, 0));
        Vector3 right = cam.ScreenToWorldPoint(new Vector3(Screen.width,0,0));
        minX = left.x + 0.5f;
        maxX = right.x - 0.5f;
    }

    void Update()
    {
        float x = 0f;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x += 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) x -= 1f;
        }

        if (x != 0)
        {
            sprite.flipX = x < 0;
        }

        anim.SetFloat("moveX", x);
        anim.SetBool("isMoving", x != 0);

        rb.linearVelocity = new Vector2(x * speed, rb.linearVelocity.y);
       

        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            anim.SetTrigger("attack");
        }

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            anim.SetTrigger("jump");
        }

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        transform.position = pos;
    }
  

    void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }
}
