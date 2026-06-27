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
    private const string PrefabPath = TemplateDir + "/MainRunWindow_Golden.prefab";
    private const string ScreenshotFileName = "MainRunWindow_Golden_Phase77A.png";

    private static TMP_FontAsset _tmpFont;

    [MenuItem("Codex/Cultivation UI/Generate Golden Sample And Screenshot")]
    public static void GenerateGoldenSampleAndScreenshot()
    {
        GenerateThemeAssets();
        GenerateGoldenPrefab();
        CaptureGoldenSample();
        AssetDatabase.Refresh();
        Debug.Log("Cultivation UI golden sample generated and captured.");
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

        CreateBackgroundTexture(Asset("BG_DarkJade_Gradient.png"), false);
        CreateBackgroundTexture(Asset("BG_Cultivation_DarkPattern.png"), true);

        CreateRoundedTexture(Asset("Panel_DarkJade_GoldBorder.png"), 320, 320, Hex("#15342D"), Hex("#0B1E1B"), Hex("#A77A34"), 10, 26, true);
        CreateRoundedTexture(Asset("Panel_DarkJade_Inner.png"), 320, 320, Hex("#183C34"), Hex("#0B201D"), Hex("#476F5F"), 5, 22, true);
        CreateRoundedTexture(Asset("Panel_Parchment_Dark.png"), 320, 320, Hex("#2A261B"), Hex("#17160F"), Hex("#7F5A2A"), 8, 22, true);

        CreateRoundedTexture(Asset("Button_Gold_Primary_Normal.png"), 360, 96, Hex("#D8B35F"), Hex("#886324"), Hex("#F2D88B"), 8, 22, true);
        CreateRoundedTexture(Asset("Button_Gold_Primary_Pressed.png"), 360, 96, Hex("#8C6626"), Hex("#543713"), Hex("#C59A49"), 8, 22, true);
        CreateRoundedTexture(Asset("Button_Gold_Primary_Disabled.png"), 360, 96, Hex("#536058"), Hex("#303A34"), Hex("#72806F"), 8, 22, false);
        CreateRoundedTexture(Asset("Button_Jade_Secondary_Normal.png"), 320, 82, Hex("#24524A"), Hex("#102B26"), Hex("#75B89D"), 7, 18, true);
        CreateRoundedTexture(Asset("Button_Jade_Secondary_Pressed.png"), 320, 82, Hex("#14352F"), Hex("#071A17"), Hex("#4C8D79"), 7, 18, true);
        CreateRoundedTexture(Asset("Button_Jade_Secondary_Disabled.png"), 320, 82, Hex("#394A41"), Hex("#1C2823"), Hex("#607265"), 7, 18, false);

        CreateRoundedTexture(Asset("Card_Frame_Common.png"), 256, 360, Hex("#24372D"), Hex("#121B17"), Hex("#B9A16B"), 10, 20, true);
        CreateRoundedTexture(Asset("Card_Frame_Rare.png"), 256, 360, Hex("#173B39"), Hex("#0A2426"), Hex("#62D1B0"), 10, 20, true);
        CreateRoundedTexture(Asset("Card_Frame_Epic.png"), 256, 360, Hex("#352844"), Hex("#170E1F"), Hex("#C88CDA"), 10, 20, true);

        CreateRoundedTexture(Asset("RouteNode_Battle.png"), 160, 160, Hex("#34231D"), Hex("#151211"), Hex("#C5783A"), 8, 34, true);
        CreateRoundedTexture(Asset("RouteNode_Event.png"), 160, 160, Hex("#263A36"), Hex("#101D1B"), Hex("#62D1B0"), 8, 34, true);
        CreateRoundedTexture(Asset("RouteNode_Chest.png"), 160, 160, Hex("#392E18"), Hex("#1C160B"), Hex("#D7A94A"), 8, 34, true);
        CreateRoundedTexture(Asset("RouteNode_Rest.png"), 160, 160, Hex("#243726"), Hex("#101B13"), Hex("#86B66E"), 8, 34, true);
        CreateRoundedTexture(Asset("RouteNode_Market.png"), 160, 160, Hex("#35251C"), Hex("#17110D"), Hex("#B88945"), 8, 34, true);
        CreateDividerTexture(Asset("RouteLine_Gold.png"), 320, 22);

        CreateRoundedTexture(Asset("IconFrame_Jade.png"), 128, 128, Hex("#173A34"), Hex("#081817"), Hex("#B99654"), 8, 20, true);
        CreateDividerTexture(Asset("Divider_Gold.png"), 256, 18);
        CreateCornerTexture(Asset("Corner_Decoration_Gold.png"));
        CreateRoundedTexture(Asset("Popup_DarkJade_GoldBorder.png"), 420, 320, Hex("#102A25"), Hex("#071614"), Hex("#D0A85C"), 10, 28, true);
        CreateMaskTexture(Asset("Mask_Dark.png"));

        CopyGeneratedAsset("BG_DarkJade_Gradient.png", BackgroundDir);
        CopyGeneratedAsset("Panel_DarkJade_GoldBorder.png", PanelDir);
        CopyGeneratedAsset("Panel_Parchment_Dark.png", PanelDir);
        CopyGeneratedAsset("Button_Gold_Primary_Normal.png", ButtonDir);
        CopyGeneratedAsset("Button_Gold_Primary_Pressed.png", ButtonDir);
        CopyGeneratedAsset("Button_Gold_Primary_Disabled.png", ButtonDir);
        CopyGeneratedAsset("Button_Jade_Secondary_Normal.png", ButtonDir);
        CopyGeneratedAsset("Button_Jade_Secondary_Pressed.png", ButtonDir);
        CopyGeneratedAsset("Button_Jade_Secondary_Disabled.png", ButtonDir);
        CopyGeneratedAsset("Card_Frame_Common.png", CardDir);
        CopyGeneratedAsset("Card_Frame_Rare.png", CardDir);
        CopyGeneratedAsset("Card_Frame_Epic.png", CardDir);
        CopyGeneratedAsset("RouteNode_Battle.png", RouteNodeDir);
        CopyGeneratedAsset("RouteNode_Event.png", RouteNodeDir);
        CopyGeneratedAsset("RouteNode_Chest.png", RouteNodeDir);
        CopyGeneratedAsset("RouteNode_Rest.png", RouteNodeDir);
        CopyGeneratedAsset("RouteNode_Market.png", RouteNodeDir);
        CopyGeneratedAsset("IconFrame_Jade.png", IconFrameDir);
        CopyGeneratedAsset("Divider_Gold.png", DividerDir);
        CopyGeneratedAsset("Corner_Decoration_Gold.png", DecorationDir);
        CopyGeneratedAsset("Popup_DarkJade_GoldBorder.png", PopupDir);

        EnsureTmpFontAsset();

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
        Debug.Log($"Generated Cultivation golden prefab: {PrefabPath}");
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
        instance.name = "MainRunWindow_Golden_CaptureInstance";
        instance.hideFlags = HideFlags.HideAndDontSave;

        var cameraObject = new GameObject("CultivationGoldenSampleCamera");
        cameraObject.hideFlags = HideFlags.HideAndDontSave;
        var camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Hex("#07110F");
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

        foreach (var text in instance.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            text.font = LoadTmpFont();
            text.ForceMeshUpdate();
            text.SetAllDirty();
            text.SetVerticesDirty();
            text.SetLayoutDirty();
            text.SetMaterialDirty();
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

        Debug.Log($"Captured Cultivation golden sample: {screenshotPath}");
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
        var bg = AddImage("Background_DarkJade", root, LoadSprite("BG_Cultivation_DarkPattern.png"), RectSpec.Center(Vector2.zero, new Vector2(1920, 1080)));
        bg.raycastTarget = false;

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
            new Vector2(-870, 458),
            new Vector2(870, 458),
            new Vector2(-870, -410),
            new Vector2(870, -410)
        };

        for (var i = 0; i < positions.Length; i++)
        {
            var corner = AddImage("CornerDecoration_" + i, root, LoadSprite("Corner_Decoration_Gold.png"), RectSpec.Center(positions[i], new Vector2(92, 92)));
            corner.raycastTarget = false;
            corner.color = new Color(1f, 0.82f, 0.42f, 0.5f);
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
        var panel = AddPanel("TopInfoBar", root, RectSpec.Center(new Vector2(0, 474), new Vector2(1816, 90)), "Panel_DarkJade_GoldBorder.png");
        AddText("Title", panel.transform, "云隐散修", 30, Hex("#F0D78A"), TextAlignmentOptions.Left, RectSpec.Stretch(new Vector2(32, 0), new Vector2(300, 0)));
        AddInfoBlock(panel.transform, "境界", "炼气三层", new Vector2(-520, 0), Hex("#D9B56A"), 180);
        AddInfoBlock(panel.transform, "生命", "120/120", new Vector2(-304, 0), Hex("#D77062"), 204);
        AddInfoBlock(panel.transform, "灵力", "80/80", new Vector2(-82, 0), Hex("#58BFA4"), 190);
        AddInfoBlock(panel.transform, "灵石", "320", new Vector2(132, 0), Hex("#E3C56E"), 176);
        AddInfoBlock(panel.transform, "位置", "秘境一层", new Vector2(366, 0), Hex("#E8DFC6"), 226);
        AddInfoBlock(panel.transform, "天数", "第 3 日", new Vector2(604, 0), Hex("#E8DFC6"), 190);
    }

    private static void AddInfoBlock(Transform parent, string label, string value, Vector2 position, Color accent, float width)
    {
        var block = AddPanel("Info_" + label, parent, RectSpec.Center(position, new Vector2(width, 58)), "Panel_Parchment_Dark.png");
        AddText(label + "_Label", block.transform, label, 15, Hex("#A99F86"), TextAlignmentOptions.Left, RectSpec.Stretch(new Vector2(16, 8), new Vector2(90, 8)));
        AddText(label + "_Value", block.transform, value, 20, accent, TextAlignmentOptions.Right, RectSpec.Stretch(new Vector2(82, 7), new Vector2(18, 7)), FontStyles.Bold);
    }

    private static void AddLeftStatusPanel(Transform root)
    {
        var panel = AddPanel("Left_CharacterStatusPanel", root, RectSpec.Center(new Vector2(-725, 92), new Vector2(430, 728)), "Panel_DarkJade_GoldBorder.png");
        AddText("Header", panel.transform, "修行状态", 28, Hex("#F0D78A"), TextAlignmentOptions.Center, RectSpec.Top(new Vector2(0, -34), new Vector2(360, 42)), FontStyles.Bold);

        var avatar = AddPanel("AvatarFrame", panel.transform, RectSpec.Top(new Vector2(0, -142), new Vector2(174, 174)), "IconFrame_Jade.png");
        AddText("AvatarGlyph", avatar.transform, "灵", 74, Hex("#D9B56A"), TextAlignmentOptions.Center, RectSpec.Stretch(Vector2.zero, Vector2.zero), FontStyles.Bold);

        AddStatRow(panel.transform, "境界", "炼气三层", -250);
        AddStatRow(panel.transform, "心境", "平稳", -292);
        AddStatRow(panel.transform, "体魄", "凡骨", -334);
        AddStatRow(panel.transform, "灵根", "木火双灵根", -376);
        AddStatRow(panel.transform, "气血", "120 / 120", -418);
        AddStatRow(panel.transform, "灵力", "80 / 80", -460);

        AddText("BuffTitle", panel.transform, "状态", 21, Hex("#D9B56A"), TextAlignmentOptions.Left, RectSpec.Top(new Vector2(-138, -502), new Vector2(110, 34)), FontStyles.Bold);
        var buffNames = new[] { "护", "悟", "息", "运" };
        for (var i = 0; i < buffNames.Length; i++)
        {
            var icon = AddPanel("Buff_" + buffNames[i], panel.transform, RectSpec.Top(new Vector2(-118 + i * 72, -552), new Vector2(54, 54)), "IconFrame_Jade.png");
            AddText("Glyph", icon.transform, buffNames[i], 25, i == 1 ? Hex("#58BFA4") : Hex("#D9B56A"), TextAlignmentOptions.Center, RectSpec.Stretch(Vector2.zero, Vector2.zero), FontStyles.Bold);
        }

        var stateBox = AddPanel("CultivationNote", panel.transform, RectSpec.Bottom(new Vector2(0, 40), new Vector2(362, 74)), "Panel_Parchment_Dark.png");
        AddText("Note", stateBox.transform, "当前状态：秘境探索中。灵息环绕，适合继续前进，也需留意心境波动。", 17, Hex("#E8DFC6"), TextAlignmentOptions.TopLeft, RectSpec.Stretch(new Vector2(18, 10), new Vector2(18, 10)));
    }

    private static void AddStatRow(Transform parent, string label, string value, float y)
    {
        var row = AddPanel("Stat_" + label, parent, RectSpec.Top(new Vector2(0, y), new Vector2(342, 38)), "Panel_Parchment_Dark.png");
        AddText(label + "_Label", row.transform, label, 17, Hex("#A99F86"), TextAlignmentOptions.Left, RectSpec.Stretch(new Vector2(16, 2), new Vector2(130, 2)));
        AddText(label + "_Value", row.transform, value, 18, Hex("#E8DFC6"), TextAlignmentOptions.Right, RectSpec.Stretch(new Vector2(120, 2), new Vector2(16, 2)), FontStyles.Bold);
    }

    private static void AddRoutePanel(Transform root)
    {
        var panel = AddPanel("Center_RoutePanel", root, RectSpec.Center(new Vector2(-40, 92), new Vector2(820, 728)), "Panel_DarkJade_GoldBorder.png");
        AddText("Header", panel.transform, "秘境路线", 30, Hex("#F0D78A"), TextAlignmentOptions.Center, RectSpec.Top(new Vector2(0, -34), new Vector2(420, 46)), FontStyles.Bold);
        AddText("SubHeader", panel.transform, "固定种子样板 · 第 3 日", 17, Hex("#A99F86"), TextAlignmentOptions.Center, RectSpec.Top(new Vector2(0, -76), new Vector2(420, 28)));

        var points = new[]
        {
            new Vector2(-300, -84),
            new Vector2(-150, 66),
            new Vector2(20, 4),
            new Vector2(180, -106),
            new Vector2(330, 72)
        };

        for (var i = 0; i < points.Length - 1; i++)
        {
            AddRouteLine(panel.transform, points[i], points[i + 1], i < 1 ? Hex("#9E7741") : Hex("#35554C"));
        }

        AddRouteNode(panel.transform, "战斗", "剑", "RouteNode_Battle.png", points[0], "已完成", NodeState.Done);
        AddRouteNode(panel.transform, "秘境", "卷", "RouteNode_Event.png", points[1], "当前", NodeState.Current);
        AddRouteNode(panel.transform, "宝箱", "宝", "RouteNode_Chest.png", points[2], "未到达", NodeState.Locked);
        AddRouteNode(panel.transform, "休息", "息", "RouteNode_Rest.png", points[3], "未到达", NodeState.Locked);
        AddRouteNode(panel.transform, "市场", "市", "RouteNode_Market.png", points[4], "未到达", NodeState.Locked);

        var note = AddPanel("RouteNote", panel.transform, RectSpec.Bottom(new Vector2(0, 72), new Vector2(700, 110)), "Panel_Parchment_Dark.png");
        AddText("RouteNoteText", note.transform, "当前节点：古井灵光。选择探索可获得灵力、功法或临时状态；失败也可能损失气血。", 20, Hex("#E8DFC6"), TextAlignmentOptions.TopLeft, RectSpec.Stretch(new Vector2(22, 16), new Vector2(22, 16)));
    }

    private static void AddRouteLine(Transform parent, Vector2 from, Vector2 to, Color color)
    {
        var delta = to - from;
        var line = AddImage("RouteLine", parent, LoadSprite("RouteLine_Gold.png"), RectSpec.Center((from + to) * 0.5f, new Vector2(delta.magnitude, 10)));
        line.color = color;
        line.rectTransform.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
    }

    private enum NodeState
    {
        Done,
        Current,
        Locked
    }

    private static void AddRouteNode(Transform parent, string label, string glyph, string spriteName, Vector2 position, string status, NodeState state)
    {
        if (state == NodeState.Current)
        {
            var glow = AddImage("Glow_" + label, parent, LoadSprite("RouteNode_Event.png"), RectSpec.Center(position, new Vector2(148, 148)));
            glow.color = new Color(0.9f, 0.75f, 0.28f, 0.32f);
            glow.raycastTarget = false;
        }

        var node = AddImage("RouteNode_" + label, parent, LoadSprite(spriteName), RectSpec.Center(position, new Vector2(112, 112)));
        node.type = Image.Type.Sliced;
        node.color = state == NodeState.Locked ? new Color(0.56f, 0.62f, 0.56f, 0.62f) : Color.white;
        AddText("Icon", node.transform, glyph, 36, state == NodeState.Current ? Hex("#F0D78A") : Hex("#E8DFC6"), TextAlignmentOptions.Center, RectSpec.Top(new Vector2(0, -14), new Vector2(78, 46)), FontStyles.Bold);
        AddText("Label", node.transform, label, 18, Hex("#E8DFC6"), TextAlignmentOptions.Center, RectSpec.Bottom(new Vector2(0, 24), new Vector2(98, 26)), FontStyles.Bold);
        AddText("Status", node.transform, status, 13, state == NodeState.Current ? Hex("#58BFA4") : Hex("#A99F86"), TextAlignmentOptions.Center, RectSpec.Bottom(new Vector2(0, 5), new Vector2(98, 20)));
    }

    private static void AddCardPreviewPanel(Transform root)
    {
        var panel = AddPanel("Right_CardRewardPreviewPanel", root, RectSpec.Center(new Vector2(680, 92), new Vector2(470, 728)), "Panel_DarkJade_GoldBorder.png");
        AddText("Header", panel.transform, "功法预览", 28, Hex("#F0D78A"), TextAlignmentOptions.Center, RectSpec.Top(new Vector2(0, -34), new Vector2(300, 42)), FontStyles.Bold);
        AddText("Hint", panel.transform, "奖励池样板 · 非真实掉落", 16, Hex("#A99F86"), TextAlignmentOptions.Center, RectSpec.Top(new Vector2(0, -72), new Vector2(300, 28)));

        AddCard(panel.transform, "青木剑诀", "1", "剑诀 · 凡品", "造成 8 点伤害。\n灵力充足时获得剑意。", new Vector2(-145, 88), false, false);
        AddCard(panel.transform, "聚气术", "0", "心法 · 灵品", "恢复 12 点灵力。\n抽 1 张牌。", new Vector2(0, 88), true, false);
        AddCard(panel.transform, "护身符", "1", "符箓 · 凡品", "获得 10 点护盾。\n本回合减伤。", new Vector2(145, 88), false, true);

        var reward = AddPanel("RewardPreview", panel.transform, RectSpec.Bottom(new Vector2(0, 86), new Vector2(390, 142)), "Panel_Parchment_Dark.png");
        AddText("RewardTitle", reward.transform, "可能奖励", 21, Hex("#D9B56A"), TextAlignmentOptions.Left, RectSpec.Top(new Vector2(-118, -18), new Vector2(130, 32)), FontStyles.Bold);
        AddRewardItem(reward.transform, "灵", "灵石 +45", new Vector2(-96, -12));
        AddRewardItem(reward.transform, "诀", "随机功法", new Vector2(94, -12));
        AddRewardItem(reward.transform, "息", "心境稳定", new Vector2(-96, -48));
        AddRewardItem(reward.transform, "玉", "秘境线索", new Vector2(94, -48));
    }

    private static void AddCard(Transform parent, string title, string cost, string type, string desc, Vector2 position, bool rare, bool epic)
    {
        var sprite = epic ? "Card_Frame_Epic.png" : rare ? "Card_Frame_Rare.png" : "Card_Frame_Common.png";
        var card = AddImage("Card_" + title, parent, LoadSprite(sprite), RectSpec.Center(position, new Vector2(130, 250)));
        card.type = Image.Type.Sliced;

        var costBadge = AddPanel("Cost", card.transform, RectSpec.Top(new Vector2(-43, -24), new Vector2(38, 38)), "IconFrame_Jade.png");
        AddText("CostText", costBadge.transform, cost, 19, Hex("#F0D78A"), TextAlignmentOptions.Center, RectSpec.Stretch(Vector2.zero, Vector2.zero), FontStyles.Bold);
        AddText("Title", card.transform, title, 17, Hex("#F0D78A"), TextAlignmentOptions.Center, RectSpec.Top(new Vector2(14, -26), new Vector2(80, 38)), FontStyles.Bold);

        var art = AddPanel("ArtFrame", card.transform, RectSpec.Top(new Vector2(0, -88), new Vector2(92, 72)), "IconFrame_Jade.png");
        AddText("ArtGlyph", art.transform, rare ? "气" : epic ? "符" : "诀", 26, rare ? Hex("#58BFA4") : Hex("#D9B56A"), TextAlignmentOptions.Center, RectSpec.Stretch(Vector2.zero, Vector2.zero), FontStyles.Bold);

        AddText("Type", card.transform, type, 13, rare ? Hex("#58BFA4") : Hex("#A99F86"), TextAlignmentOptions.Center, RectSpec.Top(new Vector2(0, -138), new Vector2(108, 24)));
        AddText("Desc", card.transform, desc, 13, Hex("#E8DFC6"), TextAlignmentOptions.Top, RectSpec.Bottom(new Vector2(0, 13), new Vector2(102, 82)));
    }

    private static void AddRewardItem(Transform parent, string glyph, string text, Vector2 position)
    {
        var item = CreateRect("Reward_" + text, parent, RectSpec.Center(position, new Vector2(166, 40)));
        var icon = AddPanel("Icon", item.transform, RectSpec.Left(new Vector2(18, 0), new Vector2(34, 34)), "IconFrame_Jade.png");
        AddText("Glyph", icon.transform, glyph, 16, Hex("#D9B56A"), TextAlignmentOptions.Center, RectSpec.Stretch(Vector2.zero, Vector2.zero), FontStyles.Bold);
        AddText("Text", item.transform, text, 15, Hex("#E8DFC6"), TextAlignmentOptions.Left, RectSpec.Stretch(new Vector2(46, 0), new Vector2(4, 0)));
    }

    private static void AddBottomActions(Transform root)
    {
        var panel = AddPanel("Bottom_ActionBar", root, RectSpec.Center(new Vector2(0, -455), new Vector2(1816, 126)), "Panel_DarkJade_GoldBorder.png");
        AddButton(panel.transform, "Primary_Continue", "继续前进", new Vector2(575, 0), new Vector2(310, 72), true);
        AddButton(panel.transform, "Button_Bag", "背包", new Vector2(-300, 0), new Vector2(150, 58), false);
        AddButton(panel.transform, "Button_Skill", "功法", new Vector2(-120, 0), new Vector2(150, 58), false);
        AddButton(panel.transform, "Button_Settings", "设置", new Vector2(60, 0), new Vector2(150, 58), false);
        AddButton(panel.transform, "Button_Back", "返回", new Vector2(240, 0), new Vector2(150, 58), false);
        AddText("FooterNote", panel.transform, "黄金样板 · 假数据 · 用于 MainRun / Route / Event / Battle 视觉基线", 16, Hex("#A99F86"), TextAlignmentOptions.Left, RectSpec.Stretch(new Vector2(34, 0), new Vector2(950, 0)));
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
        var reward = AddPanel("PopupRewardPreview", panel.transform, RectSpec.Bottom(new Vector2(0, 34), new Vector2(560, 70)), "Panel_Parchment_Dark.png");
        AddText("RewardText", reward.transform, "奖励预览：灵石 +20 / 心境波动", 19, Hex("#D9B56A"), TextAlignmentOptions.Center, RectSpec.Stretch(new Vector2(18, 4), new Vector2(18, 4)), FontStyles.Bold);
        popup.SetActive(false);
    }

    private static void AddButton(Transform parent, string name, string label, Vector2 position, Vector2 size, bool primary)
    {
        var sprite = LoadSprite(primary ? "Button_Gold_Primary_Normal.png" : "Button_Jade_Secondary_Normal.png");
        var image = AddImage(name, parent, sprite, RectSpec.Center(position, size));
        image.type = Image.Type.Sliced;

        var button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.SpriteSwap;
        var state = button.spriteState;
        state.pressedSprite = LoadSprite(primary ? "Button_Gold_Primary_Pressed.png" : "Button_Jade_Secondary_Pressed.png");
        state.disabledSprite = LoadSprite(primary ? "Button_Gold_Primary_Disabled.png" : "Button_Jade_Secondary_Disabled.png");
        button.spriteState = state;

        AddText("Label", image.transform, label, primary ? 24 : 20, primary ? Hex("#23180A") : Hex("#E8DFC6"), TextAlignmentOptions.Center, RectSpec.Stretch(Vector2.zero, Vector2.zero), FontStyles.Bold);
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

    private static TextMeshProUGUI AddText(string name, Transform parent, string text, float size, Color color, TextAlignmentOptions alignment, RectSpec spec, FontStyles style = FontStyles.Normal)
    {
        var rect = CreateRect(name, parent, spec);
        var label = rect.gameObject.AddComponent<TextMeshProUGUI>();
        label.font = LoadTmpFont();
        label.text = text;
        label.fontSize = size;
        label.color = color;
        label.alignment = alignment;
        label.fontStyle = style;
        label.enableWordWrapping = true;
        label.overflowMode = TextOverflowModes.Truncate;
        label.raycastTarget = false;
        label.enableCulling = false;

        var shadow = rect.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.55f);
        shadow.effectDistance = new Vector2(1.5f, -1.5f);

        return label;
    }

    private static RectTransform CreateRect(string name, Transform parent, RectSpec spec)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rect = (RectTransform)go.transform;
        rect.SetParent(parent, false);
        spec.Apply(rect);
        return rect;
    }

    private static TMP_FontAsset LoadTmpFont()
    {
        if (_tmpFont != null)
        {
            return _tmpFont;
        }

        _tmpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TmpFontAssetPath);
        if (_tmpFont == null)
        {
            _tmpFont = EnsureTmpFontAsset();
        }

        return _tmpFont;
    }

    private static TMP_FontAsset EnsureTmpFontAsset()
    {
        var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TmpFontAssetPath);
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
            Debug.LogWarning($"Phase77A TMP font source is missing: {FontSourcePath}. Import NotoSansCJKsc-VF.ttf before generating the golden sample.");
            return TMP_Settings.defaultFontAsset;
        }

        var fontAsset = TMP_FontAsset.CreateFontAsset(font, 90, 9, GlyphRenderMode.SDFAA, 4096, 4096, AtlasPopulationMode.Dynamic, true);
        if (fontAsset == null)
        {
            Debug.LogWarning("Failed to create TMP font asset for Phase77A. Falling back to TMP default font.");
            return TMP_Settings.defaultFontAsset;
        }

        fontAsset.name = "NotoSansCJKsc_Phase77A SDF";
        fontAsset.isMultiAtlasTexturesEnabled = true;
        AssetDatabase.CreateAsset(fontAsset, TmpFontAssetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(TmpFontAssetPath, ImportAssetOptions.ForceUpdate);

        _tmpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TmpFontAssetPath);
        return _tmpFont != null ? _tmpFont : fontAsset;
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

    private static void CreateBackgroundTexture(string assetPath, bool strongerPattern)
    {
        var width = 1920;
        var height = 1080;
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        var top = strongerPattern ? Hex("#12362E") : Hex("#102A25");
        var bottom = Hex("#06100F");
        var edge = Hex("#020706");

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var t = y / (float)(height - 1);
                var color = Color.Lerp(bottom, top, t);
                var nx = (x - width * 0.5f) / (width * 0.5f);
                var ny = (y - height * 0.5f) / (height * 0.5f);
                var vignette = Mathf.Clamp01((nx * nx + ny * ny) * 0.64f);
                color = Color.Lerp(color, edge, vignette);
                var veinA = Mathf.Sin((x * 0.018f) + Mathf.Sin(y * 0.012f) * 3.0f) * (strongerPattern ? 0.036f : 0.025f);
                var veinB = Mathf.Sin((x + y) * 0.011f) * (strongerPattern ? 0.026f : 0.014f);
                var grain = Hash01(x, y) * (strongerPattern ? 0.034f : 0.028f);
                color.r = Mathf.Clamp01(color.r + veinA + veinB + grain);
                color.g = Mathf.Clamp01(color.g + veinA * 0.8f + veinB + grain);
                color.b = Mathf.Clamp01(color.b + veinA * 0.6f + veinB * 0.8f + grain * 0.7f);
                texture.SetPixel(x, y, color);
            }
        }

        SaveTexture(texture, assetPath, Vector4.zero);
    }

    private static void CreateRoundedTexture(string assetPath, int width, int height, Color top, Color bottom, Color border, int borderWidth, int radius, bool noisy)
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
                    var innerShadow = Mathf.Clamp01((borderWidth + 20 - edge) / 30f) * 0.18f;
                    color = Color.Lerp(color, Color.black, innerShadow);
                    if (noisy)
                    {
                        var grain = (Hash01(x, y) - 0.5f) * 0.045f;
                        color.r = Mathf.Clamp01(color.r + grain);
                        color.g = Mathf.Clamp01(color.g + grain);
                        color.b = Mathf.Clamp01(color.b + grain);
                    }
                }

                texture.SetPixel(x, y, color);
            }
        }

        SaveTexture(texture, assetPath, new Vector4(radius, radius, radius, radius));
    }

    private static void CreateDividerTexture(string assetPath, int width, int height)
    {
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        var color = Hex("#C59A49");
        for (var y = 0; y < texture.height; y++)
        {
            for (var x = 0; x < texture.width; x++)
            {
                var alpha = Mathf.SmoothStep(0, 1, Mathf.Min(x, texture.width - 1 - x) / 38f);
                alpha *= 1f - Mathf.Abs(y - texture.height * 0.5f) / (texture.height * 0.55f);
                texture.SetPixel(x, y, new Color(color.r, color.g, color.b, Mathf.Clamp01(alpha)));
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
                var inCorner = (x < 14 && y < 78) || (y < 14 && x < 78) || (x - y > 38 && x < 76 && y < 30);
                var inner = (x > 22 && x < 30 && y < 64) || (y > 22 && y < 30 && x < 64);
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
