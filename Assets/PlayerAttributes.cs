using System;
using UnityEngine;

/// <summary>Editable player values shared by movement and future combat scripts.</summary>
public sealed class PlayerAttributes : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField, Min(0f)] private float moveSpeed = 5f;
    [SerializeField, Min(0f)] private float jumpVelocity = 10f;
    [SerializeField, Min(0f)] private float dashSpeed = 18f;
    [SerializeField, Min(0.01f)] private float dashDuration = 0.18f;
    [SerializeField, Min(0f)] private float dashCooldown = 0.65f;

    [Header("Player")]
    [SerializeField, Min(1)] private int maxHealth = 7;
    [SerializeField, Min(0)] private int currentHealth = 7;
    [SerializeField, Min(0)] private int attackPower = 10;
    [SerializeField, Min(0)] private int defense = 0;
    [SerializeField] private GameObject damageNumberPrefab;
    [SerializeField] private Transform damageNumberPoint;

    public float MoveSpeed => moveSpeed;
    public float JumpVelocity => jumpVelocity;
    public float DashSpeed => dashSpeed;
    public float DashDuration => dashDuration;
    public float DashCooldown => dashCooldown;
    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public int AttackPower => attackPower;
    public int Defense => defense;
    public event Action<int, int> HealthChanged;

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
        HealthChanged?.Invoke(currentHealth, maxHealth);
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
        int healedHealth = Mathf.Min(maxHealth, currentHealth + Mathf.Max(0, amount));
        if (healedHealth == currentHealth)
            return;

        currentHealth = healedHealth;
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
