#if UNITY_EDITOR
using System.IO;
using GameLogic.Cultivation.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class CultivationUIPhase79MainRouteRestoreGenerator
{
    private const string TemplateDir = "Assets/AssetRaw/UIRaw/Theme/Cultivation/Templates/SteamDemo";
    private const string SourcePrefabPath = TemplateDir + "/Steam_MainRoute_Phase78B.prefab";
    private const string RestorePrefabPath = TemplateDir + "/Steam_MainRoute_Restore.prefab";
    private const string RestorePrefabName = "Steam_MainRoute_Restore";
    private const string ScreenshotName = "Steam_MainRoute_Restore.png";
    private const string FontPath = "Assets/AssetRaw/Fonts/NotoSansCJKsc-VF.ttf";

    private static Font _font;

    [MenuItem("Codex/Cultivation UI/Phase79/Restore MainRoute Visual Baseline And Screenshot")]
    public static void RestoreMainRouteAndScreenshot()
    {
        GenerateRestorePrefab();
        CaptureRestoreScreenshot();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Phase79 MainRoute visual rollback generated.");
    }

    [MenuItem("Codex/Cultivation UI/Phase79/Generate Restore MainRoute Prefab")]
    public static void GenerateRestorePrefab()
    {
        var source = AssetDatabase.LoadAssetAtPath<GameObject>(SourcePrefabPath);
        if (source == null)
        {
            Debug.LogError("Missing Phase78B source prefab: " + SourcePrefabPath);
            return;
        }

        Directory.CreateDirectory(ToAbsolute(TemplateDir));

        var instance = (GameObject)PrefabUtility.InstantiatePrefab(source);
        instance.name = RestorePrefabName;
        PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        EnsureCanvas(instance);
        EnsureDemoBinder(instance);
        ApplyLossControlFixes(instance.transform);

        PrefabUtility.SaveAsPrefabAsset(instance, RestorePrefabPath);
        Object.DestroyImmediate(instance);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Generated restore MainRoute prefab: " + RestorePrefabPath);
    }

    [MenuItem("Codex/Cultivation UI/Phase79/Capture Restore MainRoute Screenshot")]
    public static string CaptureRestoreScreenshot()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(RestorePrefabPath);
        if (prefab == null)
        {
            GenerateRestorePrefab();
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(RestorePrefabPath);
        }

        if (prefab == null)
        {
            Debug.LogError("Missing restore prefab: " + RestorePrefabPath);
            return string.Empty;
        }

        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.name = RestorePrefabName + "_CaptureInstance";
        instance.hideFlags = HideFlags.HideAndDontSave;

        ApplyLossControlFixes(instance.transform);
        var binder = instance.GetComponent<SteamMainRouteDemoBinder>();
        if (binder != null)
        {
            binder.Bind(SteamMainRouteDemoData.Create());
        }

        var cameraObject = new GameObject(RestorePrefabName + "_CaptureCamera");
        cameraObject.hideFlags = HideFlags.HideAndDontSave;
        var camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Hex("#050908");
        camera.orthographic = true;
        camera.orthographicSize = 540;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 100f;
        camera.transform.position = new Vector3(0, 0, -10);

        var canvas = instance.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = camera;
        canvas.planeDistance = 10;
        canvas.sortingOrder = 10;

        var rt = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32);
        var previousTarget = camera.targetTexture;
        var previousActive = RenderTexture.active;
        camera.targetTexture = rt;
        RenderTexture.active = rt;
        Canvas.ForceUpdateCanvases();
        camera.Render();

        var texture = new Texture2D(1920, 1080, TextureFormat.RGBA32, false);
        texture.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
        texture.Apply();

        var screenshotDir = Path.Combine(GetRepositoryRoot(), "Doc", "UI截图验收");
        Directory.CreateDirectory(screenshotDir);
        var screenshotPath = Path.Combine(screenshotDir, ScreenshotName);
        File.WriteAllBytes(screenshotPath, texture.EncodeToPNG());

        camera.targetTexture = previousTarget;
        RenderTexture.active = previousActive;
        Object.DestroyImmediate(texture);
        Object.DestroyImmediate(rt);
        Object.DestroyImmediate(instance);
        Object.DestroyImmediate(cameraObject);

        Debug.Log("Captured Phase79 restore MainRoute screenshot: " + screenshotPath);
        return screenshotPath;
    }

    private static void EnsureCanvas(GameObject root)
    {
        var rect = root.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            rect.localScale = Vector3.one;
        }

        var canvas = root.GetComponent<Canvas>() ?? root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;

        var scaler = root.GetComponent<CanvasScaler>() ?? root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        if (root.GetComponent<GraphicRaycaster>() == null)
        {
            root.AddComponent<GraphicRaycaster>();
        }
    }

    private static void EnsureDemoBinder(GameObject root)
    {
        if (root.GetComponent<SteamMainRouteDemoBinder>() == null)
        {
            root.AddComponent<SteamMainRouteDemoBinder>();
        }
    }

    private static void ApplyLossControlFixes(Transform root)
    {
        foreach (var text in root.GetComponentsInChildren<Text>(true))
        {
            text.font = LoadFont();
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.resizeTextForBestFit = false;
        }

        FixTopResource(root, "气血", -510f, -390f);
        FixTopResource(root, "灵力", -200f, -80f);
        FixTopResource(root, "灵石", 130f, 250f);

        FixCard(root, "清风剑诀");
        FixCard(root, "幻影步");
        FixCard(root, "混元归一诀");

        FixStatusTag(root, "清心", -825f);
        FixStatusTag(root, "御剑", -730f);
        FixStatusTag(root, "灵动", -635f);

        SetAnchoredPosition(root, "Bottom_PrimaryLabel", new Vector2(365f, -486f));
        SetSize(root, "Bottom_PrimaryLabel", new Vector2(320f, 62f));
        SetAnchoredPosition(root, "Bottom_PrimaryIcon", new Vector2(610f, -486f));
    }

    private static void FixTopResource(Transform root, string label, float iconX, float textX)
    {
        SetAnchoredPosition(root, "Top_" + label + "_Icon", new Vector2(iconX, -36f));
        SetSize(root, "Top_" + label + "_Icon", new Vector2(32f, 44f));

        var text = FindText(root, "Top_" + label);
        if (text != null)
        {
            text.fontSize = 24;
            text.alignment = TextAnchor.MiddleLeft;
        }
        SetAnchoredPosition(root, "Top_" + label, new Vector2(textX, -36f));
        SetSize(root, "Top_" + label, new Vector2(190f, 46f));
        SetAnchoredPosition(root, "Top_" + label + "_BarBack", new Vector2(textX + 56f, -61f));
        SetAnchoredPosition(root, "Top_" + label + "_BarFill", new Vector2(textX + 32f, -61f));
    }

    private static void FixCard(Transform root, string title)
    {
        var titleText = FindText(root, "Right_CardTitle_" + title);
        if (titleText != null)
        {
            titleText.fontSize = title.Length > 4 ? 24 : 26;
            titleText.alignment = TextAnchor.MiddleLeft;
        }
        SetSize(root, "Right_CardTitle_" + title, new Vector2(186f, 40f));
        SetAnchoredPosition(root, "Right_CardTitle_" + title, GetCurrentPosition(root, "Right_CardTitle_" + title) + new Vector2(-12f, 0f));

        var desc = FindText(root, "Right_Desc_" + title);
        if (desc != null)
        {
            desc.fontSize = 16;
            desc.lineSpacing = 0.86f;
            desc.alignment = TextAnchor.UpperLeft;
            desc.horizontalOverflow = HorizontalWrapMode.Wrap;
            desc.verticalOverflow = VerticalWrapMode.Truncate;
        }
        SetSize(root, "Right_Desc_" + title, new Vector2(286f, 72f));
    }

    private static void FixStatusTag(Transform root, string name, float x)
    {
        SetAnchoredPosition(root, "Left_StatusTag_" + name, new Vector2(x, -826f));
        SetSize(root, "Left_StatusTag_" + name, new Vector2(62f, 112f));
        SetAnchoredPosition(root, "Left_StatusGlyph_" + name, new Vector2(x, -834f));
        SetSize(root, "Left_StatusGlyph_" + name, new Vector2(58f, 72f));
        SetAnchoredPosition(root, "Left_StatusDays_" + name, new Vector2(x, -914f));
        SetSize(root, "Left_StatusDays_" + name, new Vector2(62f, 38f));
    }

    private static Text FindText(Transform root, string name)
    {
        var child = FindChild(root, name);
        return child != null ? child.GetComponent<Text>() : null;
    }

    private static Vector2 GetCurrentPosition(Transform root, string name)
    {
        var rect = FindRect(root, name);
        return rect != null ? rect.anchoredPosition : Vector2.zero;
    }

    private static void SetAnchoredPosition(Transform root, string name, Vector2 position)
    {
        var rect = FindRect(root, name);
        if (rect != null)
        {
            rect.anchoredPosition = position;
        }
    }

    private static void SetSize(Transform root, string name, Vector2 size)
    {
        var rect = FindRect(root, name);
        if (rect != null)
        {
            rect.sizeDelta = size;
        }
    }

    private static RectTransform FindRect(Transform root, string name)
    {
        var child = FindChild(root, name);
        return child != null ? child.GetComponent<RectTransform>() : null;
    }

    private static Transform FindChild(Transform root, string name)
    {
        if (root.name == name)
        {
            return root;
        }

        foreach (Transform child in root)
        {
            var found = FindChild(child, name);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private static Font LoadFont()
    {
        if (_font != null)
        {
            return _font;
        }

        _font = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
        return _font != null ? _font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    private static Color Hex(string value)
    {
        ColorUtility.TryParseHtmlString(value, out var color);
        return color;
    }

    private static string ToAbsolute(string assetPath)
    {
        return Path.Combine(Directory.GetParent(Application.dataPath).FullName, assetPath.Replace("/", Path.DirectorySeparatorChar.ToString()));
    }

    private static string GetRepositoryRoot()
    {
        return Directory.GetParent(Directory.GetParent(Application.dataPath).FullName).FullName;
    }
}
#endif
