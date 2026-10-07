using UnityEngine;

// A short-lived sheet silhouette that tumbles away when the exam is defeated.
public sealed class DefeatedExamFragment : MonoBehaviour
{
    private Vector2 velocity;
    private float spin;
    private float age;
    private float lifetime;
    private SpriteRenderer visual;
    private Vector3 originalScale;

    public void Launch(Vector2 initialVelocity, float angularSpeed)
    {
        velocity = initialVelocity;
        spin = angularSpeed;
        lifetime = Random.Range(1.1f, 2.2f);
        visual = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
    }
    private void Update()
    {
        age += Time.deltaTime;
        velocity += Vector2.down * (5f * Time.deltaTime);
        transform.position += (Vector3)(velocity * Time.deltaTime);
        transform.Rotate(0f, 0f, spin * Time.deltaTime);
        float remaining = Mathf.Clamp01(1f - age / lifetime);
        transform.localScale = originalScale * remaining;
        visual.color = new Color(1f, 1f, 1f, remaining);
        if (age >= lifetime) Destroy(gameObject);
    }
}
