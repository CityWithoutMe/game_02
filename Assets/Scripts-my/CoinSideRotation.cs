using UnityEngine;

public sealed class CoinSideRotation : MonoBehaviour
{
    public float rotateSpeed = 100f;

    private void Update()
    {
        transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f);
    }
}
