using System;
using UnityEngine;

/// <summary>Publishes which room coin was collected without changing the existing Coin script.</summary>
public sealed class CoinRoomPickup : MonoBehaviour
{
    public static event Action<string> Collected;

    [SerializeField] private string coinId;
    private bool collected;

    public void SetCoinId(string id)
    {
        coinId = id;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || !other.CompareTag("Player"))
            return;

        collected = true;
        string id = string.IsNullOrEmpty(coinId) ? gameObject.name : coinId;
        Collected?.Invoke(id);
    }
}
