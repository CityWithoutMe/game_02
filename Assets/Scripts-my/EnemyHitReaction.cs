using UnityEngine;

[DefaultExecutionOrder(-100)]
[RequireComponent(typeof(Health))]
public sealed class EnemyHitReaction : MonoBehaviour
{
    [SerializeField, Min(0f)] private float hitStunDuration = 0.15f;
    [SerializeField, Min(0f)] private float knockbackDeceleration = 35f;

    private Health health;
    private EnemyAI enemyAI;
    private Rigidbody2D body;
    private bool wasAttackable;
    private bool stunned;
    private float stunEndsAt;

    private void Awake()
    {
        health = GetComponent<Health>();
        enemyAI = GetComponent<EnemyAI>();
        body = GetComponent<Rigidbody2D>();
        wasAttackable = health.CanAttack;
    }

    private void FixedUpdate()
    {
        bool attackable = health.CanAttack;
        if (wasAttackable && !attackable)
        {
            stunned = true;
            stunEndsAt = Time.time + hitStunDuration;
            if (enemyAI != null)
                enemyAI.enabled = false;
        }
        wasAttackable = attackable;

        if (!stunned)
            return;

        if (body != null)
        {
            body.velocity = new Vector2(
                Mathf.MoveTowards(body.velocity.x, 0f, knockbackDeceleration * Time.fixedDeltaTime),
                body.velocity.y
            );
        }

        if (Time.time < stunEndsAt)
            return;

        stunned = false;
        if (enemyAI != null)
            enemyAI.enabled = true;
    }
}
