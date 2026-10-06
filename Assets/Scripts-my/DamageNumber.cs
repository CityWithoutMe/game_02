using TMPro;
using UnityEngine;

public class DamageNumber : MonoBehaviour
{
    public float moveSpeed = 1.5f;
    public float lifetime = 0.8f;

    private TMP_Text text;
    private float timer;

    void Awake()
    {
        text = GetComponent<TMP_Text>();
        timer = lifetime;
    }

    public void SetValue(int value)
    {
        if (text != null)
        {
            text.text = value.ToString();
        }
    }

    void Update()
    {
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            Destroy(gameObject);
        }
    }
}