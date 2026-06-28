#if UNITY_EDITOR
using System;
using System.IO;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class CultivationUIPhase78BMainRouteGenerator
{
    private const string ThemeRoot = "Assets/AssetRaw/UIRaw/Theme/Cultivation";
    private const string PhaseDir = ThemeRoot + "/Phase78B";
    private const string FinalPhaseDir = ThemeRoot + "/Phase78C";
    private const string TemplateDir = ThemeRoot + "/Templates/SteamDemo";
    private const string FontPath = "Assets/AssetRaw/Fonts/NotoSansCJKsc-VF.ttf";
    private const string PrefabName = "Steam_MainRoute_Phase78B";
    private const string PrefabPath = TemplateDir + "/" + PrefabName + ".prefab";
    private const string ScreenshotName = "Steam_MainRoute_Concept_Phase78B.png";
    private const string FinalPrefabName = "Steam_MainRoute_Final";
    private const string FinalPrefabPath = TemplateDir + "/" + FinalPrefabName + ".prefab";
    private const string FinalScreenshotName = "Steam_MainRoute_Final.png";

    private static Font _font;
    private static string _activePhaseDir = PhaseDir;

    [MenuItem("Codex/Cultivation UI/Phase78B/Generate MainRoute Ref Replica And Screenshot")]
    public static void GenerateMainRouteRefReplicaAndScreenshot()
    {
        GenerateMainRoutePrefab();
        CaptureMainRouteScreenshot();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Phase78B MainRoute reference replica generated.");
    }

    [MenuItem("Codex/Cultivation UI/Phase78C/Generate MainRoute Final Visual Baseline And Screenshot")]
    public static void GenerateMainRouteFinalAndScreenshot()
    {
        GenerateMainRouteFinalPrefab();
        CaptureMainRouteFinalScreenshot();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Phase78C MainRoute final visual baseline generated.");
    }

    [MenuItem("Codex/Cultivation UI/Phase78B/Generate MainRoute Ref Replica Prefab")]
    public static void GenerateMainRoutePrefab()
    {
        EnsureDirectories();
        _activePhaseDir = PhaseDir;
        GeneratePhaseTextures(PhaseDir);

        var root = CreateRoot(PrefabName);
        BuildMainRoute(root.transform);

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        UnityEngine.Object.DestroyImmediate(root);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Generated Phase78B MainRoute prefab: " + PrefabPath);
    }

    [MenuItem("Codex/Cultivation UI/Phase78C/Generate MainRoute Final Visual Baseline Prefab")]
    public static void GenerateMainRouteFinalPrefab()
    {
        EnsureDirectories();
        _activePhaseDir = FinalPhaseDir;
        GeneratePhaseTextures(FinalPhaseDir);

        var root = CreateRoot(FinalPrefabName);
        BuildMainRoute(root.transform);

        PrefabUtility.SaveAsPrefabAsset(root, FinalPrefabPath);
        UnityEngine.Object.DestroyImmediate(root);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Generated Phase78C MainRoute final prefab: " + FinalPrefabPath);
    }

    [MenuItem("Codex/Cultivation UI/Phase78B/Capture MainRoute Ref Replica Screenshot")]
    public static string CaptureMainRouteScreenshot()
    {
        return CaptureMainRoutePrefab(PrefabPath, PrefabName, ScreenshotName, "Phase78B");
    }

    [MenuItem("Codex/Cultivation UI/Phase78C/Capture MainRoute Final Visual Baseline Screenshot")]
    public static string CaptureMainRouteFinalScreenshot()
    {
        return CaptureMainRoutePrefab(FinalPrefabPath, FinalPrefabName, FinalScreenshotName, "Phase78C");
    }

    private static string CaptureMainRoutePrefab(string prefabPath, string prefabName, string screenshotName, string phaseName)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            if (prefabName == FinalPrefabName)
            {
                GenerateMainRouteFinalPrefab();
                _activePhaseDir = FinalPhaseDir;
            }
            else
            {
                GenerateMainRoutePrefab();
                _activePhaseDir = PhaseDir;
            }
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        }

        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.name = prefabName + "_CaptureInstance";
        instance.hideFlags = HideFlags.HideAndDontSave;

        var cameraObject = new GameObject(prefabName + "_CaptureCamera");
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
        var screenshotPath = Path.Combine(screenshotDir, screenshotName);
        WriteAllBytesWithRetry(screenshotPath, texture.EncodeToPNG());

        camera.targetTexture = previousTarget;
        RenderTexture.active = previousActive;
        UnityEngine.Object.DestroyImmediate(texture);
        UnityEngine.Object.DestroyImmediate(rt);
        UnityEngine.Object.DestroyImmediate(instance);
        UnityEngine.Object.DestroyImmediate(cameraObject);

        Debug.Log("Captured " + phaseName + " MainRoute screenshot: " + screenshotPath);
        return screenshotPath;
    }

    private static void BuildMainRoute(Transform root)
    {
        AddImage("Background_TableMist", root, PhaseSprite("BG_Phase78B_TableMist.png"), RectSpec.Stretch(Vector2.zero, Vector2.zero)).raycastTarget = false;
        AddImage("Background_Vignette", root, SpriteAt("Background/BG_Cultivation_Vignette.png"), RectSpec.Stretch(Vector2.zero, Vector2.zero)).color = new Color(1f, 1f, 1f, 0.88f);

        AddTopBar(root);
        AddCharacterArchive(root);
        AddScrollRouteMap(root);
        AddCardPreviewPanel(root);
        AddBottomActions(root);
    }

    private static void AddTopBar(Transform root)
    {
        var bar = AddImage("Top_ThinResourceBar", root, SpriteAt("Panel/Panel_Bottom_Transparent.png"), RectSpec.Top(new Vector2(0, -36), new Vector2(1920, 72)));
        bar.type = Image.Type.Sliced;
        bar.color = new Color(0.02f, 0.025f, 0.024f, 0.92f);
        AddImage("Top_BottomGoldLine", root, SpriteAt("Divider/Divider_InkGold.png"), RectSpec.Top(new Vector2(0, -72), new Vector2(1920, 5))).color = new Color(0.82f, 0.62f, 0.34f, 0.42f);

        AddText("Top_Brand", root, "凌云舟", 36, Hex("#D9BD84"), TextAnchor.MiddleLeft, RectSpec.Top(new Vector2(-812, -35), new Vector2(260, 54)), FontStyle.Bold);
        AddText("Top_Seal", root, "秘", 24, Hex("#C5A66B"), TextAnchor.MiddleCenter, RectSpec.Top(new Vector2(-925, -35), new Vector2(46, 46)), FontStyle.Bold);
        AddResource(root, "气血", "72/72", Hex("#C95A4F"), new Vector2(-430, -36), "●");
        AddResource(root, "灵力", "48/56", Hex("#65BFD3"), new Vector2(-120, -36), "◎");
        AddResource(root, "灵石", "312", Hex("#76D4A1"), new Vector2(210, -36), "◆");
        AddText("Top_Location", root, "云雾秘境 · 第一层，第 7 天", 28, Hex("#D9C69B"), TextAnchor.MiddleRight, RectSpec.Top(new Vector2(595, -35), new Vector2(560, 50)), FontStyle.Bold);
        AddText("Top_Gear", root, "⚙", 38, Hex("#BFA16B"), TextAnchor.MiddleCenter, RectSpec.Top(new Vector2(910, -36), new Vector2(48, 48)), FontStyle.Bold);
    }

    private static void AddResource(Transform root, string label, string value, Color color, Vector2 topPosition, string icon)
    {
        AddText("Top_" + label + "_Icon", root, icon, 24, color, TextAnchor.MiddleCenter, RectSpec.Top(topPosition + new Vector2(-70, 0), new Vector2(34, 44)), FontStyle.Bold);
        AddText("Top_" + label, root, label + "   " + value, 25, Hex("#D9C69B"), TextAnchor.MiddleLeft, RectSpec.Top(topPosition, new Vector2(190, 46)), FontStyle.Bold);
        AddImage("Top_" + label + "_BarBack", root, SpriteAt("Panel/Panel_Bottom_Transparent.png"), RectSpec.Top(topPosition + new Vector2(56, -25), new Vector2(175, 8))).color = new Color(0f, 0f, 0f, 0.55f);
        AddImage("Top_" + label + "_BarFill", root, SpriteAt("Divider/Divider_InkGold.png"), RectSpec.Top(topPosition + new Vector2(32, -25), new Vector2(128, 5))).color = color;
    }

    private static void AddCharacterArchive(Transform root)
    {
        var panel = AddImage("Left_CultivatorArchivePanel", root, SpriteAt("Panel/Panel_Main_JadeThin.png"), RectSpec.Center(new Vector2(-728, 18), new Vector2(390, 842)));
        panel.type = Image.Type.Sliced;
        panel.color = new Color(0.62f, 0.67f, 0.58f, 0.96f);

        AddImage("Left_InnerParchment", root, SpriteAt("Panel/Panel_Secondary_Parchment.png"), RectSpec.Top(new Vector2(-728, -104), new Vector2(338, 372))).type = Image.Type.Sliced;
        AddImage("Left_Portrait", root, PhaseSprite("Portrait_Phase78B_Cultivator.png"), RectSpec.Top(new Vector2(-728, -118), new Vector2(306, 344)));
        AddImage("Left_PortraitShade", root, SpriteAt("Background/BG_Cultivation_Vignette.png"), RectSpec.Top(new Vector2(-728, -118), new Vector2(306, 344))).color = new Color(0f, 0f, 0f, 0.18f);
        AddText("Left_ClassStamp", root, "修\n士", 44, Hex("#19140C"), TextAnchor.MiddleCenter, RectSpec.Top(new Vector2(-872, -162), new Vector2(76, 124)), FontStyle.Bold);
        AddText("Left_Name", root, "凌云子", 30, Hex("#F0DEB8"), TextAnchor.MiddleCenter, RectSpec.Top(new Vector2(-728, -382), new Vector2(220, 42)), FontStyle.Bold);
        AddImage("Left_NameLine", root, SpriteAt("Divider/Divider_InkGold.png"), RectSpec.Top(new Vector2(-728, -424), new Vector2(308, 4))).color = new Color(0.86f, 0.68f, 0.36f, 0.34f);
        AddText("Left_RealmTitle", root, "境界", 18, Hex("#BBA26D"), TextAnchor.MiddleLeft, RectSpec.Top(new Vector2(-865, -432), new Vector2(90, 28)));
        AddText("Left_RealmValue", root, "筑基中期", 22, Hex("#E8D8B2"), TextAnchor.MiddleRight, RectSpec.Top(new Vector2(-645, -432), new Vector2(150, 30)), FontStyle.Bold);

        AddProfileLine(root, "气血", "72/72", -476, "♥");
        AddProfileLine(root, "灵力", "48/56", -520, "◎");
        AddProfileLine(root, "攻击", "14", -564, "╱");
        AddProfileLine(root, "防御", "9", -608, "◇");
        AddProfileLine(root, "身法", "16", -652, "↯");
        AddProfileLine(root, "悟性", "12", -696, "▰");

        AddText("Left_StatusTitle", root, "当前状态", 20, Hex("#D2B76F"), TextAnchor.MiddleLeft, RectSpec.Top(new Vector2(-815, -748), new Vector2(180, 28)), FontStyle.Bold);
        AddStatusTag(root, "清\n心", "剩余\n2天", new Vector2(-825, -804), Hex("#2B663A"));
        AddStatusTag(root, "御\n剑", "剩余\n3天", new Vector2(-728, -804), Hex("#24546B"));
        AddStatusTag(root, "灵\n动", "剩余\n1天", new Vector2(-631, -804), Hex("#43356E"));
    }

    private static void AddProfileLine(Transform root, string label, string value, float topY, string icon)
    {
        AddText("Left_Icon_" + label, root, icon, 20, Hex("#BCA276"), TextAnchor.MiddleCenter, RectSpec.Top(new Vector2(-880, topY), new Vector2(28, 28)), FontStyle.Bold);
        AddText("Left_Label_" + label, root, label, 20, Hex("#BBAE8A"), TextAnchor.MiddleLeft, RectSpec.Top(new Vector2(-835, topY), new Vector2(90, 30)));
        AddText("Left_Value_" + label, root, value, 22, Hex("#E8D8B2"), TextAnchor.MiddleRight, RectSpec.Top(new Vector2(-654, topY), new Vector2(135, 30)), FontStyle.Bold);
        AddImage("Left_Line_" + label, root, SpriteAt("Divider/Divider_InkGold.png"), RectSpec.Top(new Vector2(-730, topY - 32), new Vector2(300, 3))).color = new Color(0.79f, 0.61f, 0.31f, 0.18f);
    }

    private static void AddStatusTag(Transform root, string glyph, string days, Vector2 topPosition, Color color)
    {
        var tag = AddImage("Left_StatusTag_" + glyph.Replace("\n", ""), root, SpriteAt("Panel/Panel_Bamboo_Tag.png"), RectSpec.Top(topPosition, new Vector2(68, 128)));
        tag.type = Image.Type.Sliced;
        tag.color = color;
        AddText("Left_StatusGlyph_" + glyph.Replace("\n", ""), root, glyph, 29, Hex("#E9D7AD"), TextAnchor.MiddleCenter, RectSpec.Top(topPosition + new Vector2(0, -8), new Vector2(60, 78)), FontStyle.Bold);
        AddText("Left_StatusDays_" + glyph.Replace("\n", ""), root, days, 15, Hex("#D9C99B"), TextAnchor.MiddleCenter, RectSpec.Top(topPosition + new Vector2(0, -94), new Vector2(66, 42)));
    }

    private static void AddScrollRouteMap(Transform root)
    {
        AddImage("Center_MapShadow", root, SpriteAt("Panel/Panel_Bottom_Transparent.png"), RectSpec.Center(new Vector2(10, -18), new Vector2(1080, 802))).color = new Color(0f, 0f, 0f, 0.34f);
        AddImage("Center_LeftScrollRoller", root, PhaseSprite("ScrollRoller_Phase78B.png"), RectSpec.Center(new Vector2(-508, 18), new Vector2(78, 828)));
        AddImage("Center_RightScrollRoller", root, PhaseSprite("ScrollRoller_Phase78B.png"), RectSpec.Center(new Vector2(508, 18), new Vector2(78, 828)));
        var map = AddImage("Center_ParchmentRouteMap", root, PhaseSprite("Map_Phase78B_ParchmentScroll.png"), RectSpec.Center(new Vector2(0, 18), new Vector2(986, 760)));
        map.color = new Color(1f, 0.98f, 0.88f, 1f);

        AddText("Center_MapVerticalTitle", root, "云\n雾\n秘\n境", 42, Hex("#171209"), TextAnchor.MiddleCenter, RectSpec.Center(new Vector2(-390, 166), new Vector2(86, 260)), FontStyle.Bold);
        AddText("Center_MapSeal", root, "印", 20, Hex("#9F3829"), TextAnchor.MiddleCenter, RectSpec.Center(new Vector2(-382, 16), new Vector2(30, 30)), FontStyle.Bold);

        var hub = new Vector2(50, -4);
        var battle = new Vector2(26, 214);
        var eventNode = new Vector2(-250, 112);
        var chest = new Vector2(302, 92);
        var rest = new Vector2(-248, -226);
        var market = new Vector2(248, -248);

        AddSpiritCurve(root, hub, battle, new Vector2(78, 118), "Center_Path_Hub_Battle");
        AddSpiritCurve(root, hub, eventNode, new Vector2(-98, 104), "Center_Path_Hub_Event");
        AddSpiritCurve(root, hub, chest, new Vector2(226, 48), "Center_Path_Hub_Chest");
        AddSpiritCurve(root, hub, rest, new Vector2(-90, -126), "Center_Path_Hub_Rest");
        AddSpiritCurve(root, hub, market, new Vector2(174, -136), "Center_Path_Hub_Market");
        AddSpiritCurve(root, eventNode, battle, new Vector2(-94, 240), "Center_Path_Event_Battle");

        AddMapNode(root, "Center_Node_Event", "奇遇", eventNode, "RouteNode/Node_Event_Scroll.png", false);
        AddMapNode(root, "Center_Node_Battle", "战斗", battle, "RouteNode/Node_Battle_Sword.png", false);
        AddMapNode(root, "Center_Node_Chest", "宝箱", chest, "RouteNode/Node_Chest_Box.png", false);
        AddMapNode(root, "Center_Node_Rest", "休憩", rest, "RouteNode/Node_Rest_Meditation.png", false);
        AddMapNode(root, "Center_Node_Market", "坊市", market, "RouteNode/Node_Market_Pavilion.png", false);
        AddMapNode(root, "Center_Node_Current", "", hub, "RouteNode/Node_Rest_Meditation.png", true);
        AddText("Center_CurrentPin", root, "◆", 30, Hex("#FFDF80"), TextAnchor.MiddleCenter, RectSpec.Center(hub + new Vector2(0, 72), new Vector2(36, 36)), FontStyle.Bold);
    }

    private static void AddSpiritCurve(Transform root, Vector2 from, Vector2 to, Vector2 control, string name)
    {
        var last = from;
        const int steps = 18;
        for (var i = 1; i <= steps; i++)
        {
            var t = i / (float)steps;
            var p = Quadratic(from, control, to, t);
            AddSpiritSegment(root, last, p, name + "_" + i, 22, new Color(1f, 0.82f, 0.35f, 0.24f));
            AddSpiritSegment(root, last, p, name + "_Core_" + i, 11, new Color(1f, 0.92f, 0.58f, 0.78f));
            last = p;
        }
    }

    private static void AddSpiritSegment(Transform root, Vector2 from, Vector2 to, string name, float height, Color color)
    {
        var center = (from + to) * 0.5f;
        var size = new Vector2(Vector2.Distance(from, to), height);
        var line = AddImage(name, root, PhaseSprite("SpiritLine_Phase78B.png"), RectSpec.Center(center, size));
        line.color = color;
        line.raycastTarget = false;
        line.rectTransform.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(to.y - from.y, to.x - from.x) * Mathf.Rad2Deg);
    }

    private static void AddMapNode(Transform root, string name, string label, Vector2 position, string iconPath, bool current)
    {
        if (current)
        {
            AddImage(name + "_OuterGlow", root, SpriteAt("RouteNode/Node_Current_Glow.png"), RectSpec.Center(position, new Vector2(214, 214))).color = new Color(1f, 0.74f, 0.20f, 0.78f);
            AddImage(name + "_MiddleGlow", root, SpriteAt("RouteNode/Node_Current_Glow.png"), RectSpec.Center(position, new Vector2(166, 166))).color = new Color(1f, 0.88f, 0.42f, 0.70f);
            AddImage(name + "_FocusRing", root, SpriteAt("RouteNode/Node_Current_Glow.png"), RectSpec.Center(position, new Vector2(126, 126))).color = new Color(1f, 0.96f, 0.66f, 0.90f);
            AddImage(name + "_Island", root, PhaseSprite("MapCenter_Island_Phase78B.png"), RectSpec.Center(position, new Vector2(104, 104)));
            return;
        }

        AddImage(name + "_Glow", root, SpriteAt("RouteNode/Node_Current_Glow.png"), RectSpec.Center(position, new Vector2(138, 138))).color = new Color(1f, 0.78f, 0.35f, 0.32f);
        AddImage(name + "_Icon", root, SpriteAt(iconPath), RectSpec.Center(position, new Vector2(104, 104)));
        var labelBg = AddImage(name + "_InkLabel", root, SpriteAt("Panel/Panel_Bottom_Transparent.png"), RectSpec.Center(position + new Vector2(0, -63), new Vector2(114, 34)));
        labelBg.type = Image.Type.Sliced;
        labelBg.color = new Color(0.03f, 0.025f, 0.018f, 0.78f);
        AddText(name + "_Label", root, label, 27, Hex("#F2DFA8"), TextAnchor.MiddleCenter, RectSpec.Center(position + new Vector2(0, -64), new Vector2(106, 34)), FontStyle.Bold);
    }

    private static void AddCardPreviewPanel(Transform root)
    {
        var panel = AddImage("Right_CurrentHandPanel", root, SpriteAt("Panel/Panel_Main_JadeThin.png"), RectSpec.Center(new Vector2(730, 18), new Vector2(395, 842)));
        panel.type = Image.Type.Sliced;
        panel.color = new Color(0.74f, 0.78f, 0.68f, 0.96f);
        AddText("Right_Title", root, "当前手牌  (3/3)", 25, Hex("#DFC789"), TextAnchor.MiddleLeft, RectSpec.Top(new Vector2(730, -118), new Vector2(310, 42)), FontStyle.Bold);

        AddPreviewCard(root, "清风剑诀", "1", "剑法", "造成12点剑系伤害。\n若本回合未使用攻击牌，则抽1张牌。", "CardArt_Phase78B_Sword.png", new Vector2(730, 242), Hex("#1C6374"));
        AddPreviewCard(root, "幻影步", "1", "身法", "获得8点护甲。\n抽1张牌。", "CardArt_Phase78B_Step.png", new Vector2(730, -18), Hex("#28673D"));
        AddPreviewCard(root, "混元归一诀", "2", "心法", "恢复10点灵力。\n若灵力全满，抽2张牌。", "CardArt_Phase78B_Meditation.png", new Vector2(730, -278), Hex("#23325F"));
    }

    private static void AddPreviewCard(Transform root, string title, string cost, string type, string desc, string art, Vector2 center, Color tagColor)
    {
        var card = AddImage("Right_Card_" + title, root, SpriteAt("Card/Card_Cultivation_Common.png"), RectSpec.Center(center, new Vector2(342, 242)));
        card.type = Image.Type.Sliced;
        card.color = new Color(0.92f, 0.82f, 0.60f, 1f);
        AddImage("Right_CardDescPaper_" + title, root, PhaseSprite("CardDescPaper_Phase78B.png"), RectSpec.Center(center + new Vector2(0, -72), new Vector2(300, 78))).type = Image.Type.Sliced;
        AddImage("Right_CardArt_" + title, root, PhaseSprite(art), RectSpec.Center(center + new Vector2(0, 34), new Vector2(292, 108)));
        AddImage("Right_CardArtFrame_" + title, root, SpriteAt("Divider/Divider_InkGold.png"), RectSpec.Center(center + new Vector2(0, -22), new Vector2(300, 4))).color = new Color(0.36f, 0.25f, 0.14f, 0.45f);
        AddImage("Right_CostOrb_" + title, root, SpriteAt("Card/CostOrb_Gold.png"), RectSpec.Center(center + new Vector2(-144, 86), new Vector2(56, 56)));
        AddText("Right_Cost_" + title, root, cost, 28, Hex("#11160F"), TextAnchor.MiddleCenter, RectSpec.Center(center + new Vector2(-144, 86), new Vector2(50, 50)), FontStyle.Bold);
        AddText("Right_CardTitle_" + title, root, title, 28, Hex("#20170D"), TextAnchor.MiddleLeft, RectSpec.Center(center + new Vector2(20, 86), new Vector2(214, 42)), FontStyle.Bold);
        var typeTag = AddImage("Right_TypeTag_" + title, root, SpriteAt("Panel/Panel_Bamboo_Tag.png"), RectSpec.Center(center + new Vector2(124, 84), new Vector2(74, 34)));
        typeTag.type = Image.Type.Sliced;
        typeTag.color = tagColor;
        AddText("Right_Type_" + title, root, type, 17, Hex("#EDE2C5"), TextAnchor.MiddleCenter, RectSpec.Center(center + new Vector2(124, 84), new Vector2(70, 30)), FontStyle.Bold);
        AddText("Right_Desc_" + title, root, desc, 18, Hex("#1F160D"), TextAnchor.UpperLeft, RectSpec.Center(center + new Vector2(0, -72), new Vector2(282, 66)), FontStyle.Bold);
        AddText("Right_CardMark_" + title, root, "◇", 24, Hex("#2B2112"), TextAnchor.MiddleRight, RectSpec.Center(center + new Vector2(136, -94), new Vector2(32, 32)), FontStyle.Bold);
    }

    private static void AddBottomActions(Transform root)
    {
        AddBottomButton(root, "背包", "◎", new Vector2(-825, -486));
        AddBottomButton(root, "功法", "▰", new Vector2(-600, -486));
        AddBottomButton(root, "图鉴", "▱", new Vector2(-375, -486));
        AddBottomButton(root, "状态", "♨", new Vector2(-150, -486));

        var main = AddImage("Bottom_PrimaryAdvanceButton", root, SpriteAt("Button/Button_Primary_Scroll_Normal.png"), RectSpec.Center(new Vector2(430, -486), new Vector2(520, 84)));
        main.type = Image.Type.Sliced;
        main.color = new Color(0.88f, 0.92f, 0.72f, 1f);
        main.gameObject.AddComponent<Button>().targetGraphic = main;
        AddImage("Bottom_PrimaryGlow", root, SpriteAt("RouteNode/Node_Current_Glow.png"), RectSpec.Center(new Vector2(438, -486), new Vector2(600, 120))).color = new Color(0.18f, 0.86f, 0.57f, 0.26f);
        AddText("Bottom_PrimaryLabel", root, "进入秘境", 42, Hex("#F2E6B8"), TextAnchor.MiddleCenter, RectSpec.Center(new Vector2(360, -486), new Vector2(310, 62)), FontStyle.Bold);
        AddText("Bottom_PrimaryIcon", root, "☯", 42, Hex("#5EE0AD"), TextAnchor.MiddleCenter, RectSpec.Center(new Vector2(610, -486), new Vector2(72, 62)), FontStyle.Bold);
    }

    private static void AddBottomButton(Transform root, string label, string icon, Vector2 center)
    {
        var bg = AddImage("Bottom_Button_" + label, root, SpriteAt("Button/Button_Secondary_Jade_Normal.png"), RectSpec.Center(center, new Vector2(190, 60)));
        bg.type = Image.Type.Sliced;
        bg.color = new Color(0.58f, 0.66f, 0.56f, 0.82f);
        bg.gameObject.AddComponent<Button>().targetGraphic = bg;
        AddText("Bottom_ButtonIcon_" + label, root, icon, 28, Hex("#CDBB85"), TextAnchor.MiddleCenter, RectSpec.Center(center + new Vector2(-48, 0), new Vector2(38, 38)), FontStyle.Bold);
        AddText("Bottom_ButtonLabel_" + label, root, label, 27, Hex("#DCCB9C"), TextAnchor.MiddleCenter, RectSpec.Center(center + new Vector2(24, 0), new Vector2(92, 40)), FontStyle.Bold);
    }

    private static void GeneratePhaseTextures(string outputDir)
    {
        CreateTableMist(outputDir + "/BG_Phase78B_TableMist.png");
        CreateParchmentMap(outputDir + "/Map_Phase78B_ParchmentScroll.png");
        CreateScrollRoller(outputDir + "/ScrollRoller_Phase78B.png");
        CreateSpiritLine(outputDir + "/SpiritLine_Phase78B.png");
        CreatePortrait(outputDir + "/Portrait_Phase78B_Cultivator.png");
        CreateCardArt(outputDir + "/CardArt_Phase78B_Sword.png", 0);
        CreateCardArt(outputDir + "/CardArt_Phase78B_Step.png", 1);
        CreateCardArt(outputDir + "/CardArt_Phase78B_Meditation.png", 2);
        CreateCardDescPaper(outputDir + "/CardDescPaper_Phase78B.png");
        CreateIsland(outputDir + "/MapCenter_Island_Phase78B.png");
    }

    private static void CreateTableMist(string assetPath)
    {
        var texture = new Texture2D(1920, 1080, TextureFormat.RGBA32, false);
        for (var y = 0; y < texture.height; y++)
        {
            for (var x = 0; x < texture.width; x++)
            {
                var t = y / (float)(texture.height - 1);
                var color = Color.Lerp(Hex("#050807"), Hex("#20312E"), t);
                var noise = Hash01(x / 3, y / 3) * 0.055f;
                color = Color.Lerp(color, Hex("#73847A"), noise);
                var mountain = 260 + Mathf.Sin(x * 0.006f) * 65f + Mathf.Sin(x * 0.017f) * 28f;
                if (y < mountain)
                {
                    color = Color.Lerp(color, Hex("#07100F"), 0.72f);
                }
                var mist = Mathf.Exp(-Mathf.Pow((y - 570f + Mathf.Sin(x * 0.009f) * 70f) / 135f, 2f)) * 0.16f;
                color = Color.Lerp(color, Hex("#7E8E82"), mist);
                texture.SetPixel(x, y, new Color(color.r, color.g, color.b, 1f));
            }
        }
        SaveTexture(texture, assetPath, Vector4.zero);
    }

    private static void CreateParchmentMap(string assetPath)
    {
        var texture = new Texture2D(960, 760, TextureFormat.RGBA32, false);
        for (var y = 0; y < texture.height; y++)
        {
            for (var x = 0; x < texture.width; x++)
            {
                var edge = Mathf.Min(Mathf.Min(x, texture.width - 1 - x), Mathf.Min(y, texture.height - 1 - y));
                var color = Color.Lerp(Hex("#B9AA87"), Hex("#E0D0A5"), Mathf.Clamp01(edge / 90f));
                color = Color.Lerp(color, Hex("#5B3F24"), Hash01(x, y) * 0.07f);
                texture.SetPixel(x, y, new Color(color.r, color.g, color.b, 1f));
            }
        }

        for (var i = 0; i < 20; i++)
        {
            var baseX = 54 + i * 46;
            var baseY = 150 + Mathf.Sin(i * 0.65f) * 95;
            DrawMountain(texture, baseX, baseY, 118 + (i % 4) * 26, 110 + (i % 5) * 20, new Color(0.14f, 0.16f, 0.13f, 0.10f));
            if (i % 3 == 0)
            {
                DrawMountainWash(texture, baseX + 28, baseY + 18, 128, 92, new Color(0.10f, 0.13f, 0.11f, 0.06f));
            }
        }

        for (var i = 0; i < 12; i++)
        {
            DrawMistBand(texture, 60 + i * 82, 300 + Mathf.Sin(i * 1.2f) * 92, 170, 42, new Color(0.34f, 0.37f, 0.32f, 0.07f));
        }

        DrawRect(texture, new Rect(0, 0, 960, 16), Hex("#3B2A19"), 0.35f);
        DrawRect(texture, new Rect(0, 744, 960, 16), Hex("#3B2A19"), 0.35f);
        DrawRect(texture, new Rect(0, 0, 18, 760), Hex("#3B2A19"), 0.24f);
        DrawRect(texture, new Rect(942, 0, 18, 760), Hex("#3B2A19"), 0.24f);
        DrawArc(texture, new Vector2(760, 170), 58, 0f, Mathf.PI * 2f, Hex("#4A3B27"), 5);
        DrawLine(texture, new Vector2(718, 170), new Vector2(802, 170), Hex("#4A3B27"), 3);
        DrawLine(texture, new Vector2(760, 128), new Vector2(760, 212), Hex("#4A3B27"), 3);
        SaveTexture(texture, assetPath, new Vector4(20, 20, 20, 20));
    }

    private static void DrawMountain(Texture2D texture, float baseX, float baseY, float width, float height, Color color)
    {
        var peak = new Vector2(baseX + width * 0.5f, baseY + height);
        var left = new Vector2(baseX, baseY);
        var right = new Vector2(baseX + width, baseY);
        DrawLine(texture, left, peak, color, 5);
        DrawLine(texture, peak, right, color, 5);
        DrawLine(texture, left + new Vector2(width * 0.22f, height * 0.28f), peak + new Vector2(width * 0.06f, -height * 0.22f), color, 3);
        DrawLine(texture, peak + new Vector2(-width * 0.04f, -height * 0.24f), right + new Vector2(-width * 0.28f, height * 0.22f), color, 3);
    }

    private static void DrawMountainWash(Texture2D texture, float baseX, float baseY, float width, float height, Color color)
    {
        var center = new Vector2(baseX + width * 0.5f, baseY + height * 0.38f);
        var minX = Mathf.FloorToInt(baseX);
        var maxX = Mathf.CeilToInt(baseX + width);
        var minY = Mathf.FloorToInt(baseY);
        var maxY = Mathf.CeilToInt(baseY + height);
        for (var y = minY; y <= maxY; y++)
        {
            for (var x = minX; x <= maxX; x++)
            {
                if (!InBounds(texture, x, y))
                {
                    continue;
                }

                var p = new Vector2(x, y);
                var dx = Mathf.Abs((p.x - center.x) / (width * 0.5f));
                var dy = Mathf.Abs((p.y - center.y) / (height * 0.5f));
                var alpha = Mathf.Clamp01(1f - dx * dx - dy * dy);
                BlendPixel(texture, x, y, color, alpha * color.a);
            }
        }
    }

    private static void DrawMistBand(Texture2D texture, float centerX, float centerY, float width, float height, Color color)
    {
        var minX = Mathf.FloorToInt(centerX - width * 0.5f);
        var maxX = Mathf.CeilToInt(centerX + width * 0.5f);
        var minY = Mathf.FloorToInt(centerY - height * 0.5f);
        var maxY = Mathf.CeilToInt(centerY + height * 0.5f);
        for (var y = minY; y <= maxY; y++)
        {
            for (var x = minX; x <= maxX; x++)
            {
                if (!InBounds(texture, x, y))
                {
                    continue;
                }

                var dx = Mathf.Abs((x - centerX) / (width * 0.5f));
                var dy = Mathf.Abs((y - centerY) / (height * 0.5f));
                var alpha = Mathf.Clamp01(1f - dx * dx - dy * dy);
                BlendPixel(texture, x, y, color, alpha * color.a);
            }
        }
    }

    private static void CreateScrollRoller(string assetPath)
    {
        var texture = new Texture2D(96, 860, TextureFormat.RGBA32, false);
        for (var y = 0; y < texture.height; y++)
        {
            for (var x = 0; x < texture.width; x++)
            {
                var dx = Mathf.Abs(x - 48) / 48f;
                var shade = 1f - dx * 0.7f;
                var color = Color.Lerp(Hex("#302A20"), Hex("#84715A"), shade);
                color = Color.Lerp(color, Hex("#1A1611"), Hash01(x, y) * 0.11f);
                texture.SetPixel(x, y, new Color(color.r, color.g, color.b, 1f));
            }
        }
        DrawRect(texture, new Rect(0, 0, 96, 34), Hex("#221B13"), 0.65f);
        DrawRect(texture, new Rect(0, 826, 96, 34), Hex("#221B13"), 0.65f);
        SaveTexture(texture, assetPath, new Vector4(18, 26, 18, 26));
    }

    private static void CreateSpiritLine(string assetPath)
    {
        var texture = new Texture2D(220, 36, TextureFormat.RGBA32, false);
        for (var y = 0; y < texture.height; y++)
        {
            for (var x = 0; x < texture.width; x++)
            {
                var d = Mathf.Abs(y - texture.height * 0.5f);
                var alpha = Mathf.Exp(-d * d / 42f);
                var pulse = 0.75f + Mathf.Sin(x * 0.13f) * 0.25f;
                var color = Color.Lerp(Hex("#F8E2A0"), Hex("#FFF8D8"), Mathf.Clamp01(alpha));
                texture.SetPixel(x, y, new Color(color.r, color.g, color.b, Mathf.Clamp01(alpha * pulse)));
            }
        }
        SaveTexture(texture, assetPath, new Vector4(18, 0, 18, 0));
    }

    private static void CreatePortrait(string assetPath)
    {
        var texture = NewTransparent(420, 520);
        FillGradient(texture, Hex("#C8C1AD"), Hex("#6F7165"), 1f);
        DrawMist(texture, 0.10f);
        DrawCircle(texture, new Vector2(214, 328), 50, Hex("#D6C4A5"), 0.96f);
        DrawLine(texture, new Vector2(166, 370), new Vector2(258, 370), Hex("#11100E"), 23);
        DrawLine(texture, new Vector2(172, 342), new Vector2(122, 504), Hex("#11100E"), 14);
        DrawLine(texture, new Vector2(248, 342), new Vector2(310, 504), Hex("#11100E"), 14);
        DrawLine(texture, new Vector2(168, 354), new Vector2(86, 440), Hex("#11100E"), 9);
        DrawLine(texture, new Vector2(248, 354), new Vector2(334, 438), Hex("#11100E"), 9);
        DrawCircle(texture, new Vector2(212, 194), 122, Hex("#34464B"), 0.86f);
        DrawLine(texture, new Vector2(126, 238), new Vector2(292, 92), Hex("#D7D8CF"), 24);
        DrawLine(texture, new Vector2(156, 238), new Vector2(316, 100), Hex("#2C6470"), 10);
        DrawLine(texture, new Vector2(96, 294), new Vector2(334, 104), Hex("#0F1110"), 11);
        DrawLine(texture, new Vector2(210, 374), new Vector2(210, 506), Hex("#11100E"), 8);
        DrawCircle(texture, new Vector2(198, 334), 4, Hex("#1C1712"), 0.9f);
        DrawCircle(texture, new Vector2(230, 334), 4, Hex("#1C1712"), 0.9f);
        DrawLine(texture, new Vector2(200, 306), new Vector2(232, 306), Hex("#8D5848"), 3);
        SaveTexture(texture, assetPath, new Vector4(18, 18, 18, 18));
    }

    private static void CreateCardDescPaper(string assetPath)
    {
        var texture = new Texture2D(320, 96, TextureFormat.RGBA32, false);
        for (var y = 0; y < texture.height; y++)
        {
            for (var x = 0; x < texture.width; x++)
            {
                var edge = Mathf.Min(Mathf.Min(x, texture.width - 1 - x), Mathf.Min(y, texture.height - 1 - y));
                var color = Color.Lerp(Hex("#A98E5B"), Hex("#D9C8A0"), Mathf.Clamp01(edge / 18f));
                color = Color.Lerp(color, Hex("#6E5430"), Hash01(x, y) * 0.035f);
                texture.SetPixel(x, y, new Color(color.r, color.g, color.b, 0.98f));
            }
        }
        DrawRect(texture, new Rect(0, 0, texture.width, 4), Hex("#6C4E25"), 0.35f);
        SaveTexture(texture, assetPath, new Vector4(12, 12, 12, 12));
    }

    private static void CreateCardArt(string assetPath, int kind)
    {
        var texture = new Texture2D(420, 160, TextureFormat.RGBA32, false);
        FillGradient(texture, kind == 2 ? Hex("#5F533B") : Hex("#A8B9B0"), kind == 1 ? Hex("#294845") : Hex("#394541"), 1f);
        DrawMist(texture, 0.18f);
        for (var i = 0; i < 5; i++)
        {
            DrawMountain(texture, 34 + i * 82, 28 + Mathf.Sin(i) * 16, 112, 86 + i * 8, new Color(0.12f, 0.16f, 0.14f, 0.28f));
        }

        if (kind == 0)
        {
            DrawLine(texture, new Vector2(138, 46), new Vector2(330, 116), Hex("#D9EAE7"), 9);
            DrawLine(texture, new Vector2(186, 38), new Vector2(340, 104), Hex("#7CD9D0"), 4);
        }
        else if (kind == 1)
        {
            DrawLine(texture, new Vector2(70, 76), new Vector2(310, 88), Hex("#CBEAF0"), 8);
            DrawLine(texture, new Vector2(120, 104), new Vector2(350, 116), Hex("#83C7D5"), 5);
        }
        else
        {
            DrawCircle(texture, new Vector2(215, 82), 58, Hex("#F6DE9D"), 0.32f);
            DrawArc(texture, new Vector2(215, 84), 46, 0f, Mathf.PI * 2f, Hex("#F7D77E"), 5);
        }
        SaveTexture(texture, assetPath, new Vector4(10, 10, 10, 10));
    }

    private static void CreateIsland(string assetPath)
    {
        var texture = NewTransparent(180, 180);
        DrawCircle(texture, new Vector2(90, 88), 76, Hex("#221A10"), 0.58f);
        DrawMountain(texture, 36, 58, 108, 82, Hex("#151511"));
        DrawLine(texture, new Vector2(58, 56), new Vector2(122, 56), Hex("#C6AA61"), 4);
        SaveTexture(texture, assetPath, Vector4.zero);
    }

    private static GameObject CreateRoot(string name)
    {
        var root = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
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

    private static Image AddImage(string name, Transform parent, Sprite sprite, RectSpec spec)
    {
        var rect = CreateRect(name, parent, spec);
        var image = rect.gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.color = Color.white;
        return image;
    }

    private static Text AddText(string name, Transform parent, string text, int size, Color color, TextAnchor alignment, RectSpec spec, FontStyle style = FontStyle.Normal)
    {
        var rect = CreateRect(name, parent, spec);
        var label = rect.gameObject.AddComponent<Text>();
        label.font = LoadUiFont();
        label.text = text;
        label.fontSize = size;
        label.color = color;
        label.alignment = alignment;
        label.fontStyle = style;
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Truncate;
        label.raycastTarget = false;
        return label;
    }

    private static RectTransform CreateRect(string name, Transform parent, RectSpec spec)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        spec.Apply(rect);
        return rect;
    }

    private static Sprite SpriteAt(string relativePath)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ThemeRoot + "/" + relativePath);
        if (sprite != null)
        {
            return sprite;
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(ThemeRoot + "/Placeholders/PH_Fallback.png");
    }

    private static Sprite PhaseSprite(string fileName)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(_activePhaseDir + "/" + fileName);
    }

    private static Font LoadUiFont()
    {
        if (_font != null)
        {
            return _font;
        }

        _font = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
        if (_font == null)
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        return _font;
    }

    private static void EnsureDirectories()
    {
        Directory.CreateDirectory(ToAbsolute(PhaseDir));
        Directory.CreateDirectory(ToAbsolute(FinalPhaseDir));
        Directory.CreateDirectory(ToAbsolute(TemplateDir));
        AssetDatabase.Refresh();
    }

    private static void SaveTexture(Texture2D texture, string assetPath, Vector4 border)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(ToAbsolute(assetPath)) ?? string.Empty);
        WriteAllBytesWithRetry(ToAbsolute(assetPath), texture.EncodeToPNG());
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

    private static void WriteAllBytesWithRetry(string absolutePath, byte[] bytes)
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

        File.WriteAllBytes(absolutePath, bytes);
    }

    private static string ToAbsolute(string assetPath)
    {
        return Path.Combine(Directory.GetParent(Application.dataPath).FullName, assetPath.Replace("/", Path.DirectorySeparatorChar.ToString()));
    }

    private static string GetRepositoryRoot()
    {
        return Directory.GetParent(Directory.GetParent(Application.dataPath).FullName).FullName;
    }

    private static Texture2D NewTransparent(int width, int height)
    {
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                texture.SetPixel(x, y, Color.clear);
            }
        }
        return texture;
    }

    private static void FillGradient(Texture2D texture, Color top, Color bottom, float alpha)
    {
        for (var y = 0; y < texture.height; y++)
        {
            for (var x = 0; x < texture.width; x++)
            {
                var t = y / (float)(texture.height - 1);
                var color = Color.Lerp(bottom, top, t);
                color = Color.Lerp(color, Color.white, Hash01(x, y) * 0.04f);
                texture.SetPixel(x, y, new Color(color.r, color.g, color.b, alpha));
            }
        }
    }

    private static void DrawMist(Texture2D texture, float alpha)
    {
        for (var y = 0; y < texture.height; y++)
        {
            for (var x = 0; x < texture.width; x++)
            {
                var wave = Mathf.Sin(x * 0.025f + y * 0.008f) * 0.5f + 0.5f;
                var band = Mathf.Exp(-Mathf.Pow((y - texture.height * 0.55f + Mathf.Sin(x * 0.015f) * 20f) / (texture.height * 0.2f), 2f));
                BlendPixel(texture, x, y, Hex("#E3E4D2"), wave * band * alpha);
            }
        }
    }

    private static Vector2 Quadratic(Vector2 a, Vector2 b, Vector2 c, float t)
    {
        var u = 1f - t;
        return u * u * a + 2f * u * t * b + t * t * c;
    }

    private static Color Hex(string value)
    {
        ColorUtility.TryParseHtmlString(value, out var color);
        return color;
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
                if (!InBounds(texture, x, y) || segmentLength <= 0.001f)
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

                var inRange = angle >= start && angle <= end;
                if (!inRange)
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
