using System.Collections;
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
    private Coroutine clearAttackRoutine;
    private bool supportsAttackFlag;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (animator != null)
        {
            foreach (AnimatorControllerParameter parameter in animator.parameters)
                if (parameter.name == "isAttack" && parameter.type == AnimatorControllerParameterType.Bool)
                    supportsAttackFlag = true;
        }
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
            {
                // The attack hit and animation work while the player is airborne.
                if (supportsAttackFlag)
                {
                    animator.SetBool("isAttack", true);
                    if (clearAttackRoutine != null) StopCoroutine(clearAttackRoutine);
                    clearAttackRoutine = StartCoroutine(ClearAttackFlag());
                }
                animator.SetTrigger("attack");
            }

            nextAttackTime = Time.time + attackCooldown;
        }
    }

    private IEnumerator ClearAttackFlag()
    {
        yield return new WaitForSeconds(Mathf.Max(0.5f, attackCooldown));
        if (animator != null) animator.SetBool("isAttack", false);
        clearAttackRoutine = null;
    }

    private void OnDisable()
    {
        if (clearAttackRoutine != null) StopCoroutine(clearAttackRoutine);
        clearAttackRoutine = null;
        if (animator != null && supportsAttackFlag) animator.SetBool("isAttack", false);
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
        HashSet<ExamPaperBoss> damagedBosses = new HashSet<ExamPaperBoss>();
        foreach (Collider2D enemy in enemies)
        {
            ExamPaperBoss paperBoss = enemy.GetComponentInParent<ExamPaperBoss>();
            if (paperBoss != null)
            {
                if (damagedBosses.Add(paperBoss)) paperBoss.TakeHit();
                continue;
            }
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
