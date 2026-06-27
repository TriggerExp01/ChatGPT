#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

public static class CultivationUIGoldenSampleGenerator
{
    private const string ThemeRoot = "Assets/AssetRaw/UIRaw/Theme/Cultivation";
    private const string BackgroundDir = ThemeRoot + "/Background";
    private const string PanelDir = ThemeRoot + "/Panel";
    private const string ButtonDir = ThemeRoot + "/Button";
    private const string CardDir = ThemeRoot + "/Card";
    private const string RouteNodeDir = ThemeRoot + "/RouteNode";
    private const string IconFrameDir = ThemeRoot + "/IconFrame";
    private const string DividerDir = ThemeRoot + "/Divider";
    private const string DecorationDir = ThemeRoot + "/Decoration";
    private const string PopupDir = ThemeRoot + "/Popup";
    private const string GeneratedDir = ThemeRoot + "/Generated";
    private const string TemplateDir = ThemeRoot + "/Templates";
    private const string FontDir = "Assets/AssetRaw/Fonts";
    private const string FontSourcePath = FontDir + "/NotoSansCJKsc-VF.ttf";
    private const string TmpFontAssetPath = FontDir + "/NotoSansCJKsc_Phase77A SDF.asset";
    private const string TmpSdfShaderName = "TextMeshPro/Distance Field";
    private const string PrefabPath = TemplateDir + "/MainRunWindow_Golden.prefab";
    private const string ScreenshotFileName = "MainRunWindow_Golden_Phase77B.png";

    private static TMP_FontAsset _tmpFont;
    private static Font _uiFont;

    [MenuItem("Codex/Cultivation UI/Generate Golden Sample And Screenshot")]
    public static void GenerateGoldenSampleAndScreenshot()
    {
        GenerateThemeAssets();
        GenerateGoldenPrefab();
        CaptureGoldenSample();
        AssetDatabase.Refresh();
        Debug.Log("Phase77B Cultivation UI golden sample generated and captured.");
    }

    [MenuItem("Codex/Cultivation UI/Generate Golden Sample Prefab")]
    public static void GenerateGoldenPrefabMenu()
    {
        GenerateThemeAssets();
        GenerateGoldenPrefab();
    }

    [MenuItem("Codex/Cultivation UI/Capture Golden Sample")]
    public static void CaptureGoldenSampleMenu()
    {
        CaptureGoldenSample();
    }

    public static void GenerateThemeAssets()
    {
        EnsureDirectories();

        CreateMountainMistBackground(Asset("BG_Cultivation_MountainMist.png"));
        CreateRunePatternTexture(Asset("BG_Cultivation_RunePattern.png"));
        CreateVignetteTexture(Asset("BG_Cultivation_Vignette.png"));

        CreatePanelTexture(Asset("Panel_Main_JadeThin.png"), 360, 360, Hex("#16372F"), Hex("#071A17"), Hex("#B5914E"), 4, 22, PanelMaterial.Jade, 0.92f);
        CreatePanelTexture(Asset("Panel_Secondary_Parchment.png"), 360, 360, Hex("#2D281B"), Hex("#15130E"), Hex("#75613A"), 3, 18, PanelMaterial.Parchment, 0.9f);
        CreatePanelTexture(Asset("Panel_Map_Mist.png"), 480, 360, Hex("#122C29"), Hex("#081815"), Hex("#496A58"), 2, 18, PanelMaterial.Map, 0.82f);
        CreatePanelTexture(Asset("Panel_Bamboo_Tag.png"), 300, 92, Hex("#2E4635"), Hex("#14241E"), Hex("#8C7444"), 2, 12, PanelMaterial.Bamboo, 0.95f);
        CreatePanelTexture(Asset("Panel_Scroll_Note.png"), 420, 140, Hex("#31281A"), Hex("#17120C"), Hex("#8C6530"), 2, 16, PanelMaterial.Parchment, 0.92f);
        CreatePanelTexture(Asset("Panel_Top_JadePlaque.png"), 300, 86, Hex("#20483E"), Hex("#0E2420"), Hex("#7BC8B0"), 2, 16, PanelMaterial.Jade, 0.9f);
        CreatePanelTexture(Asset("Panel_Bottom_Transparent.png"), 420, 120, Hex("#122822"), Hex("#071310"), Hex("#4F6F5D"), 1, 18, PanelMaterial.Jade, 0.72f);

        CreateButtonTexture(Asset("Button_Primary_Scroll_Normal.png"), 420, 104, Hex("#D8B25D"), Hex("#8B6222"), Hex("#F6E0A0"), false);
        CreateButtonTexture(Asset("Button_Primary_Scroll_Pressed.png"), 420, 104, Hex("#A4762E"), Hex("#5A3A13"), Hex("#C79A45"), false);
        CreateButtonTexture(Asset("Button_Primary_Scroll_Disabled.png"), 420, 104, Hex("#58655C"), Hex("#303930"), Hex("#768174"), true);
        CreateButtonTexture(Asset("Button_Secondary_Jade_Normal.png"), 300, 78, Hex("#255449"), Hex("#0D2923"), Hex("#79C2AA"), false);
        CreateButtonTexture(Asset("Button_Secondary_Jade_Pressed.png"), 300, 78, Hex("#173A33"), Hex("#081A17"), Hex("#55A08B"), false);
        CreateButtonTexture(Asset("Button_Secondary_Jade_Disabled.png"), 300, 78, Hex("#38483F"), Hex("#1D2823"), Hex("#596B60"), true);

        CreateCardTexture(Asset("Card_Cultivation_Common.png"), Hex("#2C3327"), Hex("#12150F"), Hex("#B79A5B"), Hex("#6B5530"), Rarity.Common);
        CreateCardTexture(Asset("Card_Cultivation_Rare.png"), Hex("#173D3B"), Hex("#071E20"), Hex("#62D1B0"), Hex("#356D64"), Rarity.Rare);
        CreateCardTexture(Asset("Card_Cultivation_Epic.png"), Hex("#332340"), Hex("#130B1B"), Hex("#C58ED8"), Hex("#734C82"), Rarity.Epic);
        CreateCardArtTexture(Asset("Card_ArtPlaceholder_Sword.png"), CardArtKind.Sword);
        CreateCardArtTexture(Asset("Card_ArtPlaceholder_Qi.png"), CardArtKind.Qi);
        CreateCardArtTexture(Asset("Card_ArtPlaceholder_Talisman.png"), CardArtKind.Talisman);
        CreateCostOrbTexture(Asset("CostOrb_Gold.png"));

        CreateRouteMapTexture(Asset("RouteMap_MistPanel.png"));
        CreateSpiritLineTexture(Asset("RoutePath_SpiritLine.png"));
        CreateNodeIconTexture(Asset("Node_Battle_Sword.png"), NodeIconKind.Battle);
        CreateNodeIconTexture(Asset("Node_Event_Scroll.png"), NodeIconKind.Event);
        CreateNodeIconTexture(Asset("Node_Chest_Box.png"), NodeIconKind.Chest);
        CreateNodeIconTexture(Asset("Node_Rest_Meditation.png"), NodeIconKind.Rest);
        CreateNodeIconTexture(Asset("Node_Market_Pavilion.png"), NodeIconKind.Market);
        CreateCurrentGlowTexture(Asset("Node_Current_Glow.png"));

        CreateJadeDiscTexture(Asset("IconFrame_Jade.png"));
        CreateDividerTexture(Asset("Divider_InkGold.png"), 420, 20);
        CreateCornerTexture(Asset("Corner_Decoration_ThinGold.png"));
        CreatePanelTexture(Asset("Popup_DarkJade_GoldBorder.png"), 520, 360, Hex("#102A25"), Hex("#061412"), Hex("#C79A45"), 4, 24, PanelMaterial.Jade, 0.95f);
        CreateMaskTexture(Asset("Mask_Dark.png"));

        CopyGeneratedAsset("BG_Cultivation_MountainMist.png", BackgroundDir);
        CopyGeneratedAsset("BG_Cultivation_RunePattern.png", BackgroundDir);
        CopyGeneratedAsset("BG_Cultivation_Vignette.png", BackgroundDir);
        CopyGeneratedAsset("Panel_Main_JadeThin.png", PanelDir);
        CopyGeneratedAsset("Panel_Secondary_Parchment.png", PanelDir);
        CopyGeneratedAsset("Panel_Map_Mist.png", PanelDir);
        CopyGeneratedAsset("Panel_Bamboo_Tag.png", PanelDir);
        CopyGeneratedAsset("Panel_Scroll_Note.png", PanelDir);
        CopyGeneratedAsset("Panel_Top_JadePlaque.png", PanelDir);
        CopyGeneratedAsset("Panel_Bottom_Transparent.png", PanelDir);
        CopyGeneratedAsset("Button_Primary_Scroll_Normal.png", ButtonDir);
        CopyGeneratedAsset("Button_Primary_Scroll_Pressed.png", ButtonDir);
        CopyGeneratedAsset("Button_Primary_Scroll_Disabled.png", ButtonDir);
        CopyGeneratedAsset("Button_Secondary_Jade_Normal.png", ButtonDir);
        CopyGeneratedAsset("Button_Secondary_Jade_Pressed.png", ButtonDir);
        CopyGeneratedAsset("Button_Secondary_Jade_Disabled.png", ButtonDir);
        CopyGeneratedAsset("Card_Cultivation_Common.png", CardDir);
        CopyGeneratedAsset("Card_Cultivation_Rare.png", CardDir);
        CopyGeneratedAsset("Card_Cultivation_Epic.png", CardDir);
        CopyGeneratedAsset("Card_ArtPlaceholder_Sword.png", CardDir);
        CopyGeneratedAsset("Card_ArtPlaceholder_Qi.png", CardDir);
        CopyGeneratedAsset("Card_ArtPlaceholder_Talisman.png", CardDir);
        CopyGeneratedAsset("CostOrb_Gold.png", CardDir);
        CopyGeneratedAsset("RouteMap_MistPanel.png", RouteNodeDir);
        CopyGeneratedAsset("RoutePath_SpiritLine.png", RouteNodeDir);
        CopyGeneratedAsset("Node_Battle_Sword.png", RouteNodeDir);
        CopyGeneratedAsset("Node_Event_Scroll.png", RouteNodeDir);
        CopyGeneratedAsset("Node_Chest_Box.png", RouteNodeDir);
        CopyGeneratedAsset("Node_Rest_Meditation.png", RouteNodeDir);
        CopyGeneratedAsset("Node_Market_Pavilion.png", RouteNodeDir);
        CopyGeneratedAsset("Node_Current_Glow.png", RouteNodeDir);
        CopyGeneratedAsset("IconFrame_Jade.png", IconFrameDir);
        CopyGeneratedAsset("Divider_InkGold.png", DividerDir);
        CopyGeneratedAsset("Corner_Decoration_ThinGold.png", DecorationDir);
        CopyGeneratedAsset("Popup_DarkJade_GoldBorder.png", PopupDir);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    public static void GenerateGoldenPrefab()
    {
        GenerateThemeAssets();

        var root = CreateRoot();
        BuildMainRunWindow(root.transform);

        Directory.CreateDirectory(ToAbsolute(TemplateDir));
        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        UnityEngine.Object.DestroyImmediate(root);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Generated Phase77B Cultivation golden prefab: {PrefabPath}");
    }

    public static string CaptureGoldenSample()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null)
        {
            GenerateGoldenPrefab();
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        }

        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.name = "MainRunWindow_Golden_Phase77B_CaptureInstance";
        instance.hideFlags = HideFlags.HideAndDontSave;

        var cameraObject = new GameObject("CultivationGoldenSampleCamera_Phase77B");
        cameraObject.hideFlags = HideFlags.HideAndDontSave;
        var camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Hex("#06100F");
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

        foreach (var text in instance.GetComponentsInChildren<Text>(true))
        {
            text.font = LoadUiFont();
        }

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
        var screenshotPath = Path.Combine(screenshotDir, ScreenshotFileName);
        File.WriteAllBytes(screenshotPath, texture.EncodeToPNG());

        camera.targetTexture = previousTarget;
        RenderTexture.active = previousActive;
        UnityEngine.Object.DestroyImmediate(texture);
        UnityEngine.Object.DestroyImmediate(rt);
        UnityEngine.Object.DestroyImmediate(instance);
        UnityEngine.Object.DestroyImmediate(cameraObject);

        Debug.Log($"Captured Phase77B Cultivation golden sample: {screenshotPath}");
        return screenshotPath;
    }

    private static GameObject CreateRoot()
    {
        var root = new GameObject("MainRunWindow_Golden", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var rect = root.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.sizeDelta = new Vector2(1920, 1080);

        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;

        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        return root;
    }

    private static void BuildMainRunWindow(Transform root)
    {
        var bg = AddImage("Background_MountainMist", root, LoadSprite("BG_Cultivation_MountainMist.png"), RectSpec.Center(Vector2.zero, new Vector2(1920, 1080)));
        bg.raycastTarget = false;

        var rune = AddImage("Background_RunePattern", root, LoadSprite("BG_Cultivation_RunePattern.png"), RectSpec.Center(new Vector2(60, 30), new Vector2(1120, 760)));
        rune.color = new Color(0.65f, 1f, 0.86f, 0.22f);
        rune.raycastTarget = false;

        var vignette = AddImage("Background_Vignette", root, LoadSprite("BG_Cultivation_Vignette.png"), RectSpec.Center(Vector2.zero, new Vector2(1920, 1080)));
        vignette.raycastTarget = false;

        AddCornerDecorations(root);
        AddTopInfoBar(root);
        AddLeftStatusPanel(root);
        AddRoutePanel(root);
        AddCardPreviewPanel(root);
        AddBottomActions(root);
        AddEventPopupSample(root);
    }

    private static void AddCornerDecorations(Transform root)
    {
        var positions = new[]
        {
            new Vector2(-878, 466),
            new Vector2(878, 466),
            new Vector2(-878, -466),
            new Vector2(878, -466)
        };

        for (var i = 0; i < positions.Length; i++)
        {
            var corner = AddImage("CornerDecoration_ThinGold_" + i, root, LoadSprite("Corner_Decoration_ThinGold.png"), RectSpec.Center(positions[i], new Vector2(74, 74)));
            corner.raycastTarget = false;
            corner.color = new Color(1f, 0.82f, 0.42f, 0.42f);
            if (i == 1)
            {
                corner.rectTransform.localScale = new Vector3(-1f, 1f, 1f);
            }
            else if (i == 2)
            {
                corner.rectTransform.localScale = new Vector3(1f, -1f, 1f);
            }
            else if (i == 3)
            {
                corner.rectTransform.localScale = new Vector3(-1f, -1f, 1f);
            }
        }
    }

    private static void AddTopInfoBar(Transform root)
    {
        var titlePlaque = AddPanel("Top_Title_JadeScroll", root, RectSpec.Center(new Vector2(-675, 473), new Vector2(390, 78)), "Panel_Top_JadePlaque.png");
        AddText("Title", titlePlaque.transform, "云隐散修", 32, Hex("#F1D486"), TextAlignmentOptions.Center, RectSpec.Stretch(new Vector2(24, 4), new Vector2(24, 4)), FontStyles.Bold);
        AddImage("TitleDivider", titlePlaque.transform, LoadSprite("Divider_InkGold.png"), RectSpec.Bottom(new Vector2(0, 10), new Vector2(240, 10))).color = new Color(0.95f, 0.72f, 0.34f, 0.55f);

        AddResourcePlaque(root, "境界", "炼气三层", new Vector2(-333, 473), Hex("#D9B56A"), ResourceStyle.Text);
        AddResourcePlaque(root, "生命", "120/120", new Vector2(-103, 473), Hex("#CF6559"), ResourceStyle.Health);
        AddResourcePlaque(root, "灵力", "80/80", new Vector2(127, 473), Hex("#58BFA4"), ResourceStyle.Mana);
        AddResourcePlaque(root, "灵石", "320", new Vector2(340, 473), Hex("#E4C36E"), ResourceStyle.Text);
        AddResourcePlaque(root, "位置", "秘境一层", new Vector2(584, 473), Hex("#E8DFC6"), ResourceStyle.Text, 220);
        AddResourcePlaque(root, "天数", "第 3 日", new Vector2(805, 473), Hex("#E8DFC6"), ResourceStyle.Text, 160);
    }

    private static void AddResourcePlaque(Transform parent, string label, string value, Vector2 position, Color accent, ResourceStyle style, float width = 190)
    {
        var block = AddPanel("Resource_" + label, parent, RectSpec.Center(position, new Vector2(width, 66)), "Panel_Top_JadePlaque.png");
        AddText(label + "_Label", block.transform, label, 14, Hex("#C8B890"), TextAlignmentOptions.Left, RectSpec.Stretch(new Vector2(18, 7), new Vector2(width * 0.55f, 34)));
        AddText(label + "_Value", block.transform, value, 20, accent, TextAlignmentOptions.Right, RectSpec.Stretch(new Vector2(width * 0.34f, 7), new Vector2(18, 32)), FontStyles.Bold);

        if (style == ResourceStyle.Health || style == ResourceStyle.Mana)
        {
            var bar = AddImage("ThinBar_Back", block.transform, LoadSprite("Panel_Bottom_Transparent.png"), RectSpec.Bottom(new Vector2(0, 10), new Vector2(width - 36, 10)));
            bar.type = Image.Type.Sliced;
            bar.color = new Color(0.05f, 0.08f, 0.07f, 0.72f);
            var fill = AddImage("ThinBar_Fill", block.transform, LoadSprite("Divider_InkGold.png"), RectSpec.Bottom(new Vector2(style == ResourceStyle.Health ? -9 : -14, 10), new Vector2(style == ResourceStyle.Health ? width - 58 : width - 68, 6)));
            fill.color = style == ResourceStyle.Health ? new Color(0.78f, 0.24f, 0.2f, 0.86f) : new Color(0.27f, 0.9f, 0.74f, 0.86f);
        }
    }

    private static void AddLeftStatusPanel(Transform root)
    {
        var panel = AddPanel("Left_CultivatorJadeArchive", root, RectSpec.Center(new Vector2(-725, 86), new Vector2(430, 720)), "Panel_Main_JadeThin.png");
        AddText("Header", panel.transform, "修士玉简", 30, Hex("#F0D78A"), TextAlignmentOptions.Center, RectSpec.Top(new Vector2(0, -30), new Vector2(340, 42)), FontStyles.Bold);
        AddImage("HeaderDivider", panel.transform, LoadSprite("Divider_InkGold.png"), RectSpec.Top(new Vector2(0, -78), new Vector2(300, 12))).color = new Color(0.9f, 0.7f, 0.38f, 0.48f);

        var avatar = AddPanel("Avatar_JadeDisc", panel.transform, RectSpec.Top(new Vector2(0, -178), new Vector2(178, 178)), "IconFrame_Jade.png");
        AddImage("AvatarRune", avatar.transform, LoadSprite("BG_Cultivation_RunePattern.png"), RectSpec.Center(Vector2.zero, new Vector2(142, 142))).color = new Color(0.43f, 0.93f, 0.78f, 0.18f);
        AddText("AvatarGlyph", avatar.transform, "修", 70, Hex("#D9B56A"), TextAlignmentOptions.Center, RectSpec.Stretch(Vector2.zero, Vector2.zero), FontStyles.Bold);

        AddStatTag(panel.transform, "境界", "炼气三层", new Vector2(-86, -300), true);
        AddStatTag(panel.transform, "心境", "平稳", new Vector2(104, -300), false);
        AddStatTag(panel.transform, "体魄", "凡骨", new Vector2(-86, -354), false);
        AddStatTag(panel.transform, "灵根", "木火双灵根", new Vector2(104, -354), true);

        AddMetricRow(panel.transform, "气血", "120 / 120", -424, Hex("#C95E54"), 1f);
        AddMetricRow(panel.transform, "灵力", "80 / 80", -468, Hex("#58BFA4"), 0.86f);

        AddText("BuffTitle", panel.transform, "护身符箓", 21, Hex("#D9B56A"), TextAlignmentOptions.Left, RectSpec.Top(new Vector2(-116, -492), new Vector2(160, 30)), FontStyles.Bold);
        var buffNames = new[] { "护", "悟", "息", "运" };
        for (var i = 0; i < buffNames.Length; i++)
        {
            var icon = AddPanel("Buff_Talisman_" + buffNames[i], panel.transform, RectSpec.Top(new Vector2(-122 + i * 80, -542), new Vector2(50, 68)), "Panel_Bamboo_Tag.png");
            icon.color = i == 1 ? new Color(0.62f, 0.98f, 0.84f, 0.92f) : new Color(1f, 0.92f, 0.7f, 0.88f);
            AddText("Glyph", icon.transform, buffNames[i], 22, Hex("#F7E7B7"), TextAlignmentOptions.Center, RectSpec.Stretch(new Vector2(0, 5), new Vector2(0, 5)), FontStyles.Bold);
        }

        var stateBox = AddPanel("CultivationNote_Scroll", panel.transform, RectSpec.Bottom(new Vector2(0, 28), new Vector2(362, 82)), "Panel_Scroll_Note.png");
        AddText("Note", stateBox.transform, "当前状态：秘境探索中。灵息环绕，适合继续前进；也需留意心境波动。", 17, Hex("#E8DFC6"), TextAlignmentOptions.TopLeft, RectSpec.Stretch(new Vector2(22, 12), new Vector2(22, 12)));
    }

    private static void AddStatTag(Transform parent, string label, string value, Vector2 position, bool accent)
    {
        var tag = AddPanel("StatTag_" + label, parent, RectSpec.Top(position, new Vector2(168, 48)), "Panel_Bamboo_Tag.png");
        tag.color = accent ? new Color(0.74f, 1f, 0.89f, 0.9f) : new Color(0.92f, 0.78f, 0.52f, 0.82f);
        AddText(label + "_Label", tag.transform, label, 14, Hex("#D9B56A"), TextAlignmentOptions.Left, RectSpec.Stretch(new Vector2(14, 5), new Vector2(92, 5)), FontStyles.Bold);
        AddText(label + "_Value", tag.transform, value, 16, Hex("#F9EAC7"), TextAlignmentOptions.Right, RectSpec.Stretch(new Vector2(54, 5), new Vector2(12, 5)), FontStyles.Bold);
    }

    private static void AddMetricRow(Transform parent, string label, string value, float y, Color fillColor, float fillScale)
    {
        var row = CreateRect("Metric_" + label, parent, RectSpec.Top(new Vector2(0, y), new Vector2(342, 34)));
        AddText(label + "_Label", row, label, 16, Hex("#D9B56A"), TextAlignmentOptions.Left, RectSpec.Stretch(new Vector2(0, 0), new Vector2(268, 0)), FontStyles.Bold);
        var bar = AddPanel(label + "_BarBack", row, RectSpec.Center(new Vector2(52, 0), new Vector2(216, 14)), "Panel_Bottom_Transparent.png");
        bar.color = new Color(0.04f, 0.07f, 0.06f, 0.72f);
        var fill = AddImage(label + "_Fill", row, LoadSprite("Divider_InkGold.png"), RectSpec.Center(new Vector2(52 - (216 * (1f - fillScale) * 0.5f), 0), new Vector2(200 * fillScale, 8)));
        fill.color = new Color(fillColor.r, fillColor.g, fillColor.b, 0.82f);
        AddText(label + "_Value", row, value, 16, Hex("#E8DFC6"), TextAlignmentOptions.Right, RectSpec.Stretch(new Vector2(238, 0), new Vector2(0, 0)), FontStyles.Bold);
    }

    private static void AddRoutePanel(Transform root)
    {
        var panel = AddPanel("Center_MysticRouteMapPanel", root, RectSpec.Center(new Vector2(-36, 86), new Vector2(826, 720)), "Panel_Map_Mist.png");
        AddImage("RouteMapBackdrop", panel.transform, LoadSprite("RouteMap_MistPanel.png"), RectSpec.Center(new Vector2(0, -18), new Vector2(764, 558))).color = new Color(1f, 1f, 1f, 0.86f);
        AddText("Header", panel.transform, "秘境灵脉图", 32, Hex("#F0D78A"), TextAlignmentOptions.Center, RectSpec.Top(new Vector2(0, -30), new Vector2(420, 44)), FontStyles.Bold);
        AddText("SubHeader", panel.transform, "固定种子样板 / 第 3 日 / 事件节点预览", 16, Hex("#A99F86"), TextAlignmentOptions.Center, RectSpec.Top(new Vector2(0, -70), new Vector2(520, 26)));

        var points = new[]
        {
            new Vector2(-310, -80),
            new Vector2(-162, 86),
            new Vector2(12, 18),
            new Vector2(168, -104),
            new Vector2(326, 84)
        };

        for (var i = 0; i < points.Length - 1; i++)
        {
            AddRouteLine(panel.transform, points[i], points[i + 1], i < 1 ? new Color(0.88f, 0.62f, 0.26f, 0.78f) : new Color(0.33f, 0.58f, 0.5f, 0.34f));
        }

        AddRouteNode(panel.transform, "战斗", "Node_Battle_Sword.png", points[0], "已完成", NodeState.Done);
        AddRouteNode(panel.transform, "秘境", "Node_Event_Scroll.png", points[1], "当前", NodeState.Current);
        AddRouteNode(panel.transform, "宝匣", "Node_Chest_Box.png", points[2], "未到达", NodeState.Locked);
        AddRouteNode(panel.transform, "休息", "Node_Rest_Meditation.png", points[3], "未到达", NodeState.Locked);
        AddRouteNode(panel.transform, "市集", "Node_Market_Pavilion.png", points[4], "未到达", NodeState.Locked);

        var note = AddPanel("RouteNote_Scroll", panel.transform, RectSpec.Bottom(new Vector2(0, 54), new Vector2(704, 98)), "Panel_Scroll_Note.png");
        AddText("RouteNoteText", note.transform, "当前节点：古井灵光。探索可获得灵力、功法或临时状态；失败也可能损失气血。", 20, Hex("#E8DFC6"), TextAlignmentOptions.TopLeft, RectSpec.Stretch(new Vector2(24, 16), new Vector2(24, 14)));
    }

    private static void AddRouteLine(Transform parent, Vector2 from, Vector2 to, Color color)
    {
        var delta = to - from;
        var line = AddImage("SpiritRouteLine", parent, LoadSprite("RoutePath_SpiritLine.png"), RectSpec.Center((from + to) * 0.5f, new Vector2(delta.magnitude, 18)));
        line.color = color;
        line.rectTransform.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
    }

    private enum NodeState
    {
        Done,
        Current,
        Locked
    }

    private static void AddRouteNode(Transform parent, string label, string spriteName, Vector2 position, string status, NodeState state)
    {
        if (state == NodeState.Current)
        {
            var glow = AddImage("CurrentGlow_" + label, parent, LoadSprite("Node_Current_Glow.png"), RectSpec.Center(position, new Vector2(156, 156)));
            glow.color = new Color(0.9f, 0.78f, 0.36f, 0.88f);
            glow.raycastTarget = false;
        }

        var node = AddImage("RouteNode_" + label, parent, LoadSprite(spriteName), RectSpec.Center(position, new Vector2(116, 116)));
        node.color = state == NodeState.Locked ? new Color(0.5f, 0.56f, 0.5f, 0.58f) : Color.white;

        var tag = AddPanel("NodeLabel_" + label, parent, RectSpec.Center(position + new Vector2(0, -78), new Vector2(118, 34)), "Panel_Bamboo_Tag.png");
        tag.color = state == NodeState.Current ? new Color(0.74f, 1f, 0.86f, 0.92f) : state == NodeState.Done ? new Color(1f, 0.74f, 0.38f, 0.72f) : new Color(0.45f, 0.5f, 0.43f, 0.48f);
        AddText("Label", tag.transform, label + "  " + status, 15, state == NodeState.Locked ? Hex("#A99F86") : Hex("#F0D78A"), TextAlignmentOptions.Center, RectSpec.Stretch(new Vector2(6, 2), new Vector2(6, 2)), FontStyles.Bold);
    }

    private static void AddCardPreviewPanel(Transform root)
    {
        var panel = AddPanel("Right_ManualCardTray", root, RectSpec.Center(new Vector2(680, 86), new Vector2(470, 720)), "Panel_Main_JadeThin.png");
        AddText("Header", panel.transform, "功法书架", 30, Hex("#F0D78A"), TextAlignmentOptions.Center, RectSpec.Top(new Vector2(0, -30), new Vector2(300, 42)), FontStyles.Bold);
        AddText("Hint", panel.transform, "奖励池样板 / 非真实掉落", 16, Hex("#A99F86"), TextAlignmentOptions.Center, RectSpec.Top(new Vector2(0, -68), new Vector2(300, 26)));
        AddImage("ShelfDivider", panel.transform, LoadSprite("Divider_InkGold.png"), RectSpec.Top(new Vector2(0, -96), new Vector2(340, 12))).color = new Color(0.9f, 0.68f, 0.32f, 0.42f);

        AddCard(panel.transform, "青木剑诀", "1", "剑诀 / 凡品", "造成 8 点伤害。\n灵力充足时获得剑意。", new Vector2(-145, 82), Rarity.Common, "Card_ArtPlaceholder_Sword.png");
        AddCard(panel.transform, "聚气术", "0", "心法 / 灵品", "恢复 12 点灵力。\n抽 1 张牌。", new Vector2(0, 82), Rarity.Rare, "Card_ArtPlaceholder_Qi.png");
        AddCard(panel.transform, "护身符", "1", "符箓 / 凡品", "获得 10 点护盾。\n本回合减伤。", new Vector2(145, 82), Rarity.Epic, "Card_ArtPlaceholder_Talisman.png");

        var reward = AddPanel("RewardPreview_Scroll", panel.transform, RectSpec.Bottom(new Vector2(0, 72), new Vector2(392, 136)), "Panel_Scroll_Note.png");
        AddText("RewardTitle", reward.transform, "可能奖励", 21, Hex("#D9B56A"), TextAlignmentOptions.Left, RectSpec.Top(new Vector2(-118, -18), new Vector2(130, 30)), FontStyles.Bold);
        AddRewardItem(reward.transform, "灵石 +45", new Vector2(-94, -14), "CostOrb_Gold.png");
        AddRewardItem(reward.transform, "随机功法", new Vector2(96, -14), "Card_ArtPlaceholder_Qi.png");
        AddRewardItem(reward.transform, "心境稳定", new Vector2(-94, -50), "Node_Rest_Meditation.png");
        AddRewardItem(reward.transform, "秘境线索", new Vector2(96, -50), "Node_Event_Scroll.png");
    }

    private static void AddCard(Transform parent, string title, string cost, string type, string desc, Vector2 position, Rarity rarity, string artSprite)
    {
        var sprite = rarity == Rarity.Epic ? "Card_Cultivation_Epic.png" : rarity == Rarity.Rare ? "Card_Cultivation_Rare.png" : "Card_Cultivation_Common.png";
        var card = AddImage("Card_" + title, parent, LoadSprite(sprite), RectSpec.Center(position, new Vector2(132, 270)));
        card.type = Image.Type.Sliced;

        var costBadge = AddImage("CostOrb", card.transform, LoadSprite("CostOrb_Gold.png"), RectSpec.Top(new Vector2(-44, -25), new Vector2(42, 42)));
        AddText("CostText", costBadge.transform, cost, 20, Hex("#2A1A08"), TextAlignmentOptions.Center, RectSpec.Stretch(Vector2.zero, Vector2.zero), FontStyles.Bold);
        AddText("Title", card.transform, title, 18, Hex("#F0D78A"), TextAlignmentOptions.Center, RectSpec.Top(new Vector2(16, -24), new Vector2(86, 36)), FontStyles.Bold);

        var art = AddImage("ArtPlaceholder", card.transform, LoadSprite(artSprite), RectSpec.Top(new Vector2(0, -91), new Vector2(94, 82)));
        art.color = Color.white;

        AddText("Type", card.transform, type, 13, rarity == Rarity.Rare ? Hex("#58BFA4") : rarity == Rarity.Epic ? Hex("#D8A0EA") : Hex("#A99F86"), TextAlignmentOptions.Center, RectSpec.Top(new Vector2(0, -145), new Vector2(108, 22)));
        AddText("Desc", card.transform, desc, 13, Hex("#E8DFC6"), TextAlignmentOptions.Top, RectSpec.Bottom(new Vector2(0, 15), new Vector2(102, 76)));
    }

    private static void AddRewardItem(Transform parent, string text, Vector2 position, string iconSprite)
    {
        var item = CreateRect("Reward_" + text, parent, RectSpec.Center(position, new Vector2(168, 38)));
        var icon = AddImage("Icon", item, LoadSprite(iconSprite), RectSpec.Left(new Vector2(19, 0), new Vector2(32, 32)));
        icon.color = new Color(1f, 1f, 1f, 0.86f);
        AddText("Text", item, text, 15, Hex("#E8DFC6"), TextAlignmentOptions.Left, RectSpec.Stretch(new Vector2(44, 0), new Vector2(4, 0)));
    }

    private static void AddBottomActions(Transform root)
    {
        var panel = AddPanel("Bottom_ActionShelf", root, RectSpec.Center(new Vector2(0, -462), new Vector2(1816, 112)), "Panel_Bottom_Transparent.png");
        panel.color = new Color(1f, 1f, 1f, 0.82f);
        AddButton(panel.transform, "Button_Bag", "背包", new Vector2(-298, 0), new Vector2(146, 54), false);
        AddButton(panel.transform, "Button_Skill", "功法", new Vector2(-122, 0), new Vector2(146, 54), false);
        AddButton(panel.transform, "Button_Settings", "设置", new Vector2(54, 0), new Vector2(146, 54), false);
        AddButton(panel.transform, "Button_Back", "返回", new Vector2(230, 0), new Vector2(146, 54), false);
        AddButton(panel.transform, "Primary_Continue", "继续前进", new Vector2(595, 0), new Vector2(314, 68), true);
        AddText("FooterNote", panel.transform, "黄金样板 / 假数据 / 用于 MainRun、Route、Event、Battle 的视觉基线", 15, Hex("#8D9B86"), TextAlignmentOptions.Left, RectSpec.Stretch(new Vector2(34, 0), new Vector2(980, 0)));
    }

    private static void AddEventPopupSample(Transform root)
    {
        var popup = CreateRect("EventPopup_StyleSample", root, RectSpec.Center(Vector2.zero, new Vector2(760, 480))).gameObject;
        AddImage("Dimmer", popup.transform, LoadSprite("Mask_Dark.png"), RectSpec.Center(Vector2.zero, new Vector2(1920, 1080))).color = new Color(0, 0, 0, 0.58f);
        var panel = AddPanel("PopupPanel", popup.transform, RectSpec.Center(Vector2.zero, new Vector2(760, 480)), "Popup_DarkJade_GoldBorder.png");
        AddText("Header", panel.transform, "秘境事件", 30, Hex("#F0D78A"), TextAlignmentOptions.Center, RectSpec.Top(new Vector2(0, -38), new Vector2(360, 46)), FontStyles.Bold);
        AddText("EventName", panel.transform, "古井灵光", 24, Hex("#58BFA4"), TextAlignmentOptions.Center, RectSpec.Top(new Vector2(0, -92), new Vector2(360, 38)), FontStyles.Bold);
        AddText("Description", panel.transform, "你在破败石阶尽头发现一口古井，井中灵光流转，似有机缘，也似有凶险。", 21, Hex("#E8DFC6"), TextAlignmentOptions.TopLeft, RectSpec.Stretch(new Vector2(64, 140), new Vector2(64, 174)));
        AddButton(panel.transform, "Option_Explore", "汲取灵气", new Vector2(-150, -142), new Vector2(210, 58), false);
        AddButton(panel.transform, "Option_Meditate", "谨慎离开", new Vector2(150, -142), new Vector2(210, 58), false);
        var reward = AddPanel("PopupRewardPreview", panel.transform, RectSpec.Bottom(new Vector2(0, 34), new Vector2(560, 70)), "Panel_Scroll_Note.png");
        AddText("RewardText", reward.transform, "奖励预览：灵石 +20 / 心境波动", 19, Hex("#D9B56A"), TextAlignmentOptions.Center, RectSpec.Stretch(new Vector2(18, 4), new Vector2(18, 4)), FontStyles.Bold);
        popup.SetActive(false);
    }

    private static void AddButton(Transform parent, string name, string label, Vector2 position, Vector2 size, bool primary)
    {
        var sprite = LoadSprite(primary ? "Button_Primary_Scroll_Normal.png" : "Button_Secondary_Jade_Normal.png");
        var image = AddImage(name, parent, sprite, RectSpec.Center(position, size));
        image.type = Image.Type.Sliced;

        var button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.SpriteSwap;
        var state = button.spriteState;
        state.pressedSprite = LoadSprite(primary ? "Button_Primary_Scroll_Pressed.png" : "Button_Secondary_Jade_Pressed.png");
        state.disabledSprite = LoadSprite(primary ? "Button_Primary_Scroll_Disabled.png" : "Button_Secondary_Jade_Disabled.png");
        button.spriteState = state;

        AddText("Label", image.transform, label, primary ? 24 : 19, primary ? Hex("#23180A") : Hex("#E8DFC6"), TextAlignmentOptions.Center, RectSpec.Stretch(Vector2.zero, Vector2.zero), FontStyles.Bold);
    }

    private static Image AddPanel(string name, Transform parent, RectSpec spec, string spriteName)
    {
        var image = AddImage(name, parent, LoadSprite(spriteName), spec);
        image.type = Image.Type.Sliced;
        image.raycastTarget = true;
        return image;
    }

    private static Image AddImage(string name, Transform parent, Sprite sprite, RectSpec spec)
    {
        var rect = CreateRect(name, parent, spec);
        var image = rect.gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.color = Color.white;
        image.raycastTarget = false;
        return image;
    }

    private static Text AddText(string name, Transform parent, string text, float size, Color color, TextAlignmentOptions alignment, RectSpec spec, FontStyles style = FontStyles.Normal)
    {
        var rect = CreateRect(name, parent, spec);
        var label = rect.gameObject.AddComponent<Text>();
        label.font = LoadUiFont();
        label.text = text;
        label.fontSize = Mathf.RoundToInt(size);
        label.color = color;
        label.alignment = ToTextAnchor(alignment);
        label.fontStyle = style == FontStyles.Bold ? FontStyle.Bold : FontStyle.Normal;
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Truncate;
        label.raycastTarget = false;

        var shadow = rect.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.55f);
        shadow.effectDistance = new Vector2(1.3f, -1.3f);

        return label;
    }

    private static TextAnchor ToTextAnchor(TextAlignmentOptions alignment)
    {
        switch (alignment)
        {
            case TextAlignmentOptions.Left:
                return TextAnchor.MiddleLeft;
            case TextAlignmentOptions.Right:
                return TextAnchor.MiddleRight;
            case TextAlignmentOptions.Top:
                return TextAnchor.UpperCenter;
            case TextAlignmentOptions.TopLeft:
                return TextAnchor.UpperLeft;
            case TextAlignmentOptions.TopRight:
                return TextAnchor.UpperRight;
            case TextAlignmentOptions.Bottom:
                return TextAnchor.LowerCenter;
            case TextAlignmentOptions.BottomLeft:
                return TextAnchor.LowerLeft;
            case TextAlignmentOptions.BottomRight:
                return TextAnchor.LowerRight;
            default:
                return TextAnchor.MiddleCenter;
        }
    }

    private static RectTransform CreateRect(string name, Transform parent, RectSpec spec)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rect = (RectTransform)go.transform;
        rect.SetParent(parent, false);
        spec.Apply(rect);
        return rect;
    }

    private enum ResourceStyle
    {
        Text,
        Health,
        Mana
    }

    private enum PanelMaterial
    {
        Jade,
        Parchment,
        Map,
        Bamboo
    }

    private enum Rarity
    {
        Common,
        Rare,
        Epic
    }

    private enum CardArtKind
    {
        Sword,
        Qi,
        Talisman
    }

    private enum NodeIconKind
    {
        Battle,
        Event,
        Chest,
        Rest,
        Market
    }

    private static TMP_FontAsset LoadTmpFont()
    {
        if (_tmpFont != null)
        {
            return _tmpFont;
        }

        _tmpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TmpFontAssetPath);
        if (_tmpFont == null || !IsTmpFontUsable(_tmpFont))
        {
            _tmpFont = EnsureTmpFontAsset();
        }

        return _tmpFont;
    }

    private static Font LoadUiFont()
    {
        if (_uiFont != null)
        {
            return _uiFont;
        }

        _uiFont = AssetDatabase.LoadAssetAtPath<Font>(FontSourcePath);
        if (_uiFont == null)
        {
            _uiFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        return _uiFont;
    }

    private static TMP_FontAsset EnsureTmpFontAsset()
    {
        var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TmpFontAssetPath);
        if (existing != null)
        {
            if (!IsTmpFontUsable(existing))
            {
                AssetDatabase.DeleteAsset(TmpFontAssetPath);
                existing = null;
            }
        }

        if (existing != null)
        {
            existing.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            existing.isMultiAtlasTexturesEnabled = true;
            _tmpFont = existing;
            return existing;
        }

        var font = AssetDatabase.LoadAssetAtPath<Font>(FontSourcePath);
        if (font == null)
        {
            Debug.LogWarning($"Phase77B TMP font source is missing: {FontSourcePath}. Import NotoSansCJKsc-VF.ttf before generating the golden sample.");
            return TMP_Settings.defaultFontAsset;
        }

        EnsureTextMeshProShader();
        var fontAsset = TMP_FontAsset.CreateFontAsset(font, 90, 9, GlyphRenderMode.SDFAA, 4096, 4096, AtlasPopulationMode.Dynamic, true);
        if (fontAsset == null)
        {
            Debug.LogWarning("Failed to create TMP font asset for Phase77B. Falling back to TMP default font.");
            return TMP_Settings.defaultFontAsset;
        }

        fontAsset.name = "NotoSansCJKsc_Phase77A SDF";
        fontAsset.isMultiAtlasTexturesEnabled = true;
        fontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
        AssetDatabase.CreateAsset(fontAsset, TmpFontAssetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(TmpFontAssetPath, ImportAssetOptions.ForceUpdate);

        _tmpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TmpFontAssetPath);
        return _tmpFont != null ? _tmpFont : fontAsset;
    }

    private static bool IsTmpFontUsable(TMP_FontAsset fontAsset)
    {
        if (fontAsset == null)
        {
            return false;
        }

        var atlasTextures = fontAsset.atlasTextures;
        return atlasTextures != null && atlasTextures.Length > 0 && atlasTextures[0] != null && fontAsset.material != null;
    }

    private static void EnsureTextMeshProShader()
    {
        var shader = Shader.Find(TmpSdfShaderName);
        if (shader != null)
        {
            return;
        }

        var shaderGuids = AssetDatabase.FindAssets("Distance Field t:Shader");
        foreach (var guid in shaderGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.Contains("TextMesh Pro") && !path.Contains("TextMeshPro") && !path.Contains("com.unity.textmeshpro"))
            {
                continue;
            }

            AssetDatabase.LoadAssetAtPath<Shader>(path);
            shader = Shader.Find(TmpSdfShaderName);
            if (shader != null)
            {
                return;
            }
        }

        Debug.LogWarning($"TMP SDF shader was not resolved by name: {TmpSdfShaderName}. Text rendering may fall back to the default TMP font.");
    }

    private static Sprite LoadSprite(string fileName)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(Asset(fileName));
    }

    private static string Asset(string fileName)
    {
        return GeneratedDir + "/" + fileName;
    }

    private static void EnsureDirectories()
    {
        var dirs = new[]
        {
            BackgroundDir,
            PanelDir,
            ButtonDir,
            CardDir,
            RouteNodeDir,
            IconFrameDir,
            DividerDir,
            DecorationDir,
            PopupDir,
            GeneratedDir,
            TemplateDir,
            FontDir
        };

        foreach (var dir in dirs)
        {
            Directory.CreateDirectory(ToAbsolute(dir));
        }
    }

    private static void CopyGeneratedAsset(string fileName, string destinationDir)
    {
        var source = Asset(fileName);
        var destination = destinationDir + "/" + fileName;
        File.Copy(ToAbsolute(source), ToAbsolute(destination), true);
        AssetDatabase.ImportAsset(destination, ImportAssetOptions.ForceUpdate);
        CopySpriteImporter(source, destination);
    }

    private static void CopySpriteImporter(string source, string destination)
    {
        var sourceImporter = (TextureImporter)AssetImporter.GetAtPath(source);
        var destinationImporter = (TextureImporter)AssetImporter.GetAtPath(destination);
        if (sourceImporter == null || destinationImporter == null)
        {
            return;
        }

        destinationImporter.textureType = sourceImporter.textureType;
        destinationImporter.spriteImportMode = sourceImporter.spriteImportMode;
        destinationImporter.alphaIsTransparency = sourceImporter.alphaIsTransparency;
        destinationImporter.mipmapEnabled = sourceImporter.mipmapEnabled;
        destinationImporter.textureCompression = sourceImporter.textureCompression;
        destinationImporter.spritePixelsPerUnit = sourceImporter.spritePixelsPerUnit;
        destinationImporter.spriteBorder = sourceImporter.spriteBorder;
        destinationImporter.SaveAndReimport();
    }

    private static void CreateMountainMistBackground(string assetPath)
    {
        const int width = 1920;
        const int height = 1080;
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        var top = Hex("#163F38");
        var middle = Hex("#0A211E");
        var bottom = Hex("#050B0A");

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var t = y / (float)(height - 1);
                var color = t > 0.48f ? Color.Lerp(middle, top, (t - 0.48f) / 0.52f) : Color.Lerp(bottom, middle, t / 0.48f);
                var nx = (x - width * 0.5f) / (width * 0.5f);
                var ny = (y - height * 0.5f) / (height * 0.5f);
                var vignette = Mathf.Clamp01((nx * nx + ny * ny) * 0.72f);
                color = Color.Lerp(color, Hex("#010504"), vignette);

                var mountainBack = MountainHeight(x, 650f, 0.006f, 0.014f, 66f);
                var mountainMid = MountainHeight(x + 220, 510f, 0.008f, 0.017f, 94f);
                var mountainFront = MountainHeight(x - 160, 390f, 0.011f, 0.024f, 124f);
                if (y < mountainBack)
                {
                    color = Color.Lerp(color, Hex("#0A1817"), 0.42f);
                }
                if (y < mountainMid)
                {
                    color = Color.Lerp(color, Hex("#07100F"), 0.52f);
                }
                if (y < mountainFront)
                {
                    color = Color.Lerp(color, Hex("#030807"), 0.65f);
                }

                var mistA = Mathf.Sin(x * 0.010f + y * 0.006f) + Mathf.Sin(x * 0.019f - y * 0.003f);
                var mistBand = Mathf.Exp(-Mathf.Pow((y - 520f - mistA * 26f) / 95f, 2f)) * 0.12f;
                mistBand += Mathf.Exp(-Mathf.Pow((y - 670f + Mathf.Sin(x * 0.012f) * 30f) / 130f, 2f)) * 0.08f;
                color = Color.Lerp(color, Hex("#A9D3C3"), mistBand);

                var grain = (Hash01(x, y) - 0.5f) * 0.025f;
                color.r = Mathf.Clamp01(color.r + grain);
                color.g = Mathf.Clamp01(color.g + grain);
                color.b = Mathf.Clamp01(color.b + grain);
                texture.SetPixel(x, y, color);
            }
        }

        SaveTexture(texture, assetPath, Vector4.zero);
    }

    private static float MountainHeight(float x, float baseHeight, float waveA, float waveB, float amp)
    {
        return baseHeight + Mathf.Sin(x * waveA) * amp + Mathf.Sin(x * waveB + 1.7f) * amp * 0.55f + Mathf.Sin(x * 0.031f) * amp * 0.18f;
    }

    private static void CreateRunePatternTexture(string assetPath)
    {
        const int size = 1024;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var center = new Vector2(size * 0.5f, size * 0.5f);
        var jade = Hex("#72D8B9");
        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var p = new Vector2(x, y) - center;
                var r = p.magnitude;
                var angle = Mathf.Atan2(p.y, p.x);
                var alpha = 0f;

                alpha += RingAlpha(r, 256f, 2.3f) * 0.32f;
                alpha += RingAlpha(r, 340f, 1.7f) * 0.22f;
                alpha += RingAlpha(r, 420f, 2.1f) * 0.18f;
                alpha += Mathf.Abs(Mathf.Sin(angle * 12f)) < 0.035f && r > 238f && r < 430f ? 0.16f : 0f;
                alpha += Mathf.Abs(Mathf.Sin((x + y) * 0.012f)) < 0.012f && r > 180f && r < 410f ? 0.06f : 0f;

                for (var i = 0; i < 8; i++)
                {
                    var a = i * Mathf.PI * 0.25f;
                    var mark = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 340f;
                    var d = (p - mark).magnitude;
                    alpha += d < 22f || (Mathf.Abs(d - 44f) < 1.8f && d < 58f) ? 0.18f : 0f;
                }

                alpha *= Mathf.SmoothStep(1f, 0f, Mathf.Clamp01((r - 472f) / 62f));
                texture.SetPixel(x, y, new Color(jade.r, jade.g, jade.b, Mathf.Clamp01(alpha)));
            }
        }

        SaveTexture(texture, assetPath, Vector4.zero);
    }

    private static float RingAlpha(float radius, float target, float width)
    {
        return 1f - Mathf.Clamp01(Mathf.Abs(radius - target) / width);
    }

    private static void CreateVignetteTexture(string assetPath)
    {
        const int width = 1920;
        const int height = 1080;
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var nx = (x - width * 0.5f) / (width * 0.5f);
                var ny = (y - height * 0.5f) / (height * 0.5f);
                var edge = Mathf.Clamp01((nx * nx + ny * ny - 0.28f) / 0.86f);
                var bottomFog = Mathf.Exp(-Mathf.Pow((y - 210f) / 140f, 2f)) * 0.08f;
                texture.SetPixel(x, y, new Color(0f, 0f, 0f, Mathf.Clamp01(edge * 0.52f - bottomFog)));
            }
        }

        SaveTexture(texture, assetPath, Vector4.zero);
    }

    private static void CreatePanelTexture(string assetPath, int width, int height, Color top, Color bottom, Color border, int borderWidth, int radius, PanelMaterial material, float alpha)
    {
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var dist = RoundedRectDistance(x, y, width, height, radius);
                if (dist > 0)
                {
                    texture.SetPixel(x, y, Color.clear);
                    continue;
                }

                var t = y / (float)(height - 1);
                var color = Color.Lerp(bottom, top, t);
                var edge = Mathf.Min(Mathf.Min(x, width - 1 - x), Mathf.Min(y, height - 1 - y));
                if (edge < borderWidth || dist > -borderWidth)
                {
                    color = border;
                }
                else
                {
                    var innerShadow = Mathf.Clamp01((borderWidth + 22 - edge) / 30f) * 0.18f;
                    color = Color.Lerp(color, Color.black, innerShadow);
                    ApplyMaterialNoise(ref color, x, y, material);
                }

                color.a = alpha;
                texture.SetPixel(x, y, color);
            }
        }

        SaveTexture(texture, assetPath, new Vector4(radius, radius, radius, radius));
    }

    private static void ApplyMaterialNoise(ref Color color, int x, int y, PanelMaterial material)
    {
        var grain = (Hash01(x, y) - 0.5f);
        switch (material)
        {
            case PanelMaterial.Jade:
                var vein = Mathf.Sin(x * 0.034f + Mathf.Sin(y * 0.02f) * 2.5f) * 0.028f;
                color.r = Mathf.Clamp01(color.r + vein * 0.5f + grain * 0.025f);
                color.g = Mathf.Clamp01(color.g + vein + grain * 0.025f);
                color.b = Mathf.Clamp01(color.b + vein * 0.8f + grain * 0.02f);
                break;
            case PanelMaterial.Parchment:
                var paper = Mathf.Sin((x + y) * 0.042f) * 0.018f + grain * 0.045f;
                color.r = Mathf.Clamp01(color.r + paper);
                color.g = Mathf.Clamp01(color.g + paper * 0.85f);
                color.b = Mathf.Clamp01(color.b + paper * 0.5f);
                break;
            case PanelMaterial.Map:
                var mist = Mathf.Sin(x * 0.015f + y * 0.02f) * 0.022f + Mathf.Sin((x - y) * 0.01f) * 0.018f;
                color.r = Mathf.Clamp01(color.r + mist * 0.6f + grain * 0.02f);
                color.g = Mathf.Clamp01(color.g + mist + grain * 0.02f);
                color.b = Mathf.Clamp01(color.b + mist * 0.9f);
                break;
            case PanelMaterial.Bamboo:
                var stripe = Mathf.Sin(x * 0.09f) * 0.026f + grain * 0.03f;
                color.r = Mathf.Clamp01(color.r + stripe * 0.9f);
                color.g = Mathf.Clamp01(color.g + stripe);
                color.b = Mathf.Clamp01(color.b + stripe * 0.55f);
                break;
        }
    }

    private static void CreateButtonTexture(string assetPath, int width, int height, Color top, Color bottom, Color border, bool disabled)
    {
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        var radius = 20;
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var dist = RoundedRectDistance(x, y, width, height, radius);
                if (dist > 0)
                {
                    texture.SetPixel(x, y, Color.clear);
                    continue;
                }

                var t = y / (float)(height - 1);
                var color = Color.Lerp(bottom, top, t);
                var edge = Mathf.Min(Mathf.Min(x, width - 1 - x), Mathf.Min(y, height - 1 - y));
                if (edge < 3 || dist > -3)
                {
                    color = border;
                }
                else
                {
                    var centerHighlight = Mathf.Exp(-Mathf.Pow((x - width * 0.5f) / (width * 0.42f), 2f)) * Mathf.Exp(-Mathf.Pow((y - height * 0.62f) / (height * 0.36f), 2f)) * 0.16f;
                    color = Color.Lerp(color, Color.white, disabled ? 0.02f : centerHighlight);
                    ApplyMaterialNoise(ref color, x, y, PanelMaterial.Parchment);
                }

                texture.SetPixel(x, y, color);
            }
        }

        SaveTexture(texture, assetPath, new Vector4(radius, radius, radius, radius));
    }

    private static void CreateCardTexture(string assetPath, Color top, Color bottom, Color border, Color innerBorder, Rarity rarity)
    {
        const int width = 256;
        const int height = 380;
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        const int radius = 16;
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var dist = RoundedRectDistance(x, y, width, height, radius);
                if (dist > 0)
                {
                    texture.SetPixel(x, y, Color.clear);
                    continue;
                }

                var t = y / (float)(height - 1);
                var color = Color.Lerp(bottom, top, t);
                var edge = Mathf.Min(Mathf.Min(x, width - 1 - x), Mathf.Min(y, height - 1 - y));
                if (edge < 7 || dist > -7)
                {
                    color = border;
                }
                else if (edge < 12 || dist > -12 || (y > height - 62 && y < height - 56) || (y < 112 && y > 106))
                {
                    color = innerBorder;
                }
                else
                {
                    ApplyMaterialNoise(ref color, x, y, rarity == Rarity.Common ? PanelMaterial.Parchment : PanelMaterial.Jade);
                    var verticalGlow = Mathf.Exp(-Mathf.Pow((x - width * 0.5f) / (width * 0.54f), 2f)) * 0.055f;
                    color = Color.Lerp(color, border, verticalGlow);
                }

                texture.SetPixel(x, y, color);
            }
        }

        SaveTexture(texture, assetPath, new Vector4(radius, radius, radius, radius));
    }

    private static void CreateCardArtTexture(string assetPath, CardArtKind kind)
    {
        const int width = 220;
        const int height = 160;
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        var bgTop = kind == CardArtKind.Talisman ? Hex("#3C2330") : kind == CardArtKind.Qi ? Hex("#17453E") : Hex("#22302F");
        var bgBottom = Hex("#0A1110");
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var t = y / (float)(height - 1);
                var color = Color.Lerp(bgBottom, bgTop, t);
                var nx = (x - width * 0.5f) / (width * 0.5f);
                var ny = (y - height * 0.5f) / (height * 0.5f);
                color = Color.Lerp(color, Color.black, Mathf.Clamp01((nx * nx + ny * ny) * 0.42f));
                ApplyMaterialNoise(ref color, x, y, PanelMaterial.Map);
                var alpha = RoundedRectDistance(x, y, width, height, 14) > 0 ? 0f : 1f;
                texture.SetPixel(x, y, new Color(color.r, color.g, color.b, alpha));
            }
        }

        DrawCardArt(texture, kind);
        SaveTexture(texture, assetPath, new Vector4(14, 14, 14, 14));
    }

    private static void DrawCardArt(Texture2D texture, CardArtKind kind)
    {
        var gold = Hex("#D9B56A");
        var jade = Hex("#70D8B8");
        var red = Hex("#C75A4D");
        if (kind == CardArtKind.Sword)
        {
            DrawLine(texture, new Vector2(78, 34), new Vector2(142, 124), gold, 5);
            DrawLine(texture, new Vector2(70, 116), new Vector2(134, 28), jade, 4);
            DrawLine(texture, new Vector2(60, 62), new Vector2(98, 40), gold, 3);
            DrawLine(texture, new Vector2(122, 118), new Vector2(158, 96), gold, 3);
        }
        else if (kind == CardArtKind.Qi)
        {
            for (var i = 0; i < 4; i++)
            {
                DrawArc(texture, new Vector2(110, 80), 28 + i * 15, i * 0.55f, Mathf.PI * 1.35f + i * 0.3f, i % 2 == 0 ? jade : gold, 3);
            }
            DrawCircle(texture, new Vector2(110, 80), 16, jade, 0.55f);
        }
        else
        {
            DrawRect(texture, new Rect(82, 26, 56, 104), Hex("#E7D2A4"), 0.86f);
            DrawLine(texture, new Vector2(92, 104), new Vector2(128, 104), red, 3);
            DrawLine(texture, new Vector2(100, 80), new Vector2(124, 58), red, 3);
            DrawLine(texture, new Vector2(124, 58), new Vector2(112, 48), red, 3);
            DrawLine(texture, new Vector2(112, 48), new Vector2(132, 36), red, 3);
            DrawArc(texture, new Vector2(110, 80), 54, 0.4f, 2.7f, gold, 2);
        }
    }

    private static void CreateCostOrbTexture(string assetPath)
    {
        const int size = 96;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var center = new Vector2(size * 0.5f, size * 0.5f);
        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var d = (new Vector2(x, y) - center).magnitude;
                if (d > 45)
                {
                    texture.SetPixel(x, y, Color.clear);
                    continue;
                }

                var t = Mathf.Clamp01(d / 45f);
                var color = Color.Lerp(Hex("#F7E09A"), Hex("#8A6121"), t);
                if (d > 37)
                {
                    color = Hex("#D7A348");
                }
                texture.SetPixel(x, y, color);
            }
        }

        SaveTexture(texture, assetPath, Vector4.zero);
    }

    private static void CreateRouteMapTexture(string assetPath)
    {
        const int width = 900;
        const int height = 620;
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var t = y / (float)(height - 1);
                var color = Color.Lerp(Hex("#071311"), Hex("#153631"), t);
                ApplyMaterialNoise(ref color, x, y, PanelMaterial.Map);
                var mist = Mathf.Exp(-Mathf.Pow((y - 330f + Mathf.Sin(x * 0.018f) * 26f) / 92f, 2f)) * 0.12f;
                color = Color.Lerp(color, Hex("#B7D9C7"), mist);

                var hill = 120 + Mathf.Sin(x * 0.025f) * 18 + Mathf.Sin(x * 0.007f) * 44;
                if (y < hill)
                {
                    color = Color.Lerp(color, Hex("#050A09"), 0.48f);
                }

                var alpha = RoundedRectDistance(x, y, width, height, 22) > 0 ? 0f : 0.92f;
                texture.SetPixel(x, y, new Color(color.r, color.g, color.b, alpha));
            }
        }

        DrawArc(texture, new Vector2(450, 310), 230, 0.08f, 5.9f, new Color(0.52f, 0.9f, 0.75f, 0.18f), 2);
        DrawArc(texture, new Vector2(450, 310), 305, 0.4f, 5.4f, new Color(0.85f, 0.65f, 0.28f, 0.12f), 2);
        SaveTexture(texture, assetPath, new Vector4(22, 22, 22, 22));
    }

    private static void CreateSpiritLineTexture(string assetPath)
    {
        const int width = 360;
        const int height = 34;
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var center = height * 0.5f + Mathf.Sin(x * 0.045f) * 4f;
                var d = Mathf.Abs(y - center);
                var alpha = Mathf.Clamp01(1f - d / 7f) * Mathf.SmoothStep(0, 1, Mathf.Min(x, width - 1 - x) / 28f);
                var color = Color.Lerp(Hex("#5FE0BC"), Hex("#D8B45F"), Mathf.Sin(x * 0.05f) * 0.5f + 0.5f);
                texture.SetPixel(x, y, new Color(color.r, color.g, color.b, alpha * 0.82f));
            }
        }

        SaveTexture(texture, assetPath, Vector4.zero);
    }

    private static void CreateNodeIconTexture(string assetPath, NodeIconKind kind)
    {
        const int size = 180;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var center = new Vector2(size * 0.5f, size * 0.5f);
        var main = kind == NodeIconKind.Battle ? Hex("#C5783A") :
            kind == NodeIconKind.Event ? Hex("#78D8BE") :
            kind == NodeIconKind.Chest ? Hex("#D7A94A") :
            kind == NodeIconKind.Rest ? Hex("#86B66E") :
            Hex("#B88945");
        var dark = Hex("#07100F");

        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var p = new Vector2(x, y) - center;
                var r = p.magnitude;
                if (r > 76)
                {
                    texture.SetPixel(x, y, Color.clear);
                    continue;
                }

                var color = Color.Lerp(Hex("#182B25"), dark, Mathf.Clamp01(r / 78f));
                if (Mathf.Abs(r - 68) < 3)
                {
                    color = main;
                }
                ApplyMaterialNoise(ref color, x, y, PanelMaterial.Jade);
                texture.SetPixel(x, y, new Color(color.r, color.g, color.b, 0.95f));
            }
        }

        DrawNodeSymbol(texture, kind, main);
        SaveTexture(texture, assetPath, Vector4.zero);
    }

    private static void DrawNodeSymbol(Texture2D texture, NodeIconKind kind, Color color)
    {
        if (kind == NodeIconKind.Battle)
        {
            DrawLine(texture, new Vector2(70, 50), new Vector2(114, 126), color, 6);
            DrawLine(texture, new Vector2(112, 50), new Vector2(66, 126), color, 6);
            DrawLine(texture, new Vector2(58, 60), new Vector2(82, 48), color, 3);
            DrawLine(texture, new Vector2(122, 60), new Vector2(98, 48), color, 3);
        }
        else if (kind == NodeIconKind.Event)
        {
            DrawRect(texture, new Rect(58, 54, 64, 72), color, 0.88f);
            DrawLine(texture, new Vector2(66, 72), new Vector2(114, 72), Hex("#2A1A0A"), 2);
            DrawLine(texture, new Vector2(66, 92), new Vector2(104, 92), Hex("#2A1A0A"), 2);
            DrawLine(texture, new Vector2(66, 112), new Vector2(112, 112), Hex("#2A1A0A"), 2);
        }
        else if (kind == NodeIconKind.Chest)
        {
            DrawRect(texture, new Rect(54, 74, 72, 44), color, 0.92f);
            DrawRect(texture, new Rect(62, 58, 56, 22), color, 0.85f);
            DrawLine(texture, new Vector2(90, 58), new Vector2(90, 118), Hex("#2A1A0A"), 3);
        }
        else if (kind == NodeIconKind.Rest)
        {
            DrawArc(texture, new Vector2(90, 94), 42, Mathf.PI, Mathf.PI * 2f, color, 6);
            DrawLine(texture, new Vector2(54, 118), new Vector2(126, 118), color, 5);
            DrawLine(texture, new Vector2(78, 56), new Vector2(102, 56), color, 4);
        }
        else
        {
            DrawLine(texture, new Vector2(50, 78), new Vector2(90, 48), color, 5);
            DrawLine(texture, new Vector2(90, 48), new Vector2(130, 78), color, 5);
            DrawLine(texture, new Vector2(60, 82), new Vector2(120, 82), color, 4);
            DrawLine(texture, new Vector2(64, 82), new Vector2(64, 120), color, 4);
            DrawLine(texture, new Vector2(116, 82), new Vector2(116, 120), color, 4);
            DrawLine(texture, new Vector2(54, 120), new Vector2(126, 120), color, 5);
        }
    }

    private static void CreateCurrentGlowTexture(string assetPath)
    {
        const int size = 220;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var center = new Vector2(size * 0.5f, size * 0.5f);
        var color = Hex("#E8C86C");
        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var d = (new Vector2(x, y) - center).magnitude;
                var ring = RingAlpha(d, 76f, 5f) * 0.72f + RingAlpha(d, 94f, 4f) * 0.32f;
                var glow = Mathf.Exp(-Mathf.Pow((d - 78f) / 28f, 2f)) * 0.22f;
                texture.SetPixel(x, y, new Color(color.r, color.g, color.b, Mathf.Clamp01(ring + glow)));
            }
        }

        SaveTexture(texture, assetPath, Vector4.zero);
    }

    private static void CreateJadeDiscTexture(string assetPath)
    {
        const int size = 160;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var center = new Vector2(size * 0.5f, size * 0.5f);
        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var d = (new Vector2(x, y) - center).magnitude;
                if (d > 74)
                {
                    texture.SetPixel(x, y, Color.clear);
                    continue;
                }

                var color = Color.Lerp(Hex("#24564A"), Hex("#071918"), Mathf.Clamp01(d / 74f));
                if (d > 62)
                {
                    color = Hex("#C4A25B");
                }
                ApplyMaterialNoise(ref color, x, y, PanelMaterial.Jade);
                texture.SetPixel(x, y, new Color(color.r, color.g, color.b, 0.92f));
            }
        }

        SaveTexture(texture, assetPath, Vector4.zero);
    }

    private static void CreateDividerTexture(string assetPath, int width, int height)
    {
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        var color = Hex("#C59A49");
        for (var y = 0; y < texture.height; y++)
        {
            for (var x = 0; x < texture.width; x++)
            {
                var alpha = Mathf.SmoothStep(0, 1, Mathf.Min(x, texture.width - 1 - x) / 48f);
                alpha *= 1f - Mathf.Abs(y - texture.height * 0.5f) / (texture.height * 0.55f);
                var wave = Mathf.Sin(x * 0.08f) * 0.16f + 0.84f;
                texture.SetPixel(x, y, new Color(color.r, color.g, color.b, Mathf.Clamp01(alpha * wave)));
            }
        }

        SaveTexture(texture, assetPath, Vector4.zero);
    }

    private static void CreateCornerTexture(string assetPath)
    {
        var texture = new Texture2D(96, 96, TextureFormat.RGBA32, false);
        var color = Hex("#D0A85C");
        for (var y = 0; y < texture.height; y++)
        {
            for (var x = 0; x < texture.width; x++)
            {
                var inCorner = (x < 7 && y < 72) || (y < 7 && x < 72) || (x - y > 42 && x < 72 && y < 24);
                var inner = (x > 18 && x < 23 && y < 58) || (y > 18 && y < 23 && x < 58);
                texture.SetPixel(x, y, inCorner || inner ? color : Color.clear);
            }
        }

        SaveTexture(texture, assetPath, Vector4.zero);
    }

    private static void CreateMaskTexture(string assetPath)
    {
        var texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
        for (var y = 0; y < texture.height; y++)
        {
            for (var x = 0; x < texture.width; x++)
            {
                texture.SetPixel(x, y, Hex("#050908"));
            }
        }

        SaveTexture(texture, assetPath, Vector4.zero);
    }

    private static void DrawLine(Texture2D texture, Vector2 from, Vector2 to, Color color, int width)
    {
        var minX = Mathf.FloorToInt(Mathf.Min(from.x, to.x) - width - 2);
        var maxX = Mathf.CeilToInt(Mathf.Max(from.x, to.x) + width + 2);
        var minY = Mathf.FloorToInt(Mathf.Min(from.y, to.y) - width - 2);
        var maxY = Mathf.CeilToInt(Mathf.Max(from.y, to.y) + width + 2);
        var segment = to - from;
        var segmentLength = segment.sqrMagnitude;
        for (var y = minY; y <= maxY; y++)
        {
            for (var x = minX; x <= maxX; x++)
            {
                if (!InBounds(texture, x, y))
                {
                    continue;
                }

                var p = new Vector2(x, y);
                var t = Mathf.Clamp01(Vector2.Dot(p - from, segment) / segmentLength);
                var closest = from + segment * t;
                var d = (p - closest).magnitude;
                if (d <= width)
                {
                    BlendPixel(texture, x, y, color, Mathf.Clamp01(1f - d / (width + 0.5f)) * color.a);
                }
            }
        }
    }

    private static void DrawArc(Texture2D texture, Vector2 center, float radius, float start, float end, Color color, int width)
    {
        var minX = Mathf.FloorToInt(center.x - radius - width - 2);
        var maxX = Mathf.CeilToInt(center.x + radius + width + 2);
        var minY = Mathf.FloorToInt(center.y - radius - width - 2);
        var maxY = Mathf.CeilToInt(center.y + radius + width + 2);
        for (var y = minY; y <= maxY; y++)
        {
            for (var x = minX; x <= maxX; x++)
            {
                if (!InBounds(texture, x, y))
                {
                    continue;
                }

                var p = new Vector2(x, y) - center;
                var angle = Mathf.Atan2(p.y, p.x);
                if (angle < 0)
                {
                    angle += Mathf.PI * 2f;
                }

                if (angle < start || angle > end)
                {
                    continue;
                }

                var d = Mathf.Abs(p.magnitude - radius);
                if (d <= width)
                {
                    BlendPixel(texture, x, y, color, Mathf.Clamp01(1f - d / (width + 0.5f)) * color.a);
                }
            }
        }
    }

    private static void DrawCircle(Texture2D texture, Vector2 center, float radius, Color color, float alpha)
    {
        var minX = Mathf.FloorToInt(center.x - radius);
        var maxX = Mathf.CeilToInt(center.x + radius);
        var minY = Mathf.FloorToInt(center.y - radius);
        var maxY = Mathf.CeilToInt(center.y + radius);
        for (var y = minY; y <= maxY; y++)
        {
            for (var x = minX; x <= maxX; x++)
            {
                if (!InBounds(texture, x, y))
                {
                    continue;
                }

                var d = (new Vector2(x, y) - center).magnitude;
                if (d <= radius)
                {
                    BlendPixel(texture, x, y, color, alpha * Mathf.Clamp01(1f - d / radius));
                }
            }
        }
    }

    private static void DrawRect(Texture2D texture, Rect rect, Color color, float alpha)
    {
        var minX = Mathf.FloorToInt(rect.xMin);
        var maxX = Mathf.CeilToInt(rect.xMax);
        var minY = Mathf.FloorToInt(rect.yMin);
        var maxY = Mathf.CeilToInt(rect.yMax);
        for (var y = minY; y <= maxY; y++)
        {
            for (var x = minX; x <= maxX; x++)
            {
                if (InBounds(texture, x, y))
                {
                    BlendPixel(texture, x, y, color, alpha);
                }
            }
        }
    }

    private static bool InBounds(Texture2D texture, int x, int y)
    {
        return x >= 0 && y >= 0 && x < texture.width && y < texture.height;
    }

    private static void BlendPixel(Texture2D texture, int x, int y, Color color, float alpha)
    {
        var existing = texture.GetPixel(x, y);
        var sourceAlpha = Mathf.Clamp01(alpha);
        var blended = Color.Lerp(existing, color, sourceAlpha);
        blended.a = Mathf.Clamp01(existing.a + sourceAlpha * color.a);
        texture.SetPixel(x, y, blended);
    }

    private static void SaveTexture(Texture2D texture, string assetPath, Vector4 border)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(ToAbsolute(assetPath)) ?? string.Empty);
        WriteAllBytesWithRetry(ToAbsolute(assetPath), texture.EncodeToPNG(), assetPath);
        UnityEngine.Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

        var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.spritePixelsPerUnit = 100;
        importer.spriteBorder = border;
        importer.SaveAndReimport();
    }

    private static void WriteAllBytesWithRetry(string absolutePath, byte[] bytes, string assetPath)
    {
        const int maxAttempts = 20;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                AssetDatabase.ReleaseCachedFileHandles();
                File.WriteAllBytes(absolutePath, bytes);
                return;
            }
            catch (IOException) when (attempt < maxAttempts)
            {
                Thread.Sleep(150);
            }
            catch (UnauthorizedAccessException) when (attempt < maxAttempts)
            {
                Thread.Sleep(150);
            }
        }

        if (File.Exists(absolutePath))
        {
            Debug.LogWarning($"Skipped rewriting locked UI asset and kept existing file: {assetPath}");
            return;
        }

        File.WriteAllBytes(absolutePath, bytes);
    }

    private static float RoundedRectDistance(int x, int y, int width, int height, int radius)
    {
        var px = Mathf.Abs(x - width * 0.5f) - (width * 0.5f - radius);
        var py = Mathf.Abs(y - height * 0.5f) - (height * 0.5f - radius);
        var outside = new Vector2(Mathf.Max(px, 0), Mathf.Max(py, 0)).magnitude;
        var inside = Mathf.Min(Mathf.Max(px, py), 0);
        return outside + inside - radius;
    }

    private static float Hash01(int x, int y)
    {
        unchecked
        {
            var n = x * 73856093 ^ y * 19349663;
            n = (n << 13) ^ n;
            return 1f - ((n * (n * n * 15731 + 789221) + 1376312589) & 0x7fffffff) / 1073741824f;
        }
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

    private readonly struct RectSpec
    {
        private readonly Vector2 _anchorMin;
        private readonly Vector2 _anchorMax;
        private readonly Vector2 _pivot;
        private readonly Vector2 _anchoredPosition;
        private readonly Vector2 _sizeDelta;
        private readonly Vector2 _offsetMin;
        private readonly Vector2 _offsetMax;
        private readonly bool _stretch;

        private RectSpec(Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta, Vector2 offsetMin, Vector2 offsetMax, bool stretch)
        {
            _anchorMin = anchorMin;
            _anchorMax = anchorMax;
            _pivot = pivot;
            _anchoredPosition = anchoredPosition;
            _sizeDelta = sizeDelta;
            _offsetMin = offsetMin;
            _offsetMax = offsetMax;
            _stretch = stretch;
        }

        public static RectSpec Center(Vector2 position, Vector2 size)
        {
            return new RectSpec(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, size, Vector2.zero, Vector2.zero, false);
        }

        public static RectSpec Top(Vector2 position, Vector2 size)
        {
            return new RectSpec(new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), position, size, Vector2.zero, Vector2.zero, false);
        }

        public static RectSpec Bottom(Vector2 position, Vector2 size)
        {
            return new RectSpec(new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), position, size, Vector2.zero, Vector2.zero, false);
        }

        public static RectSpec Left(Vector2 position, Vector2 size)
        {
            return new RectSpec(new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), position, size, Vector2.zero, Vector2.zero, false);
        }

        public static RectSpec Stretch(Vector2 offsetMin, Vector2 offsetMax)
        {
            return new RectSpec(Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, offsetMin, -offsetMax, true);
        }

        public void Apply(RectTransform rect)
        {
            rect.anchorMin = _anchorMin;
            rect.anchorMax = _anchorMax;
            rect.pivot = _pivot;
            if (_stretch)
            {
                rect.offsetMin = _offsetMin;
                rect.offsetMax = _offsetMax;
            }
            else
            {
                rect.anchoredPosition = _anchoredPosition;
                rect.sizeDelta = _sizeDelta;
            }

            rect.localScale = Vector3.one;
        }
    }
}
#endif
