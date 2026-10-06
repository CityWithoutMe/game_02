using UnityEngine;

public class Coin : MonoBehaviour
{
    public int scoreValue = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        Debug.Log("获得金币，分数增加：" + scoreValue);

        Destroy(gameObject);
    }
    public float rotateSpeed = 100f;

    void Update()
    {
        transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);
    }
}