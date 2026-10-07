using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class PlayerHealthHudSceneSetup
{
    [MenuItem("Tools/Rebuild Player Health HUD")]
    public static void Build()
    {
        const string scenePath = "Assets/Scenes/main.unity";
        var scene = EditorSceneManager.OpenScene(scenePath);
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            throw new System.InvalidOperationException("main scene has no tagged Player object.");

        PlayerAttributes attributes = player.GetComponent<PlayerAttributes>();
        PlayerHealthHud hud = player.GetComponent<PlayerHealthHud>();
        Sprite full = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/health_full.png");
        Sprite empty = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/health_empty.png");
        if (attributes == null || hud == null || full == null || empty == null)
            throw new System.InvalidOperationException("Player attributes, HUD script, or paper-plane sprites are missing.");

        GameObject previous = GameObject.Find("Player Health HUD");
        if (previous != null)
            Object.DestroyImmediate(previous);

        GameObject canvasObject = new GameObject(
            "Player Health HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        canvasObject.layer = LayerMask.NameToLayer("UI");
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        GameObject rowObject = new GameObject("Health paper planes", typeof(RectTransform));
        rowObject.layer = canvasObject.layer;
        RectTransform row = rowObject.GetComponent<RectTransform>();
        row.SetParent(canvasObject.transform, false);
        row.anchorMin = row.anchorMax = row.pivot = new Vector2(0f, 1f);
        row.anchoredPosition = new Vector2(24f, -24f);

        const float iconSize = 56f;
        const float iconSpacing = 6f;
        int count = attributes.MaxHealth;
        row.sizeDelta = new Vector2(count * iconSize + (count - 1) * iconSpacing, iconSize);
        Image[] images = new Image[count];
        for (int i = 0; i < count; i++)
        {
            GameObject iconObject = new GameObject(
                "Health " + (i + 1), typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconObject.layer = canvasObject.layer;
            RectTransform iconRect = iconObject.GetComponent<RectTransform>();
            iconRect.SetParent(row, false);
            iconRect.anchorMin = iconRect.anchorMax = iconRect.pivot = new Vector2(0f, 1f);
            iconRect.anchoredPosition = new Vector2(i * (iconSize + iconSpacing), 0f);
            iconRect.sizeDelta = new Vector2(iconSize, iconSize);

            Image image = iconObject.GetComponent<Image>();
            image.sprite = i < attributes.CurrentHealth ? full : empty;
            image.preserveAspect = true;
            image.raycastTarget = false;
            images[i] = image;
        }

        SerializedObject serializedHud = new SerializedObject(hud);
        serializedHud.FindProperty("attributes").objectReferenceValue = attributes;
        serializedHud.FindProperty("fullSprite").objectReferenceValue = full;
        serializedHud.FindProperty("emptySprite").objectReferenceValue = empty;
        serializedHud.FindProperty("hudCanvas").objectReferenceValue = canvas;
        SerializedProperty iconArray = serializedHud.FindProperty("icons");
        iconArray.arraySize = images.Length;
        for (int i = 0; i < images.Length; i++)
            iconArray.GetArrayElementAtIndex(i).objectReferenceValue = images[i];
        serializedHud.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene))
            throw new System.InvalidOperationException("Could not save main scene with player HUD.");
        Debug.Log("Player Health HUD created with " + count + " paper-plane icons.");
    }
}
