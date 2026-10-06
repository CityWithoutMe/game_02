using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    public Transform leftPoint;
    public Transform rightPoint;
    public float moveSpeed = 2f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool movingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void FixedUpdate()
    {
        Transform target = movingRight ? rightPoint : leftPoint;

        float direction = Mathf.Sign(target.position.x - rb.position.x);

        rb.velocity = new Vector2(
            direction * moveSpeed,
            rb.velocity.y
        );

        if (Mathf.Abs(rb.position.x - target.position.x) < 0.05f)
        {
            movingRight = !movingRight;
            spriteRenderer.flipX = !spriteRenderer.flipX;
        }
    }
}