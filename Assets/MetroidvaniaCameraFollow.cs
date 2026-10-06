using UnityEngine;

public sealed class MetroidvaniaCameraFollow : MonoBehaviour
{
    [SerializeField] private float minX = 0f;
    [SerializeField] private float maxX = 196f;
    [SerializeField] private float fixedY = 6f;
    [SerializeField] private float smoothTime = 0.1f;
    [SerializeField] private float shaftEndX = 24f;
    [SerializeField] private float maxY = 24f;
    private Transform player;
    private Vector3 velocity;
    private Vector3 followPosition;
    private float impactTime = -10f;
    private bool initialized;

    public void SnapToPlayer(Transform target)
    {
        player = target;
        followPosition = TargetPosition();
        transform.position = followPosition;
        velocity = Vector3.zero;
        initialized = true;
    }

    public void LandingImpact() { impactTime = Time.time; }

    private Vector3 TargetPosition()
    {
        float y = player.position.x < shaftEndX ? Mathf.Clamp(player.position.y + 1f, fixedY, maxY) : fixedY;
        Camera lens = GetComponent<Camera>();
        float halfWidth = lens != null ? lens.orthographicSize * lens.aspect : 10.7f;
        float x = maxX - minX > halfWidth * 2f
            ? Mathf.Clamp(player.position.x + 2f, minX + halfWidth, maxX - halfWidth)
            : (minX + maxX) * 0.5f;
        return new Vector3(x, y, transform.position.z);
    }

    private void LateUpdate()
    {
        if (player == null)
        {
            GameObject found = GameObject.FindWithTag("Player");
            if (found == null) return;
            player = found.transform;
        }
        if (!initialized) SnapToPlayer(player);
        followPosition = Vector3.SmoothDamp(followPosition, TargetPosition(), ref velocity, smoothTime);
        float age = Time.time - impactTime;
        float shake = age < 0.35f ? Mathf.Sin(age * 85f) * 0.16f * (1f - age / 0.35f) : 0f;
        transform.position = followPosition + Vector3.up * shake;
    }
}
