using System.Collections;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("生命值")]
    public int maxHealth = 100;

    [Header("受击无敌")]
    public float invincibleTime = 0.5f;

    [Header("简单击退")]
    public float knockbackForce = 8f;
    public float knockbackUpForce = 3f;

    [Header("伤害数字")]
    public GameObject damageNumberPrefab;
    public Transform damageNumberPoint;

    private int currentHealth;
    private bool invincible;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    public int CurrentHealth => currentHealth;

    public float HealthPercent =>
        maxHealth > 0
            ? (float)currentHealth / maxHealth
            : 0f;

    public bool CanAttack =>
        !invincible && currentHealth > 0;

    void Awake()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }
    void ApplyKnockback(Vector2 attackerPosition)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        if (rb == null)
            return;

        float direction =
            transform.position.x >= attackerPosition.x ? 1f : -1f;

        rb.AddForce(
            new Vector2(
                direction * knockbackForce,
            knockbackUpForce
            ),
            ForceMode2D.Impulse
        );
    }
    // 兼容不传攻击者位置的调用
    public void TakeDamage(int damage)
    {
        TakeDamage(damage, transform.position);
    }

    // 兼容玩家和怪物攻击脚本的调用
    public void TakeDamage(int damage, Vector2 attackerPosition)
    {
        if (invincible || currentHealth <= 0)
            return;

        currentHealth = Mathf.Max(currentHealth - damage, 0);
        ApplyKnockback(attackerPosition);
        ShowDamageNumber(damage);

        Debug.Log(
            gameObject.name + " 受到 " +
            damage + " 点伤害"
        );


        StartCoroutine(InvincibleCoroutine());
        StartCoroutine(FlashCoroutine());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void ShowDamageNumber(int damage)
    {
        if (damageNumberPrefab == null)
            return;

        Vector3 spawnPosition = damageNumberPoint != null
            ? damageNumberPoint.position
            : transform.position + Vector3.up;

        GameObject numberObject = Instantiate(
            damageNumberPrefab,
            spawnPosition,
            Quaternion.identity
        );

        DamageNumber number =
            numberObject.GetComponent<DamageNumber>();

        if (number != null)
        {
            number.SetValue(damage);
        }
    }

    IEnumerator InvincibleCoroutine()
    {
        invincible = true;

        yield return new WaitForSeconds(invincibleTime);

        invincible = false;
    }

    IEnumerator FlashCoroutine()
    {
        if (spriteRenderer == null)
            yield break;

        spriteRenderer.color = Color.red;

        yield return new WaitForSeconds(0.1f);

        spriteRenderer.color = originalColor;
    }

    void Die()
    {
        Destroy(gameObject);
    }
}