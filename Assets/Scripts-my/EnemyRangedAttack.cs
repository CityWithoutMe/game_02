using System.Collections;
using UnityEngine;

/// <summary>
/// Ranged combat controller for enemies that use an Animator trigger named "attack".
/// </summary>
public sealed class EnemyRangedAttack : MonoBehaviour
{
    [Header("Attack range")]
    [SerializeField] private float attackRange = 8f;
    [SerializeField] private float minimumAttackRange;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private int attackDamage = 1;

    [Header("Animation timing")]
    [SerializeField] private string attackTrigger = "attack";
    [SerializeField] private float attackDuration = 0.7f;
    [SerializeField] private float projectileDelay = 0.32f;

    [Header("Projectile")]
    [SerializeField] private Sprite projectileSprite;
    [SerializeField] private float projectileSpeed = 7f;
    [SerializeField] private float projectileLifetime = 3f;
    [SerializeField] private float projectileScale = 0.18f;
    [SerializeField] private Vector2 projectileOffset = new Vector2(0.75f, 0.35f);

    private Animator animator;
    private EnemyAI enemyAI;
    private Health health;
    private Rigidbody2D rb;
    private Transform player;
    private float nextAttackTime;
    private bool attacking;

    public void Configure(
        Sprite rangedProjectileSprite,
        float rangedAttackRange,
        float rangedMinimumRange,
        string rangedAttackTrigger,
        float rangedCooldown,
        int rangedDamage,
        float rangedDuration,
        float rangedProjectileDelay,
        float rangedProjectileSpeed,
        float rangedProjectileLifetime,
        float rangedProjectileScale
    )
    {
        projectileSprite = rangedProjectileSprite;
        attackRange = rangedAttackRange;
        minimumAttackRange = rangedMinimumRange;
        attackTrigger = rangedAttackTrigger;
        attackCooldown = rangedCooldown;
        attackDamage = rangedDamage;
        attackDuration = rangedDuration;
        projectileDelay = rangedProjectileDelay;
        projectileSpeed = rangedProjectileSpeed;
        projectileLifetime = rangedProjectileLifetime;
        projectileScale = rangedProjectileScale;
    }

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        enemyAI = GetComponent<EnemyAI>();
        health = GetComponent<Health>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        nextAttackTime = Time.time + 0.5f;
        FindPlayer();
    }

    private void Update()
    {
        if (player == null)
        {
            FindPlayer();
            return;
        }

        if (health != null && !health.CanAttack)
            return;

        if (attacking || Time.time < nextAttackTime)
            return;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance >= minimumAttackRange && distance <= attackRange)
            StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        attacking = true;
        nextAttackTime = Time.time + attackCooldown;

        FacePlayer();
        if (enemyAI != null)
            enemyAI.enabled = false;
        if (rb != null)
            rb.velocity = new Vector2(0f, rb.velocity.y);

        if (animator != null && !string.IsNullOrEmpty(attackTrigger))
        {
            animator.ResetTrigger(attackTrigger);
            animator.SetTrigger(attackTrigger);
        }

        yield return new WaitForSeconds(Mathf.Clamp(projectileDelay, 0f, attackDuration));
        FireProjectile();

        float remaining = Mathf.Max(0f, attackDuration - projectileDelay);
        if (remaining > 0f)
            yield return new WaitForSeconds(remaining);

        if (enemyAI != null && (health == null || health.CanAttack))
            enemyAI.enabled = true;
        attacking = false;
    }

    private void OnDisable()
    {
        if (enemyAI != null)
            enemyAI.enabled = true;

        attacking = false;
    }

    private void FireProjectile()
    {
        if (player == null)
            return;

        Vector2 direction = (player.position - transform.position).normalized;
        if (direction.sqrMagnitude < 0.001f)
            direction = transform.localScale.x < 0f ? Vector2.right : Vector2.left;

        float facing = Mathf.Sign(direction.x);
        Vector3 spawnPosition = transform.position + new Vector3(
            projectileOffset.x * facing,
            projectileOffset.y,
            0f
        );

        GameObject projectileObject = new GameObject(gameObject.name + "_Projectile");
        projectileObject.transform.position = spawnPosition;
        projectileObject.transform.localScale = Vector3.one * projectileScale;
        if (transform.parent != null)
            projectileObject.transform.SetParent(transform.parent, true);

        SpriteRenderer renderer = projectileObject.AddComponent<SpriteRenderer>();
        renderer.sprite = projectileSprite;
        renderer.color = new Color(1f, 0.85f, 0.2f, 1f);
        renderer.flipX = direction.x > 0f;
        renderer.sortingOrder = 10;

        CircleCollider2D collider = projectileObject.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.5f;

        EnemyProjectile projectile = projectileObject.AddComponent<EnemyProjectile>();
        projectile.Launch(
            direction,
            gameObject,
            projectileSpeed,
            attackDamage,
            projectileLifetime
        );
    }

    private void FacePlayer()
    {
        if (player == null)
            return;

        float direction = Mathf.Sign(player.position.x - transform.position.x);
        if (Mathf.Approximately(direction, 0f))
            return;

        Vector3 scale = transform.localScale;
        scale.x = -Mathf.Abs(scale.x) * direction;
        transform.localScale = scale;
    }

    private void FindPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        player = playerObject != null ? playerObject.transform : null;
    }
}
