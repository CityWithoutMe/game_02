using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>Painted Tilemap door: an open door never blocks movement.</summary>
[RequireComponent(typeof(Tilemap), typeof(BoxCollider2D))]
public sealed class SchoolDoor : MonoBehaviour
{
    [SerializeField] private TileBase closedTile;
    [SerializeField] private TileBase openTile;
    [SerializeField] private bool initiallyOpen;
    [SerializeField] private bool initiallyLocked;
    [SerializeField] private bool alwaysOpen;
    [SerializeField] private string requiredCoinId;
    private Tilemap tiles;
    private BoxCollider2D blocker;
    private Transform player;
    private bool externallyLocked;
    private bool requiredCoinCollected;
    public bool IsOpen { get; private set; }
    public bool IsLocked { get; private set; }

    private void Awake()
    {
        externallyLocked = initiallyLocked;
        requiredCoinCollected = string.IsNullOrEmpty(requiredCoinId);
        RefreshLockState();
        SetOpen(initiallyOpen);
    }

    private void OnEnable()
    {
        CoinRoomPickup.Collected += HandleCoinCollected;
    }

    private void OnDisable()
    {
        CoinRoomPickup.Collected -= HandleCoinCollected;
    }

    private void HandleCoinCollected(string coinId)
    {
        if (!string.IsNullOrEmpty(requiredCoinId) && coinId == requiredCoinId)
        {
            requiredCoinCollected = true;
            RefreshLockState();
        }
    }

    private void RefreshLockState()
    {
        IsLocked = !alwaysOpen && (externallyLocked || !requiredCoinCollected);
        if (alwaysOpen)
            SetOpen(true);
        else if (IsLocked)
            SetOpen(false);
    }

    public void SetOpen(bool open)
    {
        if (alwaysOpen) open = true;
        else if (open && IsLocked) return;

        if (tiles == null) tiles = GetComponent<Tilemap>();
        if (blocker == null) blocker = GetComponent<BoxCollider2D>();
        IsOpen = open;
        tiles.SetTile(Vector3Int.zero, open ? openTile : closedTile);
        blocker.enabled = !open;
    }

    public void SetLocked(bool locked)
    {
        if (alwaysOpen) return;
        externallyLocked = locked;
        RefreshLockState();
    }

    private bool InReach()
    {
        if (player == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found != null) player = found.transform;
        }
        return player != null && Mathf.Abs(player.position.x - transform.position.x) < 2.6f
            && Mathf.Abs(player.position.y - 2f) < 2.2f;
    }

    private void Update()
    {
        if (!alwaysOpen && !IsLocked && InReach() && Input.GetKeyDown(KeyCode.E))
        {
            // Closing while occupied would push the player into a wall.
            if (IsOpen && Mathf.Abs(player.position.x - transform.position.x) < 1.4f) return;
            SetOpen(!IsOpen);
        }
    }

    private void OnGUI()
    {
        if (alwaysOpen || !InReach()) return;
        string message = IsLocked
            ? (!requiredCoinCollected ? "COIN REQUIRED" : "LOCKED")
            : IsOpen ? "E  /  CLOSE" : "E  /  OPEN";
        GUI.Box(new Rect(Screen.width * 0.5f - 85f, Screen.height - 70f, 170f, 34f), message);
    }
}
