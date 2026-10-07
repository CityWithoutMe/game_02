using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class FinalExamVictorySceneSetup
{
    [MenuItem("Tools/Final Exam/Add or Update Victory Sequence")]
    public static void Build()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/main.unity");
        var boss = GameObject.Find("Final Exam Encounter");
        if (boss == null) throw new Exception("Final Exam Encounter is missing from main.");
        Configure(boss.GetComponent<FinalExamEncounter>());
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Validate();
        Debug.Log("FINAL_EXAM_VICTORY_BUILD_OK");
    }
    public static void Configure(FinalExamEncounter encounter)
    {
        var old = GameObject.Find("Final Exam Victory UI");
        if (old != null) UnityEngine.Object.DestroyImmediate(old);
        var labelTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/FinalExamBoss/DawnLabel.jpg");
        var paperSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ui/菜单/Settings/settings_paper.png");
        var buttonSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ui/菜单/Settings/settings_button.png");
        if (labelTexture == null || paperSprite == null || buttonSprite == null)
            throw new Exception("Victory label or existing Settings art is missing.");
        var presenter = encounter.GetComponent<FinalExamVictoryPresentation>();
        if (presenter == null) presenter = encounter.gameObject.AddComponent<FinalExamVictoryPresentation>();
        encounter.victoryPresentation = presenter;
        var canvasObject = new GameObject("Final Exam Victory UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.layer = LayerMask.NameToLayer("UI");
        var canvas = canvasObject.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 200;
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f,1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        var flash = new GameObject("Dawn light", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        var flashRect = (RectTransform)flash.transform; flashRect.SetParent(canvasObject.transform,false);
        flashRect.anchorMin = Vector2.zero; flashRect.anchorMax = Vector2.one;
        flashRect.offsetMin = flashRect.offsetMax = Vector2.zero;
        var white = flash.GetComponent<Image>(); white.color = new Color(1f,1f,1f,0f); white.raycastTarget = false;
        presenter.dawnFlash = white;
        var labelObject = new GameObject("Dawn label - 天亮了", typeof(RectTransform), typeof(CanvasGroup));
        var labelRect = (RectTransform)labelObject.transform; labelRect.SetParent(canvasObject.transform,false);
        labelRect.anchorMin = labelRect.anchorMax = labelRect.pivot = new Vector2(0.5f,1f);
        labelRect.anchoredPosition = new Vector2(0f,-30f); labelRect.sizeDelta = new Vector2(800f,238f);
        presenter.dawnLabel = labelObject.GetComponent<CanvasGroup>(); presenter.dawnLabel.alpha = 0f;
        var tag = new GameObject("Original hand-drawn tag", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        var tagRect = (RectTransform)tag.transform; tagRect.SetParent(labelRect,false);
        Stretch(tagRect);
        var raw = tag.GetComponent<RawImage>(); raw.texture = labelTexture;
        // The source has a large blank upper area; UV framing uses the original pixels untouched.
        raw.uvRect = new Rect(0.12f,0f,0.81f,0.38f); raw.raycastTarget = false;
        presenter.dawnLabelText = Text("天亮了",labelRect,new Vector2(0f,-10f),new Vector2(580f,120f),72);
        var panelObject = new GameObject("Victory panel",typeof(RectTransform),typeof(CanvasRenderer),typeof(Image),typeof(CanvasGroup));
        var panelRect = (RectTransform)panelObject.transform; panelRect.SetParent(canvasObject.transform,false);
        panelRect.anchorMin = panelRect.anchorMax = panelRect.pivot = new Vector2(0.5f,0.5f);
        panelRect.anchoredPosition = new Vector2(0f,-80f); panelRect.sizeDelta = new Vector2(760f,560f);
        var panelImage = panelObject.GetComponent<Image>(); panelImage.sprite = paperSprite;
        panelImage.color = new Color(1f,1f,1f,0.96f); panelImage.raycastTarget = true;
        presenter.victoryPanel = panelObject.GetComponent<CanvasGroup>(); presenter.victoryPanel.alpha = 0f;
        presenter.victoryTitle = Text("胜利",panelRect,new Vector2(0f,110f),new Vector2(560f,100f),74);
        presenter.retryButton = Button("重新开始",buttonSprite,panelRect,new Vector2(0f,-75f),out Text retry);
        presenter.retryText = retry;
        presenter.menuButton = Button("返回主菜单",buttonSprite,panelRect,new Vector2(0f,-185f),out Text menu);
        presenter.menuText = menu;
        if (UnityEngine.Object.FindObjectOfType<EventSystem>() == null)
            new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));
    }
    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }
    private static Text Text(string value, Transform parent, Vector2 position, Vector2 size, int fontSize)
    {
        var go = new GameObject(value,typeof(RectTransform),typeof(CanvasRenderer),typeof(Text));
        var rect = (RectTransform)go.transform; rect.SetParent(parent,false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f,0.5f);
        rect.anchoredPosition = position; rect.sizeDelta = size;
        var text = go.GetComponent<Text>(); text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = value; text.fontSize = fontSize; text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(0.05f,0.05f,0.06f); text.raycastTarget = false;
        return text;
    }
    private static Button Button(string label, Sprite sprite, Transform parent, Vector2 position, out Text caption)
    {
        var go = new GameObject(label + " button",typeof(RectTransform),typeof(CanvasRenderer),typeof(Image),typeof(Button));
        var rect = (RectTransform)go.transform; rect.SetParent(parent,false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f,0.5f);
        rect.anchoredPosition = position; rect.sizeDelta = new Vector2(420f,90f);
        var image = go.GetComponent<Image>(); image.sprite = sprite;
        var button = go.GetComponent<Button>(); button.targetGraphic = image;
        var colors = button.colors; colors.highlightedColor = new Color(0.9f,0.9f,0.9f);
        colors.pressedColor = new Color(0.72f,0.72f,0.72f); button.colors = colors;
        caption = Text(label,rect,Vector2.zero,new Vector2(380f,80f),38);
        return button;
    }
    public static void Validate()
    {
        var boss = UnityEngine.Object.FindObjectOfType<FinalExamEncounter>();
        var ui = boss != null ? boss.victoryPresentation : null;
        var canvas = GameObject.Find("Final Exam Victory UI");
        if (ui == null || ui.dawnFlash == null || ui.dawnLabel == null || ui.victoryPanel == null ||
            ui.dawnLabelText == null || ui.dawnLabelText.text != "天亮了" ||
            ui.retryButton == null || ui.menuButton == null || canvas == null)
            throw new Exception("Final victory references are incomplete.");
        if (canvas.GetComponent<Canvas>().sortingOrder <= 100 ||
            canvas.GetComponentInChildren<RawImage>().texture == null ||
            UnityEngine.Object.FindObjectOfType<EventSystem>() == null)
            throw new Exception("Victory canvas, art, or button input is missing.");
        Debug.Log("FINAL_EXAM_VICTORY_VALIDATION_OK");
    }
}
