using UnityEngine;

public class PhysicsCheck : MonoBehaviour
{
    public Transform groundCheck;
    public float checkRadius = 0.15f;
    public LayerMask groundLayer;

    public bool isGround { get; private set; }

    private void Update()
    {
        if (groundCheck == null)
        {
            isGround = false;
            return;
        }

        isGround = Physics2D.OverlapCircle(
            groundCheck.position,
            checkRadius,
            groundLayer
        );
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            groundCheck.position,
            checkRadius
        );
    }
}