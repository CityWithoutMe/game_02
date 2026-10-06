using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Player2DController : MonoBehaviour
{
    [Header("移动")]
    public float walkSpeed = 5f;
    public float runSpeed = 9f;

    [Header("跳跃")]
    public float jumpForce = 17f;
    public float jumpCutMultiplier = 0.45f;
    public int maxJumps = 1;

    [Header("地面检测")]
    public LayerMask groundMask;
    public Transform groundCheck;
    public float groundCheckRadius = 0.12f;
    public Transform modelRoot;

    [Header("攻击方向")]
    public Transform attackPoint;

    private Rigidbody2D body;
    private float move;
    private bool running;
    private bool grounded;
    private int jumpCount;
    private bool controlsEnabled = true;

    public bool IsGrounded => grounded;
    public int JumpCount => jumpCount;
    public bool ControlsEnabled => controlsEnabled;

    public void SetControlsEnabled(bool enabled)
    {
        controlsEnabled = enabled;
        if (!enabled)
        {
            move = 0f;
            running = false;
            if (body != null)
                body.velocity = new Vector2(0f, body.velocity.y);
        }
    }


    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.sleepMode = RigidbodySleepMode2D.NeverSleep;
        body.freezeRotation = true;
    }

    void Update()
    {
        grounded = groundCheck != null &&
                   Physics2D.OverlapCircle(
                       groundCheck.position,
                       groundCheckRadius,
                       groundMask
                   );

        if (grounded && body.velocity.y <= 0f)
            jumpCount = 0;

        if (!controlsEnabled)
        {
            move = 0f;
            running = false;
            return;
        }

        move = Input.GetAxisRaw("Horizontal");
        running = Input.GetKey(KeyCode.LeftShift) ||
                  Input.GetKey(KeyCode.RightShift);

        bool jumpPressed =
            Input.GetButtonDown("Jump") ||
            Input.GetKeyDown(KeyCode.Space);

        if (jumpPressed && jumpCount < maxJumps)
        {
            body.velocity = new Vector2(
                body.velocity.x,
                jumpForce
            );

            jumpCount++;
        }

        bool jumpReleased =
            Input.GetButtonUp("Jump") ||
            Input.GetKeyUp(KeyCode.Space);

        if (jumpReleased && body.velocity.y > 0f)
        {
            body.velocity = new Vector2(
                body.velocity.x,
                body.velocity.y * jumpCutMultiplier
            );
        }

        // D 或右方向键向右，A 或左方向键向左
        if (move != 0f)
        {
            Transform facingRoot = modelRoot != null ? modelRoot : transform;
            Vector3 scale = facingRoot.localScale;
            scale.x = Mathf.Abs(scale.x) * Mathf.Sign(move);
            facingRoot.localScale = scale;
        }
    }

    void FixedUpdate()
    {
        if (!controlsEnabled)
        {
            body.velocity = new Vector2(0f, body.velocity.y);
            return;
        }

        float speed = running ? runSpeed : walkSpeed;

        body.velocity = new Vector2(
            move * speed,
            body.velocity.y
        );
    }
  

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            groundCheck.position,
            groundCheckRadius
        );
    }

}
