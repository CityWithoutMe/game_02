using UnityEngine;

public sealed class ExamHazard : MonoBehaviour
{
    private FinalExamEncounter owner;
    private Vector2 velocity;
    private float expires;
    private bool beam;
    private float width;

    public void Configure(FinalExamEncounter encounter, Vector2 speed, float life, bool isBeam, float beamWidth = 0f)
    {
        owner = encounter; velocity = speed; expires = Time.time + life;
        beam = isBeam; width = beamWidth;
    }

    private void Update()
    {
        if (owner == null || !owner.IsFighting || Time.time >= expires)
        { Destroy(gameObject); return; }
        if (beam)
        {
            foreach (Collider2D hit in Physics2D.OverlapBoxAll(transform.position, new Vector2(width, 12f), 0f))
                if (hit.GetComponentInParent<PlayerAttributes>() != null) owner.TryHurtPlayer();
        }
        else
        {
            Vector2 step = velocity * Time.deltaTime;
            // Sweep between frames so a fast sheet cannot skip over the player.
            foreach (RaycastHit2D hit in Physics2D.CircleCastAll(transform.position, 0.18f, step.normalized, step.magnitude))
            {
                if (hit.collider.GetComponentInParent<PlayerAttributes>() == null) continue;
                owner.TryHurtPlayer(); Destroy(gameObject); return;
            }
            transform.position += (Vector3)step;
            transform.Rotate(0f, 0f, 100f * Time.deltaTime);
        }
    }
}
