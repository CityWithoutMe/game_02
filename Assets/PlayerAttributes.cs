using UnityEngine;

/// <summary>Editable player values shared by movement and future combat scripts.</summary>
public sealed class PlayerAttributes : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField, Min(0f)] private float moveSpeed = 5f;
    [SerializeField, Min(0f)] private float jumpVelocity = 10f;

    [Header("Player")]
    [SerializeField, Min(1)] private int maxHealth = 100;
    [SerializeField, Min(0)] private int currentHealth = 100;
    [SerializeField, Min(0)] private int attackPower = 10;
    [SerializeField, Min(0)] private int defense = 0;
    [SerializeField] private GameObject damageNumberPrefab;
    [SerializeField] private Transform damageNumberPoint;

    public float MoveSpeed => moveSpeed;
    public float JumpVelocity => jumpVelocity;
    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public int AttackPower => attackPower;
    public int Defense => defense;

    private void OnValidate()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }

    public void TakeDamage(int amount)
    {
        int damage = Mathf.Max(0, amount - defense);
        if (damage == 0 || currentHealth <= 0)
            return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        ShowDamageNumber(damage);
    }

    private void ShowDamageNumber(int damage)
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

        DamageNumber number = numberObject.GetComponent<DamageNumber>();
        if (number != null)
            number.SetValue(damage);
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + Mathf.Max(0, amount));
    }
}
