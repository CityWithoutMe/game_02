using UnityEngine;
using UnityEngine.UI;

/// <summary>Shows one paper plane for each point of the player's health.</summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerAttributes))]
public sealed class PlayerHealthHud : MonoBehaviour
{
    [SerializeField] private PlayerAttributes attributes;
    [SerializeField] private Sprite fullSprite;
    [SerializeField] private Sprite emptySprite;
    [SerializeField, Min(1f)] private float iconSize = 56f;
    [SerializeField, Min(0f)] private float iconSpacing = 6f;
    [SerializeField] private Canvas hudCanvas;
    [SerializeField] private Image[] icons;

    private RectTransform iconRow;
    private bool createdCanvas;

    private void Awake()
    {
        if (attributes == null)
            attributes = GetComponent<PlayerAttributes>();

        if (fullSprite == null)
            fullSprite = Resources.Load<Sprite>("health_full");
        if (emptySprite == null)
            emptySprite = Resources.Load<Sprite>("health_empty");

        if (attributes == null || fullSprite == null || emptySprite == null)
        {
            Debug.LogError("Player Health HUD requires PlayerAttributes and both paper-plane sprites.", this);
            enabled = false;
            return;
        }

        if (icons == null || icons.Length == 0)
        {
            CreateCanvas();
            createdCanvas = true;
        }
        Refresh(attributes.CurrentHealth, attributes.MaxHealth);
    }

    private void OnEnable()
    {
        if (attributes != null)
            attributes.HealthChanged += Refresh;
        if (hudCanvas != null)
            hudCanvas.enabled = true;
    }

    private void OnDisable()
    {
        if (attributes != null)
            attributes.HealthChanged -= Refresh;
        if (hudCanvas != null)
            hudCanvas.enabled = false;
    }

    private void OnDestroy()
    {
        if (createdCanvas && hudCanvas != null)
            Destroy(hudCanvas.gameObject);
    }

    private void CreateCanvas()
    {
        GameObject canvasObject = new GameObject(
            "Player Health HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        hudCanvas = canvasObject.GetComponent<Canvas>();
        hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        hudCanvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        GameObject rowObject = new GameObject("Health paper planes", typeof(RectTransform));
        iconRow = rowObject.GetComponent<RectTransform>();
        iconRow.SetParent(canvasObject.transform, false);
        iconRow.anchorMin = iconRow.anchorMax = iconRow.pivot = new Vector2(0f, 1f);
        iconRow.anchoredPosition = new Vector2(24f, -24f);
    }

    private void Refresh(int currentHealth, int maxHealth)
    {
        int count = Mathf.Max(1, maxHealth);
        if (iconRow != null && (icons == null || icons.Length != count))
            CreateIcons(count);
        if (icons == null)
            return;

        for (int i = 0; i < icons.Length; i++)
            if (icons[i] != null)
                icons[i].sprite = i < currentHealth ? fullSprite : emptySprite;
    }

    private void CreateIcons(int count)
    {
        if (icons != null)
        {
            foreach (Image icon in icons)
                if (icon != null)
                    Destroy(icon.gameObject);
        }

        icons = new Image[count];
        iconRow.sizeDelta = new Vector2(count * iconSize + (count - 1) * iconSpacing, iconSize);

        for (int i = 0; i < count; i++)
        {
            GameObject iconObject = new GameObject(
                "Health " + (i + 1), typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform iconRect = iconObject.GetComponent<RectTransform>();
            iconRect.SetParent(iconRow, false);
            iconRect.anchorMin = iconRect.anchorMax = iconRect.pivot = new Vector2(0f, 1f);
            iconRect.anchoredPosition = new Vector2(i * (iconSize + iconSpacing), 0f);
            iconRect.sizeDelta = new Vector2(iconSize, iconSize);

            Image icon = iconObject.GetComponent<Image>();
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            icons[i] = icon;
        }
    }
}
