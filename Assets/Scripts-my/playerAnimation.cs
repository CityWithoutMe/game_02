using UnityEngine;

public class playerAnimation : MonoBehaviour
{
    public Animator animator;
    public Rigidbody2D rb;
    public PhysicsCheck ph;
    public Player2DController controller;
    void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (ph == null) ph = GetComponent<PhysicsCheck>();
        if (controller == null) controller = GetComponent<Player2DController>();
    }

    void Update()
    {
        if (animator == null || rb == null) return;
        animator.SetFloat( "speedX",Mathf.Abs(rb.velocity.x ));
        animator.SetFloat("speedY", Mathf.Abs(rb.velocity.y));
        bool grounded = ph != null ? ph.isGround : controller != null && controller.IsGrounded;
        animator.SetBool("isGround", grounded);
    }
}
