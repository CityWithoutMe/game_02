using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(PlayerAttributes))]
public sealed class PlayerMovement2D : MonoBehaviour
{
    [SerializeField] private PlayerAttributes attributes;
    [SerializeField] private LayerMask groundLayers = ~0;
    [SerializeField] private Player2DController platformerController;

    private Rigidbody2D body;
    private BoxCollider2D bodyCollider;
    private SpriteRenderer sprite;
    private readonly Collider2D[] groundHits = new Collider2D[8];
    private float horizontal;
    private bool jumpQueued;
    private bool controlsEnabled = true;
    private int legacyJumpCount;
    public int JumpCount => platformerController != null ? platformerController.JumpCount : legacyJumpCount;
    public bool IsGrounded => platformerController != null
        ? platformerController.IsGrounded
        : bodyCollider != null && CheckGrounded();

    public void SetControlsEnabled(bool enabled)
    {
        if (platformerController != null)
        {
            platformerController.SetControlsEnabled(enabled);
            return;
        }
        controlsEnabled = enabled;
        horizontal = 0f;
        jumpQueued = false;
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<BoxCollider2D>();
        sprite = GetComponent<SpriteRenderer>();
        if (platformerController == null) platformerController = GetComponent<Player2DController>();
        if (attributes == null) attributes = GetComponent<PlayerAttributes>();
        body.freezeRotation = true;
    }

    private void Update()
    {
        if (platformerController != null) return;
        if (!controlsEnabled) return;
        horizontal = (Input.GetKey(KeyCode.D) ? 1f : 0f) - (Input.GetKey(KeyCode.A) ? 1f : 0f);
        if (Input.GetKeyDown(KeyCode.K)) jumpQueued = true;
        if (sprite != null && horizontal != 0f) sprite.flipX = horizontal < 0f;
    }

    private void FixedUpdate()
    {
        if (platformerController != null) return;
        body.velocity = new Vector2(horizontal * attributes.MoveSpeed, body.velocity.y);
        if (jumpQueued && IsGrounded)
        {
            body.velocity = new Vector2(body.velocity.x, attributes.JumpVelocity);
            legacyJumpCount++;
        }
        jumpQueued = false;
    }

    private bool CheckGrounded()
    {
        Bounds bounds = bodyCollider.bounds;
        Vector2 center = new Vector2(bounds.center.x, bounds.min.y - 0.06f);
        Vector2 size = new Vector2(bounds.size.x * 0.75f, 0.10f);
        int count = Physics2D.OverlapBoxNonAlloc(center, size, 0f, groundHits, groundLayers);
        for (int i = 0; i < count; i++)
            if (groundHits[i] != null && groundHits[i] != bodyCollider && !groundHits[i].isTrigger)
                return true;
        return false;
    }
}
