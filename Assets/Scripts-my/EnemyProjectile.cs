using UnityEngine;

/// <summary>
/// A lightweight enemy projectile that reuses the player's Health feedback.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public sealed class EnemyProjectile : MonoBehaviour
{
    private Rigidbody2D rb;
    private Collider2D projectileCollider;
    private GameObject owner;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        projectileCollider = GetComponent<Collider2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        projectileCollider.isTrigger = true;
    }

    public void Launch(
        Vector2 direction,
        GameObject projectileOwner,
        float speed,
        int damage,
        float lifetime
    )
    {
        owner = projectileOwner;

        if (owner != null)
        {
            Collider2D ownerCollider = owner.GetComponent<Collider2D>();
            if (ownerCollider != null)
                Physics2D.IgnoreCollision(projectileCollider, ownerCollider, true);
        }

        rb.velocity = direction.sqrMagnitude > 0.001f
            ? direction.normalized * speed
            : Vector2.right * speed;

        Destroy(gameObject, lifetime);

        this.damage = damage;
    }

    private int damage = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (owner != null && (other.gameObject == owner || other.transform.IsChildOf(owner.transform)))
            return;

        bool hitPlayer = other.CompareTag("Player") || other.transform.root.CompareTag("Player");
        if (hitPlayer)
        {
            Health health = other.GetComponentInParent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage, transform.position);
                Destroy(gameObject);
                return;
            }

            PlayerAttributes attributes = other.GetComponentInParent<PlayerAttributes>();
            if (attributes != null)
            {
                attributes.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }
        }

        if (!other.isTrigger)
            Destroy(gameObject);
    }
}
