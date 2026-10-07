using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Scene-local ending: temporary exposure, the supplied hand-drawn label, then choices.
public sealed class FinalExamVictoryPresentation : MonoBehaviour
{
    public Image dawnFlash;
    public CanvasGroup dawnLabel;
    public CanvasGroup victoryPanel;
    public Text dawnLabelText;
    public Text victoryTitle;
    public Text retryText;
    public Text menuText;
    public Button retryButton;
    public Button menuButton;
    [Min(0.1f)] public float brightenSeconds = 2.2f;
    [Min(0f)] public float brightnessBoost = 1.1f;
    [Min(0.1f)] public float labelFadeSeconds = 0.8f;
    [Min(0f)] public float labelHoldSeconds = 1.5f;
    [Min(0.1f)] public float panelFadeSeconds = 0.8f;
    private bool started;
    private Font runtimeFont;

    private void Awake()
    {
        if (dawnLabel == null || victoryPanel == null || dawnFlash == null || retryButton == null || menuButton == null)
        {
            Debug.LogError("Final Exam victory UI references are missing.", this);
            enabled = false;
            return;
        }
        dawnLabel.alpha = 0f;
        dawnLabel.blocksRaycasts = false;
        victoryPanel.alpha = 0f;
        victoryPanel.blocksRaycasts = false;
        victoryPanel.interactable = false;
        dawnFlash.color = new Color(1f, 1f, 1f, 0f);
        runtimeFont = Font.CreateDynamicFontFromOSFont(
            new[] { "STKaiti", "KaiTi", "Microsoft YaHei", "SimHei", "Noto Sans CJK SC", "Arial" }, 72);
        if (runtimeFont == null) runtimeFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        foreach (Text label in new[] { dawnLabelText, victoryTitle, retryText, menuText })
            if (label != null) label.font = runtimeFont;
        retryButton.onClick.AddListener(Restart);
        menuButton.onClick.AddListener(ReturnToMenu);
    }
    public void ShowVictory()
    {
        if (!enabled || started) return;
        started = true;
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var movement = player.GetComponent<PlayerMovement2D>();
            if (movement != null) movement.SetControlsEnabled(false);
        }
        StartCoroutine(PlaySequence());
    }
    private IEnumerator PlaySequence()
    {
        var display = DisplayColorSettings.Instance;
        // The moment the exam bursts, a white flash gives way to a lasting dawn.
        for (float t = 0f; t < brightenSeconds; t += Time.unscaledDeltaTime)
        {
            float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / brightenSeconds));
            if (display != null) display.SetSceneBrightnessOffset(brightnessBoost * k);
            dawnFlash.color = new Color(1f, 1f, 1f, Mathf.Lerp(0.62f, 0.07f, k));
            yield return null;
        }
        if (display != null) display.SetSceneBrightnessOffset(brightnessBoost);
        dawnFlash.color = new Color(1f, 1f, 1f, 0.07f);
        for (float t = 0f; t < labelFadeSeconds; t += Time.unscaledDeltaTime)
        {
            dawnLabel.alpha = Mathf.Clamp01(t / labelFadeSeconds);
            yield return null;
        }
        dawnLabel.alpha = 1f;
        yield return new WaitForSecondsRealtime(labelHoldSeconds);
        for (float t = 0f; t < panelFadeSeconds; t += Time.unscaledDeltaTime)
        {
            victoryPanel.alpha = Mathf.Clamp01(t / panelFadeSeconds);
            yield return null;
        }
        victoryPanel.alpha = 1f;
        victoryPanel.blocksRaycasts = true;
        victoryPanel.interactable = true;
    }
    public void Restart()
    {
        if (DisplayColorSettings.Instance != null) DisplayColorSettings.Instance.SetSceneBrightnessOffset(0f);
        SceneManager.LoadScene("main");
    }
    public void ReturnToMenu()
    {
        if (DisplayColorSettings.Instance != null) DisplayColorSettings.Instance.SetSceneBrightnessOffset(0f);
        SceneManager.LoadScene("offline");
    }
    private void OnDestroy()
    {
        if (retryButton != null) retryButton.onClick.RemoveListener(Restart);
        if (menuButton != null) menuButton.onClick.RemoveListener(ReturnToMenu);
        if (runtimeFont != null && runtimeFont.name != "Arial") Destroy(runtimeFont);
    }
}
