using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Player2DController : MonoBehaviour
{
    [Header("Movement (fallback without PlayerAttributes)")]
    public float walkSpeed = 5f;
    public float runSpeed = 9f;

    [Header("Jump (fallback without PlayerAttributes)")]
    public float jumpForce = 17f;
    public float jumpCutMultiplier = 0.45f;
    public int maxJumps = 1;

    [Header("Ground Check")]
    public LayerMask groundMask;
    public Transform groundCheck;
    public float groundCheckRadius = 0.12f;
    public Transform modelRoot;

    [Header("Attack Direction")]
    public Transform attackPoint;

    private Rigidbody2D body;
    private PlayerAttributes attributes;
    private float move;
    private bool running;
    private bool grounded;
    private int jumpCount;
    private bool controlsEnabled = true;
    private float facingDirection = 1f;
    private float dashDirection = 1f;
    private float dashRemaining;
    private float nextDashTime;
    private float originalGravityScale;

    public bool IsGrounded => grounded;
    public int JumpCount => jumpCount;
    public bool ControlsEnabled => controlsEnabled;
    public bool IsDashing => dashRemaining > 0f;

    private float DashSpeed => attributes != null ? attributes.DashSpeed : 18f;
    private float DashDuration => attributes != null ? attributes.DashDuration : 0.18f;
    private float DashCooldown => attributes != null ? attributes.DashCooldown : 0.65f;

    public void SetControlsEnabled(bool enabled)
    {
        controlsEnabled = enabled;
        if (enabled)
            return;

        EndDash();
        move = 0f;
        running = false;
        if (body != null)
            body.velocity = new Vector2(0f, body.velocity.y);
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        attributes = GetComponent<PlayerAttributes>();
        originalGravityScale = body.gravityScale;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.sleepMode = RigidbodySleepMode2D.NeverSleep;
        body.freezeRotation = true;
    }

    private void OnDisable()
    {
        EndDash();
    }

    private void Update()
    {
        grounded = groundCheck != null &&
                   Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask);

        if (grounded && body.velocity.y <= 0f)
            jumpCount = 0;

        if (!controlsEnabled)
        {
            move = 0f;
            running = false;
            return;
        }

        move = Input.GetAxisRaw("Horizontal");
        running = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        if (move != 0f && !IsDashing)
        {
            facingDirection = Mathf.Sign(move);
            Transform facingRoot = modelRoot != null ? modelRoot : transform;
            Vector3 scale = facingRoot.localScale;
            scale.x = Mathf.Abs(scale.x) * facingDirection;
            facingRoot.localScale = scale;
        }

        if (Input.GetKeyDown(KeyCode.L) && !IsDashing && Time.time >= nextDashTime)
            BeginDash();

        if (IsDashing)
            return;

        // K is the only jump key in this controller.
        if (Input.GetKeyDown(KeyCode.K) && jumpCount < maxJumps)
        {
            float takeoffSpeed = attributes != null ? attributes.JumpVelocity : jumpForce;
            body.velocity = new Vector2(body.velocity.x, takeoffSpeed);
            jumpCount++;
        }

        if (Input.GetKeyUp(KeyCode.K) && body.velocity.y > 0f)
            body.velocity = new Vector2(body.velocity.x, body.velocity.y * jumpCutMultiplier);
    }

    private void FixedUpdate()
    {
        if (!controlsEnabled)
        {
            body.velocity = new Vector2(0f, body.velocity.y);
            return;
        }

        if (IsDashing)
        {
            // Velocity-based movement keeps 2D collision detection active.
            body.velocity = new Vector2(dashDirection * DashSpeed, 0f);
            dashRemaining -= Time.fixedDeltaTime;
            if (dashRemaining <= 0f)
                EndDash();
            return;
        }

        float speed = running ? runSpeed : attributes != null ? attributes.MoveSpeed : walkSpeed;
        body.velocity = new Vector2(move * speed, body.velocity.y);
    }

    private void BeginDash()
    {
        dashDirection = move != 0f ? Mathf.Sign(move) : facingDirection;
        dashRemaining = Mathf.Max(0.01f, DashDuration);
        nextDashTime = Time.time + dashRemaining + DashCooldown;
        body.gravityScale = 0f;
        body.velocity = new Vector2(dashDirection * DashSpeed, 0f);
    }

    private void EndDash()
    {
        dashRemaining = 0f;
        if (body != null)
            body.gravityScale = originalGravityScale;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
