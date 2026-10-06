using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public float attackRange = 1.2f;
    public int attackDamage = 1;
    public float attackCooldown = 1f;

    private Transform player;
    private float nextAttackTime;
    private Animator animator;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    void Update()
    {
        if (player == null)
            return;

        float distance = Vector2.Distance(
            transform.position,
            player.position
        );

        if (distance <= attackRange &&
            Time.time >= nextAttackTime)
        {
            if (animator != null)
                animator.SetTrigger("attack");

            Attack();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    void Attack()
    {
        Health health = player.GetComponent<Health>();

        if (health != null)
        {
            health.TakeDamage(attackDamage, transform.position);
            return;
        }

        PlayerAttributes attributes = player.GetComponent<PlayerAttributes>();
        if (attributes != null)
            attributes.TakeDamage(attackDamage);
    }
}
