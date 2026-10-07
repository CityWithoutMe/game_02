using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

/// <summary>Saved display calibration shared by the menu and later scenes.</summary>
public sealed class DisplayColorSettings : MonoBehaviour
{
    private const string BrightnessKey = "Display.BrightnessEV";
    private const string ContrastKey = "Display.Contrast";
    private const string GammaKey = "Display.Gamma";

    public static DisplayColorSettings Instance { get; private set; }
    public float Brightness { get; private set; }
    public float SceneBrightnessOffset { get; private set; }
    public float Contrast { get; private set; }
    public float Gamma { get; private set; }
    public Material UiMaterial { get; private set; }

    private VolumeProfile profile;
    private ColorAdjustments colorAdjustments;
    private LiftGammaGain liftGammaGain;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null) return;
        GameObject owner = new GameObject("Display Color Settings");
        DontDestroyOnLoad(owner);
        owner.AddComponent<DisplayColorSettings>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Brightness = Mathf.Clamp(PlayerPrefs.GetFloat(BrightnessKey, 0f), -2f, 2f);
        Contrast = Mathf.Clamp(PlayerPrefs.GetFloat(ContrastKey, 0f), -50f, 50f);
        Gamma = Mathf.Clamp(PlayerPrefs.GetFloat(GammaKey, 1f), 0.5f, 2f);

        profile = ScriptableObject.CreateInstance<VolumeProfile>();
        colorAdjustments = profile.Add<ColorAdjustments>(true);
        liftGammaGain = profile.Add<LiftGammaGain>(true);

        Volume volume = gameObject.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 100f;
        volume.weight = 1f;
        volume.sharedProfile = profile;

        Shader uiShader = Resources.Load<Shader>("OfflineDisplayCorrection");
        if (uiShader != null)
            UiMaterial = new Material(uiShader) { name = "Saved Display Correction (UI)" };
        else
            Debug.LogError("OfflineDisplayCorrection shader is missing from Resources.", this);

        Apply();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // The ending glow belongs to one scene; never persist it in calibration.
        SceneBrightnessOffset = 0f;
        Apply();
        foreach (Camera camera in Camera.allCameras)
        {
            if (camera == null) continue;
            UniversalAdditionalCameraData data = camera.GetUniversalAdditionalCameraData();
            data.renderPostProcessing = true;
            data.volumeLayerMask |= 1 << gameObject.layer;
        }
    }

    public void SetValues(float brightness, float contrast, float gamma)
    {
        Brightness = Mathf.Clamp(brightness, -2f, 2f);
        Contrast = Mathf.Clamp(contrast, -50f, 50f);
        Gamma = Mathf.Clamp(gamma, 0.5f, 2f);
        Apply();
        PlayerPrefs.SetFloat(BrightnessKey, Brightness);
        PlayerPrefs.SetFloat(ContrastKey, Contrast);
        PlayerPrefs.SetFloat(GammaKey, Gamma);
    }

    public void SetSceneBrightnessOffset(float offset)
    {
        SceneBrightnessOffset = Mathf.Clamp(offset, 0f, 2f);
        Apply();
    }

    public void ResetValues()
    {
        SetValues(0f, 0f, 1f);
    }

    public void Save()
    {
        PlayerPrefs.Save();
    }

    private void Apply()
    {
        if (colorAdjustments != null)
        {
            colorAdjustments.postExposure.Override(Mathf.Clamp(Brightness + SceneBrightnessOffset, -2f, 3f));
            colorAdjustments.contrast.Override(Contrast);
        }
        if (liftGammaGain != null)
        {
            // URP converts the trackball W channel to a power of 1 / (1 + W).
            // A displayed gamma of 1 is neutral.
            liftGammaGain.gamma.Override(new Vector4(1f, 1f, 1f, Gamma - 1f));
        }
        if (UiMaterial != null)
        {
            UiMaterial.SetFloat("_Brightness", Brightness);
            UiMaterial.SetFloat("_Contrast", Contrast);
            UiMaterial.SetFloat("_Gamma", Gamma);
        }
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused) Save();
    }

    private void OnApplicationQuit()
    {
        Save();
    }

    private void OnDestroy()
    {
        if (Instance != this) return;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (UiMaterial != null) Destroy(UiMaterial);
        if (profile != null) Destroy(profile);
        Instance = null;
    }
}
