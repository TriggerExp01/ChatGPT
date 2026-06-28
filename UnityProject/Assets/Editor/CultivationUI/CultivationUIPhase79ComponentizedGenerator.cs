#if UNITY_EDITOR
using System.IO;
using GameLogic.Cultivation.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class CultivationUIPhase79ComponentizedGenerator
{
    private const string ThemeRoot = "Assets/AssetRaw/UIRaw/Theme/Cultivation";
    private const string ComponentDir = ThemeRoot + "/Components/Steam";
    private const string TemplateDir = ThemeRoot + "/Templates/SteamDemo";
    private const string PhaseTextureDir = ThemeRoot + "/Phase78B";
    private const string FontPath = "Assets/AssetRaw/Fonts/NotoSansCJKsc-VF.ttf";
    private const string PrefabPath = TemplateDir + "/Steam_MainRoute_Componentized.prefab";
    private const string ScreenshotName = "Steam_MainRoute_Componentized.png";

    private static Font _font;

    [MenuItem("Codex/Cultivation UI/Phase79/Generate Componentized MainRoute And Screenshot")]
    public static void GenerateAll()
    {
        GenerateComponentPrefabs();
        GenerateMainRoutePrefab();
        CaptureScreenshot();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Phase79 componentized MainRoute generated.");
    }

    [MenuItem("Codex/Cultivation UI/Phase79/Generate Steam Components")]
    public static void GenerateComponentPrefabs()
    {
        EnsureDirectories();
        SaveComponent("UI_CardItem", BuildCardItem);
        SaveComponent("UI_RouteNode", BuildRouteNode);
        SaveComponent("UI_TopResourceItem", BuildTopResourceItem);
        SaveComponent("UI_CharacterStatusPanel", BuildCharacterStatusPanel);
        SaveComponent("UI_PrimaryButton", BuildPrimaryButton);
        SaveComponent("UI_SecondaryButton", BuildSecondaryButton);
        SaveComponent("UI_StatusTag", BuildStatusTag);
        SaveComponent("UI_RewardItem", BuildRewardItem);
        SaveComponent("UI_PopupWindow", BuildPopupWindow);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [MenuItem("Codex/Cultivation UI/Phase79/Generate Componentized MainRoute Prefab")]
    public static void GenerateMainRoutePrefab()
    {
        EnsureDirectories();
        GenerateComponentPrefabs();

        var root = CreateRoot("Steam_MainRoute_Componentized");
        root.AddComponent<SteamMainRouteDemoBinder>();
        BuildMainRoute(root.transform);
        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [MenuItem("Codex/Cultivation UI/Phase79/Capture Componentized MainRoute Screenshot")]
    public static string CaptureScreenshot()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null)
        {
            GenerateMainRoutePrefab();
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        }

        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.hideFlags = HideFlags.HideAndDontSave;
        instance.name = "Steam_MainRoute_Componentized_CaptureInstance";

        var binder = instance.GetComponent<SteamMainRouteDemoBinder>();
        if (binder != null)
        {
            binder.Bind(SteamMainRouteDemoData.Create());
        }

        var cameraObject = new GameObject("Steam_MainRoute_Componentized_CaptureCamera");
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
            text.font = LoadFont();
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
        var screenshotPath = Path.Combine(screenshotDir, ScreenshotName);
        File.WriteAllBytes(screenshotPath, texture.EncodeToPNG());

        camera.targetTexture = previousTarget;
        RenderTexture.active = previousActive;
        Object.DestroyImmediate(texture);
        Object.DestroyImmediate(rt);
        Object.DestroyImmediate(instance);
        Object.DestroyImmediate(cameraObject);

        Debug.Log("Captured Phase79 componentized MainRoute screenshot: " + screenshotPath);
        return screenshotPath;
    }

    private static void BuildMainRoute(Transform root)
    {
        AddImage("Background_TableMist", root, PhaseSprite("BG_Phase78B_TableMist.png"), RectSpec.Stretch(Vector2.zero, Vector2.zero)).raycastTarget = false;
        AddImage("Background_Vignette", root, SpriteAt("Background/BG_Cultivation_Vignette.png"), RectSpec.Stretch(Vector2.zero, Vector2.zero)).color = new Color(1f, 1f, 1f, 0.88f);
        AddTopBar(root);
        AddCharacterArchive(root);
        AddRouteMap(root);
        AddCardPanel(root);
        AddBottomActions(root);
    }

    private static void AddTopBar(Transform root)
    {
        var bar = AddImage("Top_ThinResourceBar", root, SpriteAt("Panel/Panel_Bottom_Transparent.png"), RectSpec.Top(new Vector2(0, -36), new Vector2(1920, 72)));
        bar.type = Image.Type.Sliced;
        bar.color = new Color(0.02f, 0.025f, 0.024f, 0.92f);
        AddImage("Top_BottomGoldLine", root, SpriteAt("Divider/Divider_InkGold.png"), RectSpec.Top(new Vector2(0, -72), new Vector2(1920, 5))).color = new Color(0.82f, 0.62f, 0.34f, 0.42f);
        AddText("Top_Brand", root, string.Empty, 36, Hex("#D9BD84"), TextAnchor.MiddleLeft, RectSpec.Top(new Vector2(-812, -35), new Vector2(260, 54)), FontStyle.Bold);
        for (var i = 0; i < 3; i++)
        {
            var item = InstantiateComponent("UI_TopResourceItem", root, "TopResource_" + i, new Vector2(-430 + i * 320, 504), new Vector2(240, 54));
            RenameChild(item.transform, "IconText", "TopResource_" + i + "_Icon");
            RenameChild(item.transform, "ValueText", "TopResource_" + i + "_Text");
        }
        AddText("Top_Location", root, string.Empty, 28, Hex("#D9C69B"), TextAnchor.MiddleRight, RectSpec.Top(new Vector2(595, -35), new Vector2(560, 50)), FontStyle.Bold);
    }

    private static void AddCharacterArchive(Transform root)
    {
        var panel = InstantiateComponent("UI_CharacterStatusPanel", root, "Left_CharacterStatusPanel", new Vector2(-728, 18), new Vector2(390, 842));
        RenameChild(panel.transform, "NameText", "Left_Name");
        RenameChild(panel.transform, "RealmValueText", "Left_RealmValue");
        for (var i = 0; i < 6; i++)
        {
            RenameChild(panel.transform, "Stat_" + i + "_Icon", "Left_Stat_" + i + "_Icon");
            RenameChild(panel.transform, "Stat_" + i + "_Label", "Left_Stat_" + i + "_Label");
            RenameChild(panel.transform, "Stat_" + i + "_Value", "Left_Stat_" + i + "_Value");
        }
        for (var i = 0; i < 3; i++)
        {
            RenameChild(panel.transform, "StatusTag_" + i + "_Title", "Left_StatusTag_" + i + "_Title");
            RenameChild(panel.transform, "StatusTag_" + i + "_Duration", "Left_StatusTag_" + i + "_Duration");
        }
    }

    private static void AddRouteMap(Transform root)
    {
        AddImage("Center_MapShadow", root, SpriteAt("Panel/Panel_Bottom_Transparent.png"), RectSpec.Center(new Vector2(8, -16), new Vector2(1060, 800))).color = new Color(0f, 0f, 0f, 0.36f);
        AddImage("Center_LeftScrollRoller", root, PhaseSprite("ScrollRoller_Phase78B.png"), RectSpec.Center(new Vector2(-496, 18), new Vector2(74, 820)));
        AddImage("Center_RightScrollRoller", root, PhaseSprite("ScrollRoller_Phase78B.png"), RectSpec.Center(new Vector2(496, 18), new Vector2(74, 820)));
        AddImage("Center_ParchmentRouteMap", root, PhaseSprite("Map_Phase78B_ParchmentScroll.png"), RectSpec.Center(new Vector2(0, 18), new Vector2(960, 760)));
        AddText("Center_MapVerticalTitle", root, "云\n雾\n秘\n境", 42, Hex("#171209"), TextAnchor.MiddleCenter, RectSpec.Center(new Vector2(-390, 166), new Vector2(86, 260)), FontStyle.Bold);

        var ids = new[] { "event", "battle", "chest", "rest", "market", "current" };
        var positions = new[]
        {
            new Vector2(-250, 112),
            new Vector2(26, 214),
            new Vector2(302, 92),
            new Vector2(-248, -226),
            new Vector2(248, -248),
            new Vector2(50, -4)
        };
        var icons = new[]
        {
            "RouteNode/Node_Event_Scroll.png",
            "RouteNode/Node_Battle_Sword.png",
            "RouteNode/Node_Chest_Box.png",
            "RouteNode/Node_Rest_Meditation.png",
            "RouteNode/Node_Market_Pavilion.png",
            "RouteNode/Node_Rest_Meditation.png"
        };
        var current = positions[5];
        for (var i = 0; i < positions.Length - 1; i++)
        {
            AddSpiritLine(root, current, positions[i], "RoutePath_" + ids[i]);
        }
        for (var i = 0; i < positions.Length; i++)
        {
            var node = InstantiateComponent("UI_RouteNode", root, "RouteNode_" + ids[i], positions[i], new Vector2(132, 132));
            SetChildSprite(node.transform, "Icon", icons[i]);
            RenameChild(node.transform, "LabelText", "RouteNode_" + ids[i] + "_Label");
            if (ids[i] == "current")
            {
                FindChild(node.transform, "CurrentGlow")?.gameObject.SetActive(true);
                FindChild(node.transform, "LabelRoot")?.gameObject.SetActive(false);
            }
        }
    }

    private static void AddCardPanel(Transform root)
    {
        var panel = AddImage("Right_CurrentHandPanel", root, SpriteAt("Panel/Panel_Main_JadeThin.png"), RectSpec.Center(new Vector2(730, 18), new Vector2(395, 842)));
        panel.type = Image.Type.Sliced;
        panel.color = new Color(0.74f, 0.78f, 0.68f, 0.96f);
        AddText("Right_Title", root, "当前手牌  (3/3)", 25, Hex("#DFC789"), TextAnchor.MiddleLeft, RectSpec.Top(new Vector2(730, -118), new Vector2(310, 42)), FontStyle.Bold);
        var centers = new[] { new Vector2(730, 242), new Vector2(730, -18), new Vector2(730, -278) };
        for (var i = 0; i < centers.Length; i++)
        {
            var card = InstantiateComponent("UI_CardItem", root, "Right_Card_" + i, centers[i], new Vector2(342, 242));
            RenameChild(card.transform, "CostText", "Right_Card_" + i + "_Cost");
            RenameChild(card.transform, "TitleText", "Right_Card_" + i + "_Title");
            RenameChild(card.transform, "TypeText", "Right_Card_" + i + "_Type");
            RenameChild(card.transform, "DescText", "Right_Card_" + i + "_Desc");
            RenameChild(card.transform, "CountText", "Right_Card_" + i + "_Count");
        }
    }

    private static void AddBottomActions(Transform root)
    {
        var labels = new[] { "背包", "功法", "图鉴", "状态" };
        for (var i = 0; i < labels.Length; i++)
        {
            var button = InstantiateComponent("UI_SecondaryButton", root, "Bottom_Button_" + labels[i], new Vector2(-825 + i * 225, -486), new Vector2(190, 60));
            SetChildText(button.transform, "LabelText", labels[i]);
        }
        var primary = InstantiateComponent("UI_PrimaryButton", root, "Bottom_PrimaryAdvanceButton", new Vector2(430, -486), new Vector2(520, 84));
        SetChildText(primary.transform, "LabelText", "进入秘境");
    }

    private static void BuildCardItem(Transform root)
    {
        SetRect(root, RectSpec.Center(Vector2.zero, new Vector2(342, 242)));
        var card = AddImage("Frame", root, SpriteAt("Card/Card_Cultivation_Common.png"), RectSpec.Stretch(Vector2.zero, Vector2.zero));
        card.type = Image.Type.Sliced;
        card.color = new Color(0.92f, 0.82f, 0.60f, 1f);
        AddImage("Art", root, PhaseSprite("CardArt_Phase78B_Sword.png"), RectSpec.Center(new Vector2(0, 34), new Vector2(292, 108)));
        AddImage("DescPaper", root, SpriteAt("Panel/Panel_Secondary_Parchment.png"), RectSpec.Center(new Vector2(0, -72), new Vector2(300, 78))).type = Image.Type.Sliced;
        AddImage("CostOrb", root, SpriteAt("Card/CostOrb_Gold.png"), RectSpec.Center(new Vector2(-144, 86), new Vector2(56, 56)));
        AddText("CostText", root, string.Empty, 28, Hex("#11160F"), TextAnchor.MiddleCenter, RectSpec.Center(new Vector2(-144, 86), new Vector2(50, 50)), FontStyle.Bold);
        AddText("TitleText", root, string.Empty, 28, Hex("#20170D"), TextAnchor.MiddleLeft, RectSpec.Center(new Vector2(20, 86), new Vector2(214, 42)), FontStyle.Bold);
        AddText("TypeText", root, string.Empty, 17, Hex("#EDE2C5"), TextAnchor.MiddleCenter, RectSpec.Center(new Vector2(124, 84), new Vector2(70, 30)), FontStyle.Bold);
        AddText("DescText", root, string.Empty, 18, Hex("#1F160D"), TextAnchor.UpperLeft, RectSpec.Center(new Vector2(0, -72), new Vector2(282, 66)), FontStyle.Bold);
        AddText("CountText", root, string.Empty, 16, Hex("#1F160D"), TextAnchor.MiddleRight, RectSpec.Center(new Vector2(120, -96), new Vector2(60, 24)), FontStyle.Bold);
    }

    private static void BuildRouteNode(Transform root)
    {
        SetRect(root, RectSpec.Center(Vector2.zero, new Vector2(132, 132)));
        AddImage("CurrentGlow", root, SpriteAt("RouteNode/Node_Current_Glow.png"), RectSpec.Center(Vector2.zero, new Vector2(138, 138))).color = new Color(1f, 0.78f, 0.35f, 0.32f);
        AddImage("Icon", root, SpriteAt("RouteNode/Node_Event_Scroll.png"), RectSpec.Center(Vector2.zero, new Vector2(104, 104)));
        var labelRoot = CreateRect("LabelRoot", root, RectSpec.Center(new Vector2(0, -63), new Vector2(114, 34)));
        var labelBg = labelRoot.gameObject.AddComponent<Image>();
        labelBg.sprite = SpriteAt("Panel/Panel_Bottom_Transparent.png");
        labelBg.type = Image.Type.Sliced;
        labelBg.color = new Color(0.03f, 0.025f, 0.018f, 0.78f);
        AddText("LabelText", labelRoot, string.Empty, 27, Hex("#F2DFA8"), TextAnchor.MiddleCenter, RectSpec.Stretch(Vector2.zero, Vector2.zero), FontStyle.Bold);
    }

    private static void BuildTopResourceItem(Transform root)
    {
        SetRect(root, RectSpec.Center(Vector2.zero, new Vector2(240, 54)));
        AddText("IconText", root, string.Empty, 24, Hex("#D9BD84"), TextAnchor.MiddleCenter, RectSpec.Center(new Vector2(-96, 0), new Vector2(34, 44)), FontStyle.Bold);
        AddText("ValueText", root, string.Empty, 25, Hex("#D9C69B"), TextAnchor.MiddleLeft, RectSpec.Center(new Vector2(20, 0), new Vector2(190, 46)), FontStyle.Bold);
        AddImage("BarBack", root, SpriteAt("Panel/Panel_Bottom_Transparent.png"), RectSpec.Center(new Vector2(66, -20), new Vector2(175, 8))).color = new Color(0f, 0f, 0f, 0.55f);
        AddImage("BarFill", root, SpriteAt("Divider/Divider_InkGold.png"), RectSpec.Center(new Vector2(42, -20), new Vector2(128, 5))).color = Hex("#C95A4F");
    }

    private static void BuildCharacterStatusPanel(Transform root)
    {
        SetRect(root, RectSpec.Center(Vector2.zero, new Vector2(390, 842)));
        var panel = AddImage("Panel", root, SpriteAt("Panel/Panel_Main_JadeThin.png"), RectSpec.Stretch(Vector2.zero, Vector2.zero));
        panel.type = Image.Type.Sliced;
        panel.color = new Color(0.72f, 0.78f, 0.68f, 0.95f);
        AddImage("PortraitPaper", root, SpriteAt("Panel/Panel_Secondary_Parchment.png"), RectSpec.Top(new Vector2(0, -104), new Vector2(338, 372))).type = Image.Type.Sliced;
        AddImage("Portrait", root, PhaseSprite("Portrait_Phase78B_Cultivator.png"), RectSpec.Top(new Vector2(0, -118), new Vector2(306, 344)));
        AddText("ClassStamp", root, "修\n士", 44, Hex("#19140C"), TextAnchor.MiddleCenter, RectSpec.Top(new Vector2(-144, -162), new Vector2(76, 124)), FontStyle.Bold);
        AddText("NameText", root, string.Empty, 30, Hex("#F0DEB8"), TextAnchor.MiddleCenter, RectSpec.Top(new Vector2(0, -382), new Vector2(220, 42)), FontStyle.Bold);
        AddText("RealmTitleText", root, "境界", 18, Hex("#BBA26D"), TextAnchor.MiddleLeft, RectSpec.Top(new Vector2(-137, -432), new Vector2(90, 28)));
        AddText("RealmValueText", root, string.Empty, 22, Hex("#E8D8B2"), TextAnchor.MiddleRight, RectSpec.Top(new Vector2(83, -432), new Vector2(150, 30)), FontStyle.Bold);
        for (var i = 0; i < 6; i++)
        {
            var y = -476 - i * 44;
            AddText("Stat_" + i + "_Icon", root, string.Empty, 20, Hex("#BCA276"), TextAnchor.MiddleCenter, RectSpec.Top(new Vector2(-152, y), new Vector2(28, 28)), FontStyle.Bold);
            AddText("Stat_" + i + "_Label", root, string.Empty, 20, Hex("#BBAE8A"), TextAnchor.MiddleLeft, RectSpec.Top(new Vector2(-107, y), new Vector2(90, 30)));
            AddText("Stat_" + i + "_Value", root, string.Empty, 22, Hex("#E8D8B2"), TextAnchor.MiddleRight, RectSpec.Top(new Vector2(74, y), new Vector2(135, 30)), FontStyle.Bold);
        }
        AddText("StatusTitleText", root, "当前状态", 20, Hex("#D2B76F"), TextAnchor.MiddleLeft, RectSpec.Top(new Vector2(-87, -748), new Vector2(180, 28)), FontStyle.Bold);
        for (var i = 0; i < 3; i++)
        {
            var tag = InstantiateComponent("UI_StatusTag", root, "StatusTag_" + i, new Vector2(-97 + i * 97, -328), new Vector2(68, 128));
            RenameChild(tag.transform, "TitleText", "StatusTag_" + i + "_Title");
            RenameChild(tag.transform, "DurationText", "StatusTag_" + i + "_Duration");
        }
    }

    private static void BuildPrimaryButton(Transform root)
    {
        SetRect(root, RectSpec.Center(Vector2.zero, new Vector2(520, 84)));
        var bg = AddImage("Bg", root, SpriteAt("Button/Button_Primary_Scroll_Normal.png"), RectSpec.Stretch(Vector2.zero, Vector2.zero));
        bg.type = Image.Type.Sliced;
        bg.color = new Color(0.88f, 0.92f, 0.72f, 1f);
        root.gameObject.AddComponent<Button>().targetGraphic = bg;
        AddText("LabelText", root, string.Empty, 42, Hex("#F2E6B8"), TextAnchor.MiddleCenter, RectSpec.Stretch(Vector2.zero, Vector2.zero), FontStyle.Bold);
    }

    private static void BuildSecondaryButton(Transform root)
    {
        SetRect(root, RectSpec.Center(Vector2.zero, new Vector2(190, 60)));
        var bg = AddImage("Bg", root, SpriteAt("Button/Button_Secondary_Jade_Normal.png"), RectSpec.Stretch(Vector2.zero, Vector2.zero));
        bg.type = Image.Type.Sliced;
        bg.color = new Color(0.58f, 0.66f, 0.56f, 0.82f);
        root.gameObject.AddComponent<Button>().targetGraphic = bg;
        AddText("LabelText", root, string.Empty, 27, Hex("#DCCB9C"), TextAnchor.MiddleCenter, RectSpec.Stretch(Vector2.zero, Vector2.zero), FontStyle.Bold);
    }

    private static void BuildStatusTag(Transform root)
    {
        SetRect(root, RectSpec.Center(Vector2.zero, new Vector2(68, 128)));
        var bg = AddImage("Bg", root, SpriteAt("Panel/Panel_Bamboo_Tag.png"), RectSpec.Stretch(Vector2.zero, Vector2.zero));
        bg.type = Image.Type.Sliced;
        bg.color = Hex("#2B663A");
        AddText("TitleText", root, string.Empty, 29, Hex("#E9D7AD"), TextAnchor.MiddleCenter, RectSpec.Top(new Vector2(0, -8), new Vector2(60, 78)), FontStyle.Bold);
        AddText("DurationText", root, string.Empty, 15, Hex("#D9C99B"), TextAnchor.MiddleCenter, RectSpec.Top(new Vector2(0, -94), new Vector2(66, 42)));
    }

    private static void BuildRewardItem(Transform root)
    {
        SetRect(root, RectSpec.Center(Vector2.zero, new Vector2(238, 338)));
        AddImage("Bg", root, SpriteAt("Panel/Panel_Secondary_Parchment.png"), RectSpec.Stretch(Vector2.zero, Vector2.zero)).type = Image.Type.Sliced;
        AddText("TitleText", root, string.Empty, 28, Hex("#F1D58D"), TextAnchor.MiddleCenter, RectSpec.Top(new Vector2(0, -34), new Vector2(172, 40)), FontStyle.Bold);
        AddText("DescText", root, string.Empty, 21, Hex("#E6DEC3"), TextAnchor.MiddleCenter, RectSpec.Center(new Vector2(0, -90), new Vector2(180, 90)));
        AddText("RarityText", root, string.Empty, 16, Hex("#8FD8BE"), TextAnchor.MiddleCenter, RectSpec.Bottom(new Vector2(0, 30), new Vector2(110, 28)));
    }

    private static void BuildPopupWindow(Transform root)
    {
        SetRect(root, RectSpec.Center(Vector2.zero, new Vector2(680, 820)));
        AddImage("Bg", root, SpriteAt("Popup/Popup_DarkJade_GoldBorder.png"), RectSpec.Stretch(Vector2.zero, Vector2.zero)).type = Image.Type.Sliced;
        AddText("TitleText", root, string.Empty, 36, Hex("#F2D58A"), TextAnchor.MiddleCenter, RectSpec.Top(new Vector2(0, -54), new Vector2(520, 52)), FontStyle.Bold);
        AddText("BodyText", root, string.Empty, 22, Hex("#EAE0C4"), TextAnchor.UpperLeft, RectSpec.Top(new Vector2(0, -366), new Vector2(520, 120)));
    }

    private static void AddSpiritLine(Transform root, Vector2 from, Vector2 to, string name)
    {
        var center = (from + to) * 0.5f;
        var size = new Vector2(Vector2.Distance(from, to), 16);
        var line = AddImage(name, root, PhaseSprite("SpiritLine_Phase78B.png"), RectSpec.Center(center, size));
        line.color = new Color(1f, 0.92f, 0.58f, 0.55f);
        line.raycastTarget = false;
        line.rectTransform.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(to.y - from.y, to.x - from.x) * Mathf.Rad2Deg);
    }

    private static void SaveComponent(string name, System.Action<Transform> build)
    {
        var root = new GameObject(name, typeof(RectTransform));
        build(root.transform);
        PrefabUtility.SaveAsPrefabAsset(root, ComponentDir + "/" + name + ".prefab");
        Object.DestroyImmediate(root);
    }

    private static GameObject InstantiateComponent(string name, Transform parent, string instanceName, Vector2 position, Vector2 size)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ComponentDir + "/" + name + ".prefab");
        var instance = prefab != null ? (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent) : new GameObject(instanceName, typeof(RectTransform));
        instance.name = instanceName;
        instance.transform.SetParent(parent, false);
        SetRect(instance.transform, RectSpec.Center(position, size));
        return instance;
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
        root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
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
        label.font = LoadFont();
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

    private static void SetRect(Transform transform, RectSpec spec)
    {
        var rect = transform.GetComponent<RectTransform>() ?? transform.gameObject.AddComponent<RectTransform>();
        spec.Apply(rect);
    }

    private static void RenameChild(Transform root, string currentName, string newName)
    {
        var child = FindChild(root, currentName);
        if (child != null)
        {
            child.name = newName;
        }
    }

    private static void SetChildText(Transform root, string childName, string value)
    {
        var child = FindChild(root, childName);
        var text = child != null ? child.GetComponent<Text>() : null;
        if (text != null)
        {
            text.text = value;
        }
    }

    private static void SetChildSprite(Transform root, string childName, string spritePath)
    {
        var child = FindChild(root, childName);
        var image = child != null ? child.GetComponent<Image>() : null;
        if (image != null)
        {
            image.sprite = SpriteAt(spritePath);
        }
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

    private static Sprite SpriteAt(string relativePath)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(ThemeRoot + "/" + relativePath) ??
               AssetDatabase.LoadAssetAtPath<Sprite>(ThemeRoot + "/Placeholders/PH_Fallback.png");
    }

    private static Sprite PhaseSprite(string fileName)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(PhaseTextureDir + "/" + fileName) ??
               AssetDatabase.LoadAssetAtPath<Sprite>(ThemeRoot + "/Placeholders/PH_Fallback.png");
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

    private static void EnsureDirectories()
    {
        Directory.CreateDirectory(ToAbsolute(ComponentDir));
        Directory.CreateDirectory(ToAbsolute(TemplateDir));
        AssetDatabase.Refresh();
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
