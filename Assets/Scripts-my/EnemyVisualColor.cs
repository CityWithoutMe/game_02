using UnityEngine;

public sealed class EnemyVisualColor : MonoBehaviour
{
    public Color normalColor = new Color(1f, 0.45f, 0.05f, 1f);

    private Health health;
    private SpriteRenderer[] renderers;

    private void Awake()
    {
        health = GetComponent<Health>();
        renderers = GetComponentsInChildren<SpriteRenderer>();
    }

    public void ApplyNow()
    {
        if (renderers == null)
            renderers = GetComponentsInChildren<SpriteRenderer>();

        SetColor(normalColor);
    }

    private void LateUpdate()
    {
        Color color = health != null && !health.CanAttack
            ? Color.red
            : normalColor;
        SetColor(color);
    }

    private void SetColor(Color color)
    {
        if (renderers == null)
            return;

        for (int i = 0; i < renderers.Length; i++)
            renderers[i].color = color;
    }
}
