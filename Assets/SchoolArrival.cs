using UnityEngine;

/// <summary>High drop into room one, landing impact, and respawn after death.</summary>
public sealed class SchoolArrival : MonoBehaviour
{
    [SerializeField] private Transform dropPoint;
    [SerializeField] private MetroidvaniaCameraFollow levelCamera;
    [SerializeField] private MetroidvaniaLevelFlow levelFlow;
    private Rigidbody2D body;
    private PlayerMovement2D movement;
    private PlayerAttributes attributes;
    private bool arriving;
    private float dropTime;

    private void Start()
    {
        body = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement2D>();
        attributes = GetComponent<PlayerAttributes>();
        Respawn();
    }

    public void Respawn()
    {
        if (body == null || dropPoint == null) return;
        body.position = dropPoint.position;
        body.velocity = new Vector2(0f, -3f);
        if (attributes != null) attributes.Heal(attributes.MaxHealth);
        if (movement != null) movement.SetControlsEnabled(true);
        arriving = true;
        dropTime = Time.time;
        if (levelFlow != null) levelFlow.OnPlayerRespawn();
        if (levelCamera != null) levelCamera.SnapToPlayer(transform);
    }

    private void Update()
    {
        if (transform.position.y < -8f || (attributes != null && attributes.CurrentHealth <= 0))
        {
            Respawn();
            return;
        }
        if (arriving && Time.time > dropTime + 0.2f && movement != null && movement.IsGrounded)
        {
            arriving = false;
            movement.SetControlsEnabled(true);
            if (levelCamera != null) levelCamera.LandingImpact();
        }
    }
}
