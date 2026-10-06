using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public Transform attackPoint;
    public float attackRange = 1.35f;
    public int attackDamage = 1;
    public float attackCooldown = 0.5f;
    public Animator animator;

    private float nextAttackTime;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        Player2DController controller = GetComponent<Player2DController>();
        if (controller != null && !controller.ControlsEnabled)
            return;

        if (Input.GetKeyDown(KeyCode.J) &&
            Time.time >= nextAttackTime)
        {
            Attack();

            if (animator != null)
                animator.SetTrigger("attack");

            nextAttackTime = Time.time + attackCooldown;
        }
    }

    private void Attack()
    {
        if (attackPoint == null)
            return;

        Collider2D[] enemies = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRange
        );

        HashSet<Health> damagedEnemies = new HashSet<Health>();
        foreach (Collider2D enemy in enemies)
        {
            Health health = enemy.GetComponent<Health>();

            if (health == null)
                health = enemy.GetComponentInParent<Health>();

            if (health != null &&
                health.transform != transform &&
                !health.transform.IsChildOf(transform) &&
                damagedEnemies.Add(health))
            {
                health.TakeDamage(
                    attackDamage,
                    transform.position
                );
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            attackPoint.position,
            attackRange
        );
    }
}
