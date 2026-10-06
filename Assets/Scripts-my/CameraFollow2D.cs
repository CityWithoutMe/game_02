using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public Transform target;
    public float smoothTime = 0.12f;
    public Vector2 offset = new Vector2(2f, 1f);
    Vector3 velocity;
    void LateUpdate()
    {
        if (!target) return;
        Vector3 desired = new Vector3(target.position.x + offset.x, target.position.y + offset.y, transform.position.z);
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
    }
}
