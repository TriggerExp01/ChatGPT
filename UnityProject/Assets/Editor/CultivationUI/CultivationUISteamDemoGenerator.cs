#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class CultivationUISteamDemoGenerator
{
    private const string ThemeRoot = "Assets/AssetRaw/UIRaw/Theme/Cultivation";
    private const string TemplateDir = ThemeRoot + "/Templates/SteamDemo";
    private const string ComponentDir = ThemeRoot + "/Components";
    private const string PlaceholderDir = ThemeRoot + "/Placeholders";
    private const string FontPath = "Assets/AssetRaw/Fonts/NotoSansCJKsc-VF.ttf";

    private static readonly string[] TemplateNames =
    {
        "Steam_MainRoute",
        "Steam_EventPopup",
        "Steam_Battle",
        "Steam_Reward",
        "Steam_CardLibrary"
    };

    private static readonly Dictionary<string, string> ScreenshotNames = new Dictionary<string, string>
    {
        { "Steam_MainRoute", "Steam_MainRoute_Concept.png" },
        { "Steam_EventPopup", "Steam_EventPopup_Concept.png" },
        { "Steam_Battle", "Steam_Battle_Concept.png" },
        { "Steam_Reward", "Steam_Reward_Concept.png" },
        { "Steam_CardLibrary", "Steam_CardLibrary_Concept.png" }
    };

    private static Font _font;

    [MenuItem("Codex/Cultivation UI/Phase78/Generate Steam Demo Samples And Screenshots")]
    public static void GenerateAllMenu()
    {
        GenerateAll();
    }

    [MenuItem("Codex/Cultivation UI/Phase78/Generate Steam Demo Prefabs")]
    public static void GeneratePrefabsMenu()
    {
        GenerateAssetsAndPrefabs();
    }

    [MenuItem("Codex/Cultivation UI/Phase78/Capture Steam Demo Screenshots")]
    public static void CaptureScreenshotsMenu()
    {
        CaptureAllScreenshots();
    }

    public static void GenerateAll()
    {
        GenerateAssetsAndPrefabs();
        CaptureAllScreenshots();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Phase78 Steam demo UI structure samples generated.");
    }

    public static void GenerateAssetsAndPrefabs()
    {
        EnsureDirectories();
        GeneratePlaceholderSprites();
        GenerateComponentPrefabs();
        GenerateTemplatePrefabs();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    public static void CaptureAllScreenshots()
    {
        foreach (var templateName in TemplateNames)
        {
            CaptureTemplate(templateName);
        }
    }

    private static void GenerateComponentPrefabs()
    {
        SaveComponent("Button_Primary", BuildButtonPrimary);
        SaveComponent("Button_Secondary", BuildButtonSecondary);
        SaveComponent("CardItem", BuildCardItem);
        SaveComponent("RouteNode", BuildRouteNode);
        SaveComponent("PopupWindow", BuildPopupWindow);
        SaveComponent("RewardItem", BuildRewardItem);
        SaveComponent("TopResourceItem", BuildTopResourceItem);
        SaveComponent("CharacterStatusPanel", BuildCharacterStatusPanel);
    }

    private static void GenerateTemplatePrefabs()
    {
        SaveTemplate("Steam_MainRoute", BuildMainRoute);
        SaveTemplate("Steam_EventPopup", BuildEventPopup);
        SaveTemplate("Steam_Battle", BuildBattle);
        SaveTemplate("Steam_Reward", BuildReward);
        SaveTemplate("Steam_CardLibrary", BuildCardLibrary);
    }

    private static void SaveComponent(string name, Action<Transform> build)
    {
        var root = new GameObject(name, typeof(RectTransform));
        build(root.transform);
        PrefabUtility.SaveAsPrefabAsset(root, ComponentDir + "/" + name + ".prefab");
        UnityEngine.Object.DestroyImmediate(root);
    }

    private static void SaveTemplate(string name, Action<Transform> build)
    {
        var root = CreateCanvasRoot(name);
        AddScreenBackground(root.transform, name);
        build(root.transform);
        PrefabUtility.SaveAsPrefabAsset(root, TemplateDir + "/" + name + ".prefab");
        UnityEngine.Object.DestroyImmediate(root);
    }

    private static GameObject CreateCanvasRoot(string name)
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

    private static void BuildButtonPrimary(Transform root)
    {
        SetRect(root, RectSpec.Center(Vector2.zero, new Vector2(300, 78)));
        var bg = AddImage("Bg", root, SpriteAt("Button/Button_Primary_Scroll_Normal.png"), RectSpec.Stretch(Vector2.zero, Vector2.zero));
        bg.type = Image.Type.Sliced;
        bg.color = new Color(1f, 0.96f, 0.78f, 1f);
        var button = root.gameObject.AddComponent<Button>();
        button.targetGraphic = bg;
        AddText("Label", root, "确认", 28, Hex("#2D1D0E"), TextAnchor.MiddleCenter, RectSpec.Stretch(new Vector2(28, 8), new Vector2(28, 8)), FontStyle.Bold);
    }

    private static void BuildButtonSecondary(Transform root)
    {
        SetRect(root, RectSpec.Center(Vector2.zero, new Vector2(220, 64)));
        var bg = AddImage("Bg", root, SpriteAt("Button/Button_Secondary_Jade_Normal.png"), RectSpec.Stretch(Vector2.zero, Vector2.zero));
        bg.type = Image.Type.Sliced;
        bg.color = new Color(0.82f, 1f, 0.93f, 0.92f);
        var button = root.gameObject.AddComponent<Button>();
        button.targetGraphic = bg;
        AddText("Label", root, "返回", 24, Hex("#EDE6C8"), TextAnchor.MiddleCenter, RectSpec.Stretch(new Vector2(22, 6), new Vector2(22, 6)), FontStyle.Bold);
    }

    private static void BuildCardItem(Transform root)
    {
        SetRect(root, RectSpec.Center(Vector2.zero, new Vector2(190, 292)));
        var bg = AddImage("CardFrame", root, SpriteAt("Card/Card_Cultivation_Rare.png"), RectSpec.Stretch(Vector2.zero, Vector2.zero));
        bg.type = Image.Type.Sliced;
        AddImage("Art", root, SpriteAt("Placeholders/PH_CardArt_Sword.png"), RectSpec.Top(new Vector2(0, -79), new Vector2(150, 102)));
        AddImage("CostOrb", root, SpriteAt("Card/CostOrb_Gold.png"), RectSpec.Top(new Vector2(-76, -30), new Vector2(44, 44)));
        AddText("CostText", root, "1", 23, Hex("#10201D"), TextAnchor.MiddleCenter, RectSpec.Top(new Vector2(-76, -29), new Vector2(42, 42)), FontStyle.Bold);
        AddText("Title", root, "青木剑诀", 22, Hex("#F2DFAB"), TextAnchor.MiddleCenter, RectSpec.Top(new Vector2(18, -29), new Vector2(126, 36)), FontStyle.Bold);
        AddText("TypeTag", root, "剑诀", 16, Hex("#D1B978"), TextAnchor.MiddleRight, RectSpec.Top(new Vector2(42, -178), new Vector2(96, 24)));
        AddText("Desc", root, "造成 6 点伤害。\n若本回合使用过剑诀，抽 1 张牌。", 16, Hex("#E7DEC2"), TextAnchor.UpperLeft, RectSpec.Bottom(new Vector2(0, 42), new Vector2(142, 70)));
        AddText("Count", root, "x2", 15, Hex("#9FDBCA"), TextAnchor.MiddleRight, RectSpec.Bottom(new Vector2(62, 17), new Vector2(44, 22)), FontStyle.Bold);
    }

    private static void BuildRouteNode(Transform root)
    {
        SetRect(root, RectSpec.Center(Vector2.zero, new Vector2(94, 94)));
        var glow = AddImage("CurrentGlow", root, SpriteAt("RouteNode/Node_Current_Glow.png"), RectSpec.Center(Vector2.zero, new Vector2(112, 112)));
        glow.color = new Color(1f, 0.84f, 0.35f, 0.55f);
        AddImage("Icon", root, SpriteAt("RouteNode/Node_Battle_Sword.png"), RectSpec.Center(Vector2.zero, new Vector2(82, 82)));
        AddText("Index", root, "战", 18, Hex("#FFE4A0"), TextAnchor.MiddleCenter, RectSpec.Bottom(new Vector2(0, -14), new Vector2(42, 24)), FontStyle.Bold);
    }

    private static void BuildPopupWindow(Transform root)
    {
        SetRect(root, RectSpec.Center(Vector2.zero, new Vector2(680, 820)));
        var bg = AddImage("Bg", root, SpriteAt("Popup/Popup_DarkJade_GoldBorder.png"), RectSpec.Stretch(Vector2.zero, Vector2.zero));
        bg.type = Image.Type.Sliced;
        AddText("Title", root, "古井灵光", 36, Hex("#F2D58A"), TextAnchor.MiddleCenter, RectSpec.Top(new Vector2(0, -54), new Vector2(520, 52)), FontStyle.Bold);
        AddImage("HeroImage", root, SpriteAt("Placeholders/PH_EventWell.png"), RectSpec.Top(new Vector2(0, -184), new Vector2(514, 250)));
        AddText("Body", root, "你在裂纹石阶尽头发现一口古井，井中灵光流转，似有机缘，也似有凶险。", 22, Hex("#EAE0C4"), TextAnchor.UpperLeft, RectSpec.Top(new Vector2(0, -366), new Vector2(520, 92)));
        AddOptionRow(root, "Option_1", "汲取灵气", "灵力 +15", "机缘", -490, Hex("#81CDAF"));
        AddOptionRow(root, "Option_2", "谨慎离开", "无事发生", "安全", -586, Hex("#DCC07C"));
        AddOptionRow(root, "Option_3", "投入灵石", "灵石 -30", "风险", -682, Hex("#C56A55"));
    }

    private static void BuildRewardItem(Transform root)
    {
        SetRect(root, RectSpec.Center(Vector2.zero, new Vector2(238, 338)));
        var bg = AddImage("Bg", root, SpriteAt("Panel/Panel_Secondary_Parchment.png"), RectSpec.Stretch(Vector2.zero, Vector2.zero));
        bg.type = Image.Type.Sliced;
        AddImage("Icon", root, SpriteAt("Placeholders/PH_Reward_Jade.png"), RectSpec.Top(new Vector2(0, -102), new Vector2(150, 150)));
        AddText("Title", root, "灵石", 28, Hex("#F1D58D"), TextAnchor.MiddleCenter, RectSpec.Top(new Vector2(0, -34), new Vector2(172, 40)), FontStyle.Bold);
        AddText("Desc", root, "灵石 +45", 23, Hex("#E6DEC3"), TextAnchor.MiddleCenter, RectSpec.Bottom(new Vector2(0, 72), new Vector2(180, 52)));
        AddText("Rarity", root, "奖励", 16, Hex("#8FD8BE"), TextAnchor.MiddleCenter, RectSpec.Bottom(new Vector2(0, 30), new Vector2(110, 28)));
    }

    private static void BuildTopResourceItem(Transform root)
    {
        SetRect(root, RectSpec.Center(Vector2.zero, new Vector2(172, 52)));
        var bg = AddImage("Bg", root, SpriteAt("Panel/Panel_Top_JadePlaque.png"), RectSpec.Stretch(Vector2.zero, Vector2.zero));
        bg.type = Image.Type.Sliced;
        AddText("Label", root, "灵石", 15, Hex("#B9A982"), TextAnchor.MiddleLeft, RectSpec.Stretch(new Vector2(18, 5), new Vector2(94, 5)));
        AddText("Value", root, "320", 18, Hex("#E9D18A"), TextAnchor.MiddleRight, RectSpec.Stretch(new Vector2(70, 5), new Vector2(18, 5)), FontStyle.Bold);
    }

    private static void BuildCharacterStatusPanel(Transform root)
    {
        SetRect(root, RectSpec.Center(Vector2.zero, new Vector2(328, 820)));
        var bg = AddImage("Bg", root, SpriteAt("Panel/Panel_Main_JadeThin.png"), RectSpec.Stretch(Vector2.zero, Vector2.zero));
        bg.type = Image.Type.Sliced;
        AddText("Title", root, "修士档案", 30, Hex("#F1D58D"), TextAnchor.MiddleCenter, RectSpec.Top(new Vector2(0, -36), new Vector2(250, 42)), FontStyle.Bold);
        AddImage("Portrait", root, SpriteAt("Placeholders/PH_CharacterPortrait_Ink.png"), RectSpec.Top(new Vector2(0, -152), new Vector2(242, 190)));
        AddText("Name", root, "炼云子", 26, Hex("#EFE4C3"), TextAnchor.MiddleCenter, RectSpec.Top(new Vector2(0, -262), new Vector2(250, 42)), FontStyle.Bold);
        AddStatusLine(root, "境界", "炼气三层", -326);
        AddStatusLine(root, "门派", "青云剑宗", -374);
        AddBar(root, "生命", "120/120", -436, Hex("#C95E54"), 1f);
        AddBar(root, "灵力", "80/80", -496, Hex("#58BFA4"), 0.74f);
        AddStatusLine(root, "心境", "平和", -558);
        AddText("BuffTitle", root, "护身符", 20, Hex("#D8BF78"), TextAnchor.MiddleLeft, RectSpec.Top(new Vector2(-74, -612), new Vector2(116, 30)), FontStyle.Bold);
        for (var i = 0; i < 4; i++)
        {
            var icon = AddImage("Buff_" + i, root, SpriteAt("Panel/Panel_Bamboo_Tag.png"), RectSpec.Top(new Vector2(-102 + i * 68, -668), new Vector2(44, 58)));
            icon.type = Image.Type.Sliced;
            AddText("BuffGlyph_" + i, root, new[] { "护", "悟", "息", "运" }[i], 18, Hex("#F0D9A0"), TextAnchor.MiddleCenter, RectSpec.Top(new Vector2(-102 + i * 68, -668), new Vector2(44, 58)), FontStyle.Bold);
        }
        AddText("Note", root, "状态说明：结构占位，后续绑定角色属性、Buff 列表与临时状态说明。", 18, Hex("#CDBE9A"), TextAnchor.UpperLeft, RectSpec.Bottom(new Vector2(0, 34), new Vector2(260, 88)));
    }

    private static void BuildMainRoute(Transform root)
    {
        AddHeader(root, "1. 主界面 / 秘境路线界面");
        AddTopBar(root);
        var status = InstantiateComponent("CharacterStatusPanel", root, "Left_CharacterStatusPanel", new Vector2(-765, 18), new Vector2(328, 820));
        SetChildText(status, "Name", "炼云子");

        var map = AddImage("Center_RouteMapPanel", root, SpriteAt("RouteNode/RouteMap_MistPanel.png"), RectSpec.Center(new Vector2(-92, 25), new Vector2(920, 762)));
        map.type = Image.Type.Sliced;
        AddText("MapTitle", root, "秘境 · 幽云谷", 31, Hex("#F1D58D"), TextAnchor.MiddleCenter, RectSpec.Top(new Vector2(-92, -106), new Vector2(420, 48)), FontStyle.Bold);
        AddText("MapHint", root, "提示：扣点表达秘境探索路径，连线为灵脉路线占位。", 18, Hex("#C9B891"), TextAnchor.MiddleCenter, RectSpec.Bottom(new Vector2(-92, 122), new Vector2(590, 34)));

        var points = new[]
        {
            new Vector2(-382, -108),
            new Vector2(-166, 74),
            new Vector2(42, -18),
            new Vector2(242, 162),
            new Vector2(390, -70),
            new Vector2(185, -250),
            new Vector2(-88, -260),
            new Vector2(-308, 182)
        };

        for (var i = 0; i < points.Length - 1; i++)
        {
            AddSpiritLine(root, points[i] + new Vector2(-92, 25), points[i + 1] + new Vector2(-92, 25), "RouteLine_" + i);
        }

        var icons = new[]
        {
            "RouteNode/Node_Battle_Sword.png",
            "RouteNode/Node_Event_Scroll.png",
            "RouteNode/Node_Chest_Box.png",
            "RouteNode/Node_Rest_Meditation.png",
            "RouteNode/Node_Battle_Sword.png",
            "RouteNode/Node_Market_Pavilion.png",
            "RouteNode/Node_Event_Scroll.png",
            "RouteNode/Node_Chest_Box.png"
        };

        for (var i = 0; i < points.Length; i++)
        {
            var node = InstantiateComponent("RouteNode", root, "RouteNode_" + i, points[i] + new Vector2(-92, 25), new Vector2(94, 94));
            SetChildSprite(node, "Icon", icons[i]);
            SetChildText(node, "Index", new[] { "战", "事", "宝", "息", "战", "市", "奇", "匣" }[i]);
            var glow = node.transform.Find("CurrentGlow");
            if (glow != null)
            {
                glow.gameObject.SetActive(i == 1 || i == 4);
            }
        }

        var deck = AddImage("Right_CardDeckPanel", root, SpriteAt("Panel/Panel_Main_JadeThin.png"), RectSpec.Center(new Vector2(768, 78), new Vector2(336, 708)));
        deck.type = Image.Type.Sliced;
        AddText("DeckTitle", root, "功法卡组", 28, Hex("#F1D58D"), TextAnchor.MiddleCenter, RectSpec.Top(new Vector2(768, -112), new Vector2(240, 40)), FontStyle.Bold);
        AddText("DeckCount", root, "卡牌 8/20", 18, Hex("#CDBE9A"), TextAnchor.MiddleCenter, RectSpec.Top(new Vector2(768, -154), new Vector2(160, 26)));
        AddMiniCard(root, "青木剑诀", "1", new Vector2(768, 216), SpriteAt("Placeholders/PH_CardArt_Sword.png"));
        AddMiniCard(root, "护身符", "1", new Vector2(768, 18), SpriteAt("Placeholders/PH_CardArt_Talisman.png"));
        AddMiniCard(root, "聚气术", "0", new Vector2(768, -180), SpriteAt("Placeholders/PH_CardArt_Qi.png"));

        var bottom = AddImage("Bottom_ActionDock", root, SpriteAt("Panel/Panel_Bottom_Transparent.png"), RectSpec.Bottom(new Vector2(0, 38), new Vector2(1320, 88)));
        bottom.type = Image.Type.Sliced;
        InstantiateComponent("Button_Secondary", root, "Btn_Bag", new Vector2(-236, -470), new Vector2(170, 58), "背包");
        InstantiateComponent("Button_Secondary", root, "Btn_Card", new Vector2(0, -470), new Vector2(170, 58), "功法");
        InstantiateComponent("Button_Secondary", root, "Btn_Setting", new Vector2(236, -470), new Vector2(170, 58), "设置");
        InstantiateComponent("Button_Primary", root, "Btn_Continue", new Vector2(670, -468), new Vector2(300, 72), "继续前进");
    }

    private static void BuildEventPopup(Transform root)
    {
        AddHeader(root, "2. 秘境事件弹窗");
        var popup = InstantiateComponent("PopupWindow", root, "EventPopup_AncientWell", Vector2.zero, new Vector2(680, 820));
        SetChildText(popup, "Title", "古井灵光");
        AddText("TurnTag", root, "回合 2", 20, Hex("#8F8060"), TextAnchor.MiddleRight, RectSpec.Top(new Vector2(530, -34), new Vector2(190, 34)));
        InstantiateComponent("Button_Secondary", root, "Btn_Close", new Vector2(570, 372), new Vector2(58, 58), "X");
        AddText("RewardHint", root, "可能获得：", 20, Hex("#E7D6A4"), TextAnchor.MiddleLeft, RectSpec.Center(new Vector2(-172, -348), new Vector2(130, 34)), FontStyle.Bold);
        AddRewardBadge(root, SpriteAt("Placeholders/PH_Reward_Unknown.png"), new Vector2(-42, -348));
        AddRewardBadge(root, SpriteAt("Placeholders/PH_Reward_Qi.png"), new Vector2(58, -348));
        AddRewardBadge(root, SpriteAt("Placeholders/PH_Reward_Jade.png"), new Vector2(158, -348));
    }

    private static void BuildBattle(Transform root)
    {
        AddHeader(root, "3. 战斗界面");
        AddTopBar(root);
        AddImage("BattleBackdrop", root, SpriteAt("Placeholders/PH_BattleBackground_Mountains.png"), RectSpec.Center(new Vector2(0, 40), new Vector2(1600, 800))).color = new Color(1f, 1f, 1f, 0.88f);
        AddCombatant(root, "幽冥鬼卒", "46/60", new Vector2(-355, 250), true);
        AddCombatant(root, "邪修散人", "75/89", new Vector2(410, 250), false);
        AddCharacterHero(root, "PlayerSilhouette", new Vector2(-275, 78), true);
        AddCharacterHero(root, "EnemySilhouette", new Vector2(392, 88), false);

        var status = AddImage("PlayerStatusDock", root, SpriteAt("Panel/Panel_Main_JadeThin.png"), RectSpec.Bottom(new Vector2(-620, 112), new Vector2(318, 220)));
        status.type = Image.Type.Sliced;
        AddImage("PlayerPortrait", root, SpriteAt("Placeholders/PH_CharacterPortrait_Ink.png"), RectSpec.Bottom(new Vector2(-748, 152), new Vector2(92, 92)));
        AddText("PlayerName", root, "炼云子\n炼气三层", 21, Hex("#F2DDAA"), TextAnchor.MiddleLeft, RectSpec.Bottom(new Vector2(-585, 180), new Vector2(160, 58)), FontStyle.Bold);
        AddBar(root, "气血", "120/120", new Vector2(-548, 126), 172, Hex("#C95E54"), 1f);
        AddBar(root, "灵力", "80/80", new Vector2(-548, 86), 172, Hex("#58BFA4"), 0.74f);
        AddText("ManaPips", root, "3/3   ● ● ●", 22, Hex("#83D9C0"), TextAnchor.MiddleLeft, RectSpec.Bottom(new Vector2(-614, 42), new Vector2(210, 32)), FontStyle.Bold);

        var cardPositions = new[] { -310, -154, 2, 158, 314 };
        var cardNames = new[] { "青木剑诀", "幻影步", "护身符", "聚气术", "流光同一诀" };
        var costs = new[] { "1", "1", "1", "0", "2" };
        var arts = new[]
        {
            "Placeholders/PH_CardArt_Sword.png",
            "Placeholders/PH_CardArt_Qi.png",
            "Placeholders/PH_CardArt_Talisman.png",
            "Placeholders/PH_CardArt_Qi.png",
            "Placeholders/PH_CardArt_Sword.png"
        };

        for (var i = 0; i < cardPositions.Length; i++)
        {
            var card = InstantiateComponent("CardItem", root, "HandCard_" + i, new Vector2(cardPositions[i], -332 + Mathf.Abs(i - 2) * 9), new Vector2(158, 244));
            card.transform.localRotation = Quaternion.Euler(0, 0, (i - 2) * -5f);
            ConfigureCard(card, cardNames[i], costs[i], arts[i], CardDisplay.Hand);
        }

        InstantiateComponent("Button_Primary", root, "Btn_EndTurn", new Vector2(728, -248), new Vector2(232, 72), "结束回合");
        AddBattleCommand(root, "抽牌堆", "15", new Vector2(760, -350));
        AddBattleCommand(root, "弃牌堆", "3", new Vector2(760, -416));
        AddBattleCommand(root, "消耗堆", "1", new Vector2(760, -482));
    }

    private static void BuildReward(Transform root)
    {
        AddHeader(root, "4. 奖励 / 宝箱 / 战斗结算界面");
        AddText("RewardTitle", root, "获得机缘", 48, Hex("#F1D58D"), TextAnchor.MiddleCenter, RectSpec.Top(new Vector2(0, -118), new Vector2(420, 68)), FontStyle.Bold);
        AddImage("TitleLine", root, SpriteAt("Divider/Divider_InkGold.png"), RectSpec.Top(new Vector2(0, -178), new Vector2(640, 18))).color = new Color(1f, 0.78f, 0.36f, 0.58f);

        var left = InstantiateComponent("RewardItem", root, "Reward_Card", new Vector2(-360, 75), new Vector2(250, 360));
        SetChildText(left, "Title", "清风剑诀");
        SetChildText(left, "Desc", "造成 6 点伤害。\n若本回合使用过剑诀，抽 1 张牌。");
        SetChildSprite(left, "Icon", "Placeholders/PH_CardArt_Sword.png");

        var middle = InstantiateComponent("RewardItem", root, "Reward_Jade", new Vector2(0, 75), new Vector2(250, 360));
        SetChildText(middle, "Title", "灵石");
        SetChildText(middle, "Desc", "灵石 +45");
        SetChildSprite(middle, "Icon", "Placeholders/PH_Reward_Jade.png");

        var right = InstantiateComponent("RewardItem", root, "Reward_Scroll", new Vector2(360, 75), new Vector2(250, 360));
        SetChildText(right, "Title", "破旧玉简");
        SetChildText(right, "Desc", "解锁一门残缺功法，\n可在功法中查看。");
        SetChildSprite(right, "Icon", "Placeholders/PH_Reward_Scroll.png");

        AddText("ChoiceHint", root, "请选择一项奖励", 29, Hex("#E7D6A4"), TextAnchor.MiddleCenter, RectSpec.Bottom(new Vector2(0, 218), new Vector2(360, 44)), FontStyle.Bold);
        InstantiateComponent("Button_Secondary", root, "Btn_Skip", new Vector2(-280, -360), new Vector2(250, 64), "跳过");
        InstantiateComponent("Button_Primary", root, "Btn_Confirm", new Vector2(340, -360), new Vector2(260, 70), "确认");
    }

    private static void BuildCardLibrary(Transform root)
    {
        AddHeader(root, "5. 功法 / 卡牌详情界面");
        var side = AddImage("CategoryTabs", root, SpriteAt("Panel/Panel_Main_JadeThin.png"), RectSpec.Left(new Vector2(42, 24), new Vector2(200, 790)));
        side.type = Image.Type.Sliced;
        AddText("CategoryTitle", root, "分类", 25, Hex("#F1D58D"), TextAnchor.MiddleCenter, RectSpec.Top(new Vector2(-768, -126), new Vector2(140, 38)), FontStyle.Bold);
        var tabs = new[] { "全部", "剑诀", "身法", "心法", "符箓", "灵兽" };
        for (var i = 0; i < tabs.Length; i++)
        {
            InstantiateComponent(i == 0 ? "Button_Primary" : "Button_Secondary", root, "Tab_" + tabs[i], new Vector2(-768, 292 - i * 82), new Vector2(150, 56), tabs[i]);
        }

        AddText("ListTitle", root, "卡牌列表  (16/40)", 26, Hex("#F1D58D"), TextAnchor.MiddleLeft, RectSpec.Top(new Vector2(-134, -88), new Vector2(420, 42)), FontStyle.Bold);
        var cardData = new[]
        {
            ("青木剑诀", "1", "Placeholders/PH_CardArt_Sword.png"),
            ("幻影步", "1", "Placeholders/PH_CardArt_Qi.png"),
            ("护身符", "1", "Placeholders/PH_CardArt_Talisman.png"),
            ("聚气术", "0", "Placeholders/PH_CardArt_Qi.png"),
            ("混元归一诀", "2", "Placeholders/PH_CardArt_Qi.png"),
            ("流云剑阵", "2", "Placeholders/PH_CardArt_Sword.png"),
            ("紫气诀", "1", "Placeholders/PH_CardArt_Qi.png"),
            ("玄玉麒麟", "3", "Placeholders/PH_Reward_Qi.png")
        };

        for (var i = 0; i < cardData.Length; i++)
        {
            var x = -406 + (i % 4) * 190;
            var y = 210 - (i / 4) * 318;
            var card = InstantiateComponent("CardItem", root, "LibraryCard_" + i, new Vector2(x, y), new Vector2(168, 258));
            ConfigureCard(card, cardData[i].Item1, cardData[i].Item2, cardData[i].Item3, CardDisplay.Compact);
        }

        var detail = AddImage("DetailPanel", root, SpriteAt("Panel/Panel_Main_JadeThin.png"), RectSpec.Right(new Vector2(-290, 12), new Vector2(430, 820)));
        detail.type = Image.Type.Sliced;
        var bigCard = InstantiateComponent("CardItem", root, "SelectedCardPreview", new Vector2(632, 184), new Vector2(274, 420));
        ConfigureCard(bigCard, "青木剑诀", "1", "Placeholders/PH_CardArt_Sword.png", CardDisplay.Detail);
        AddText("UpgradeTitle", root, "升级效果", 25, Hex("#F1D58D"), TextAnchor.MiddleLeft, RectSpec.Center(new Vector2(646, -86), new Vector2(300, 38)), FontStyle.Bold);
        AddText("UpgradeDesc", root, "造成 8 点伤害。\n若本回合使用过剑诀，额外抽 1 张牌。", 21, Hex("#E7DCC2"), TextAnchor.UpperLeft, RectSpec.Center(new Vector2(646, -166), new Vector2(322, 112)));
        InstantiateComponent("Button_Primary", root, "Btn_Upgrade", new Vector2(568, -398), new Vector2(190, 64), "升级");
        InstantiateComponent("Button_Secondary", root, "Btn_Equip", new Vector2(760, -398), new Vector2(170, 64), "装备");
        AddText("Currency", root, "灵石：320", 19, Hex("#D8BF78"), TextAnchor.MiddleRight, RectSpec.Bottom(new Vector2(770, 38), new Vector2(190, 32)), FontStyle.Bold);
        InstantiateComponent("Button_Secondary", root, "Btn_Close", new Vector2(872, 472), new Vector2(58, 58), "X");
    }

    private static void AddScreenBackground(Transform root, string templateName)
    {
        var bg = AddImage("Background_MountainMist", root, SpriteAt("Background/BG_Cultivation_MountainMist.png"), RectSpec.Stretch(Vector2.zero, Vector2.zero));
        bg.raycastTarget = false;
        if (templateName == "Steam_Battle")
        {
            bg.color = new Color(0.58f, 0.67f, 0.66f, 0.96f);
        }

        var rune = AddImage("Background_RuneOverlay", root, SpriteAt("Background/BG_Cultivation_RunePattern.png"), RectSpec.Center(Vector2.zero, new Vector2(1260, 760)));
        rune.raycastTarget = false;
        rune.color = new Color(0.55f, 0.9f, 0.76f, 0.14f);

        var vignette = AddImage("Background_Vignette", root, SpriteAt("Background/BG_Cultivation_Vignette.png"), RectSpec.Stretch(Vector2.zero, Vector2.zero));
        vignette.raycastTarget = false;
        vignette.color = new Color(1f, 1f, 1f, 0.96f);
    }

    private static void AddHeader(Transform root, string text)
    {
        var header = AddImage("ScreenHeader", root, SpriteAt("Panel/Panel_Bottom_Transparent.png"), RectSpec.Top(new Vector2(-670, -42), new Vector2(540, 70)));
        header.type = Image.Type.Sliced;
        header.color = new Color(0.03f, 0.06f, 0.05f, 0.78f);
        AddText("ScreenHeaderText", root, text, 31, Hex("#F2E8D0"), TextAnchor.MiddleLeft, RectSpec.Top(new Vector2(-664, -42), new Vector2(496, 54)), FontStyle.Bold);
    }

    private static void AddTopBar(Transform root)
    {
        var data = new[]
        {
            ("境界", "炼气三层"),
            ("战力", "80/80"),
            ("灵石", "320"),
            ("天数", "第 3 日")
        };

        for (var i = 0; i < data.Length; i++)
        {
            var item = InstantiateComponent("TopResourceItem", root, "TopResource_" + i, new Vector2(-190 + i * 220, 472), new Vector2(182, 54));
            SetChildText(item, "Label", data[i].Item1);
            SetChildText(item, "Value", data[i].Item2);
        }
    }

    private static void AddOptionRow(Transform root, string name, string title, string result, string tag, float y, Color tagColor)
    {
        var row = AddImage(name, root, SpriteAt("Panel/Panel_Secondary_Parchment.png"), RectSpec.Top(new Vector2(0, y), new Vector2(520, 72)));
        row.type = Image.Type.Sliced;
        AddText(name + "_Title", root, title, 22, Hex("#F2DDAA"), TextAnchor.MiddleLeft, RectSpec.Top(new Vector2(-136, y), new Vector2(220, 44)), FontStyle.Bold);
        AddText(name + "_Result", root, result, 18, Hex("#D9CEAE"), TextAnchor.MiddleCenter, RectSpec.Top(new Vector2(94, y), new Vector2(154, 44)));
        AddText(name + "_Tag", root, tag, 17, tagColor, TextAnchor.MiddleCenter, RectSpec.Top(new Vector2(220, y), new Vector2(72, 32)), FontStyle.Bold);
    }

    private static void AddStatusLine(Transform root, string label, string value, float y)
    {
        AddText("Line_" + label, root, label, 18, Hex("#BFAE82"), TextAnchor.MiddleLeft, RectSpec.Top(new Vector2(-76, y), new Vector2(80, 28)));
        AddText("Value_" + label, root, value, 19, Hex("#EEE1BE"), TextAnchor.MiddleRight, RectSpec.Top(new Vector2(58, y), new Vector2(150, 28)), FontStyle.Bold);
    }

    private static void AddBar(Transform root, string label, string value, float y, Color color, float fill)
    {
        AddText("BarLabel_" + label, root, label, 18, Hex("#BFAE82"), TextAnchor.MiddleLeft, RectSpec.Top(new Vector2(-100, y), new Vector2(82, 26)));
        AddImage("BarBack_" + label, root, SpriteAt("Panel/Panel_Bottom_Transparent.png"), RectSpec.Top(new Vector2(42, y), new Vector2(170, 16))).color = new Color(0.03f, 0.06f, 0.05f, 0.78f);
        AddImage("BarFill_" + label, root, SpriteAt("Divider/Divider_InkGold.png"), RectSpec.Top(new Vector2(42 - 85 * (1f - fill), y), new Vector2(170 * fill, 10))).color = color;
        AddText("BarText_" + label, root, value, 15, Hex("#EEE1BE"), TextAnchor.MiddleRight, RectSpec.Top(new Vector2(44, y - 20), new Vector2(166, 20)));
    }

    private static void AddBar(Transform root, string label, string value, Vector2 center, float width, Color color, float fill)
    {
        AddText("Label_" + label, root, label, 16, Hex("#BFAE82"), TextAnchor.MiddleLeft, RectSpec.Center(center + new Vector2(-105, 0), new Vector2(70, 24)));
        AddImage("Back_" + label, root, SpriteAt("Panel/Panel_Bottom_Transparent.png"), RectSpec.Center(center, new Vector2(width, 14))).color = new Color(0.03f, 0.06f, 0.05f, 0.78f);
        AddImage("Fill_" + label, root, SpriteAt("Divider/Divider_InkGold.png"), RectSpec.Center(center + new Vector2(-width * (1f - fill) * 0.5f, 0), new Vector2(width * fill, 8))).color = color;
        AddText("Text_" + label, root, value, 14, Hex("#EDE0BD"), TextAnchor.MiddleRight, RectSpec.Center(center + new Vector2(0, -20), new Vector2(width, 20)));
    }

    private static void AddSpiritLine(Transform root, Vector2 from, Vector2 to, string name)
    {
        var center = (from + to) * 0.5f;
        var size = new Vector2(Vector2.Distance(from, to), 26);
        var line = AddImage(name, root, SpriteAt("RouteNode/RoutePath_SpiritLine.png"), RectSpec.Center(center, size));
        line.rectTransform.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(to.y - from.y, to.x - from.x) * Mathf.Rad2Deg);
        line.raycastTarget = false;
    }

    private static void AddMiniCard(Transform root, string title, string cost, Vector2 position, Sprite art)
    {
        var card = InstantiateComponent("CardItem", root, "MiniCard_" + title, position, new Vector2(236, 158));
        ConfigureCard(card, title, cost, art, CardDisplay.CompactWide);
    }

    private static void ConfigureCard(GameObject card, string title, string cost, string artPath, CardDisplay display)
    {
        ConfigureCard(card, title, cost, SpriteAt(artPath), display);
    }

    private static void ConfigureCard(GameObject card, string title, string cost, Sprite art, CardDisplay display)
    {
        SetChildText(card, "Title", title);
        SetChildText(card, "CostText", cost);
        SetChildSprite(card, "Art", art);

        var titleText = FindDeep(card.transform, "Title")?.GetComponent<Text>();
        var costText = FindDeep(card.transform, "CostText")?.GetComponent<Text>();
        var descText = FindDeep(card.transform, "Desc")?.GetComponent<Text>();
        var typeTag = FindDeep(card.transform, "TypeTag");
        var count = FindDeep(card.transform, "Count");
        var costOrb = FindDeep(card.transform, "CostOrb");
        var artNode = FindDeep(card.transform, "Art");

        if (display == CardDisplay.CompactWide)
        {
            if (titleText != null) titleText.fontSize = 20;
            if (costText != null) costText.fontSize = 21;
            if (descText != null) descText.gameObject.SetActive(false);
            if (typeTag != null) typeTag.gameObject.SetActive(false);
            if (count != null) SetRect(count, RectSpec.Bottom(new Vector2(82, 20), new Vector2(34, 22)));
            if (costOrb != null) SetRect(costOrb, RectSpec.Top(new Vector2(-92, -32), new Vector2(42, 42)));
            if (artNode != null) SetRect(artNode, RectSpec.Center(new Vector2(0, -14), new Vector2(128, 84)));
            SetRect(FindDeep(card.transform, "Title"), RectSpec.Top(new Vector2(18, -32), new Vector2(152, 32)));
            SetRect(FindDeep(card.transform, "CostText"), RectSpec.Top(new Vector2(-92, -31), new Vector2(40, 40)));
        }
        else if (display == CardDisplay.Compact)
        {
            if (titleText != null) titleText.fontSize = 18;
            if (costText != null) costText.fontSize = 20;
            if (descText != null) descText.gameObject.SetActive(false);
            if (typeTag != null) typeTag.gameObject.SetActive(false);
            if (count != null) SetRect(count, RectSpec.Bottom(new Vector2(56, 18), new Vector2(34, 20)));
            if (costOrb != null) SetRect(costOrb, RectSpec.Top(new Vector2(-65, -30), new Vector2(38, 38)));
            if (artNode != null) SetRect(artNode, RectSpec.Top(new Vector2(0, -92), new Vector2(122, 88)));
            SetRect(FindDeep(card.transform, "Title"), RectSpec.Top(new Vector2(16, -31), new Vector2(110, 30)));
            SetRect(FindDeep(card.transform, "CostText"), RectSpec.Top(new Vector2(-65, -29), new Vector2(36, 36)));
        }
        else if (display == CardDisplay.Hand)
        {
            if (titleText != null) titleText.fontSize = 18;
            if (costText != null) costText.fontSize = 20;
            if (descText != null)
            {
                descText.fontSize = 13;
                descText.lineSpacing = 0.85f;
                SetRect(descText.transform, RectSpec.Bottom(new Vector2(0, 46), new Vector2(122, 54)));
            }
            if (typeTag != null) typeTag.gameObject.SetActive(false);
            if (count != null) SetRect(count, RectSpec.Bottom(new Vector2(52, 17), new Vector2(30, 20)));
            if (costOrb != null) SetRect(costOrb, RectSpec.Top(new Vector2(-62, -29), new Vector2(38, 38)));
            if (artNode != null) SetRect(artNode, RectSpec.Top(new Vector2(0, -86), new Vector2(118, 82)));
            SetRect(FindDeep(card.transform, "Title"), RectSpec.Top(new Vector2(16, -30), new Vector2(106, 30)));
            SetRect(FindDeep(card.transform, "CostText"), RectSpec.Top(new Vector2(-62, -28), new Vector2(36, 36)));
        }
        else if (display == CardDisplay.Detail)
        {
            if (titleText != null) titleText.fontSize = 28;
            if (costText != null) costText.fontSize = 28;
            if (descText != null)
            {
                descText.fontSize = 21;
                descText.lineSpacing = 1.05f;
                SetRect(descText.transform, RectSpec.Bottom(new Vector2(0, 82), new Vector2(210, 92)));
            }
            if (costOrb != null) SetRect(costOrb, RectSpec.Top(new Vector2(-108, -46), new Vector2(58, 58)));
            if (artNode != null) SetRect(artNode, RectSpec.Top(new Vector2(0, -140), new Vector2(210, 142)));
            SetRect(FindDeep(card.transform, "Title"), RectSpec.Top(new Vector2(22, -46), new Vector2(176, 44)));
            SetRect(FindDeep(card.transform, "CostText"), RectSpec.Top(new Vector2(-108, -45), new Vector2(56, 56)));
            if (typeTag != null) SetRect(typeTag, RectSpec.Bottom(new Vector2(82, 144), new Vector2(86, 28)));
            if (count != null) SetRect(count, RectSpec.Bottom(new Vector2(88, 38), new Vector2(44, 24)));
        }
    }

    private static void AddRewardBadge(Transform root, Sprite sprite, Vector2 position)
    {
        var badge = AddImage("RewardBadge", root, SpriteAt("Panel/Panel_Bottom_Transparent.png"), RectSpec.Center(position, new Vector2(72, 72)));
        badge.type = Image.Type.Sliced;
        AddImage("RewardBadgeIcon", root, sprite, RectSpec.Center(position, new Vector2(48, 48)));
    }

    private static void AddCombatant(Transform root, string name, string hp, Vector2 position, bool left)
    {
        AddText("Combatant_" + name, root, name, 24, Hex("#F0D6A2"), TextAnchor.MiddleCenter, RectSpec.Center(position + new Vector2(0, 44), new Vector2(220, 36)), FontStyle.Bold);
        AddImage("HpBack_" + name, root, SpriteAt("Panel/Panel_Bottom_Transparent.png"), RectSpec.Center(position + new Vector2(0, 8), new Vector2(180, 16))).color = new Color(0.04f, 0.04f, 0.04f, 0.8f);
        AddImage("HpFill_" + name, root, SpriteAt("Divider/Divider_InkGold.png"), RectSpec.Center(position + new Vector2(-18, 8), new Vector2(144, 10))).color = Hex("#C95E54");
        AddText("HpText_" + name, root, hp, 15, Hex("#E7DCC2"), TextAnchor.MiddleCenter, RectSpec.Center(position + new Vector2(0, -14), new Vector2(120, 22)));
        AddText("Intent_" + name, root, left ? "意图：攻击" : "意图：防御", 17, left ? Hex("#D78C5A") : Hex("#86B9E8"), TextAnchor.MiddleCenter, RectSpec.Center(position + new Vector2(0, -48), new Vector2(150, 26)));
    }

    private static void AddCharacterHero(Transform root, string name, Vector2 position, bool player)
    {
        var sprite = player ? SpriteAt("Placeholders/PH_PlayerInkFigure.png") : SpriteAt("Placeholders/PH_EnemyInkFigure.png");
        var image = AddImage(name, root, sprite, RectSpec.Center(position, new Vector2(290, 340)));
        image.raycastTarget = false;
    }

    private static void AddBattleCommand(Transform root, string label, string value, Vector2 position)
    {
        var bg = AddImage("Command_" + label, root, SpriteAt("Panel/Panel_Bottom_Transparent.png"), RectSpec.Center(position, new Vector2(204, 50)));
        bg.type = Image.Type.Sliced;
        AddText("CommandLabel_" + label, root, label, 18, Hex("#CDBE9A"), TextAnchor.MiddleLeft, RectSpec.Center(position + new Vector2(-38, 0), new Vector2(96, 30)));
        AddText("CommandValue_" + label, root, value, 18, Hex("#F0D58A"), TextAnchor.MiddleRight, RectSpec.Center(position + new Vector2(56, 0), new Vector2(50, 30)), FontStyle.Bold);
    }

    private static GameObject InstantiateComponent(string componentName, Transform parent, string instanceName, Vector2 position, Vector2 size, string labelOverride = null)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ComponentDir + "/" + componentName + ".prefab");
        GameObject instance;
        if (prefab != null)
        {
            instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        }
        else
        {
            instance = new GameObject(componentName, typeof(RectTransform));
            instance.transform.SetParent(parent, false);
        }

        instance.name = instanceName;
        SetRect(instance.transform, RectSpec.Center(position, size));
        if (!string.IsNullOrEmpty(labelOverride))
        {
            SetChildText(instance, "Label", labelOverride);
        }

        return instance;
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

    private static void SetRect(Transform transform, RectSpec spec)
    {
        var rect = transform.GetComponent<RectTransform>();
        if (rect == null)
        {
            rect = transform.gameObject.AddComponent<RectTransform>();
        }
        spec.Apply(rect);
    }

    private static void SetChildText(GameObject root, string childName, string value)
    {
        var child = FindDeep(root.transform, childName);
        if (child == null)
        {
            return;
        }

        var text = child.GetComponent<Text>();
        if (text != null)
        {
            text.text = value;
        }
    }

    private static void SetChildSprite(GameObject root, string childName, string spritePath)
    {
        SetChildSprite(root, childName, SpriteAt(spritePath));
    }

    private static void SetChildSprite(GameObject root, string childName, Sprite sprite)
    {
        var child = FindDeep(root.transform, childName);
        if (child == null)
        {
            return;
        }

        var image = child.GetComponent<Image>();
        if (image != null)
        {
            image.sprite = sprite;
        }
    }

    private static Transform FindDeep(Transform root, string name)
    {
        if (root.name == name)
        {
            return root;
        }

        foreach (Transform child in root)
        {
            var found = FindDeep(child, name);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private static Sprite SpriteAt(string relativePath)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ThemeRoot + "/" + relativePath);
        if (sprite != null)
        {
            return sprite;
        }

        sprite = AssetDatabase.LoadAssetAtPath<Sprite>(PlaceholderDir + "/PH_Fallback.png");
        return sprite;
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

    private static string CaptureTemplate(string templateName)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(TemplateDir + "/" + templateName + ".prefab");
        if (prefab == null)
        {
            GenerateAssetsAndPrefabs();
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(TemplateDir + "/" + templateName + ".prefab");
        }

        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.name = templateName + "_CaptureInstance";
        instance.hideFlags = HideFlags.HideAndDontSave;

        var cameraObject = new GameObject(templateName + "_CaptureCamera");
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
        var screenshotPath = Path.Combine(screenshotDir, ScreenshotNames[templateName]);
        File.WriteAllBytes(screenshotPath, texture.EncodeToPNG());

        camera.targetTexture = previousTarget;
        RenderTexture.active = previousActive;
        UnityEngine.Object.DestroyImmediate(texture);
        UnityEngine.Object.DestroyImmediate(rt);
        UnityEngine.Object.DestroyImmediate(instance);
        UnityEngine.Object.DestroyImmediate(cameraObject);

        Debug.Log("Captured Phase78 Steam demo screenshot: " + ScreenshotNames[templateName]);
        return screenshotPath;
    }

    private static void GeneratePlaceholderSprites()
    {
        CreateFallback(PlaceholderDir + "/PH_Fallback.png");
        CreateInkPortrait(PlaceholderDir + "/PH_CharacterPortrait_Ink.png");
        CreateBattleBackground(PlaceholderDir + "/PH_BattleBackground_Mountains.png");
        CreateCardArt(PlaceholderDir + "/PH_CardArt_Sword.png", PlaceholderKind.Sword);
        CreateCardArt(PlaceholderDir + "/PH_CardArt_Qi.png", PlaceholderKind.Qi);
        CreateCardArt(PlaceholderDir + "/PH_CardArt_Talisman.png", PlaceholderKind.Talisman);
        CreateReward(PlaceholderDir + "/PH_Reward_Jade.png", PlaceholderKind.Jade);
        CreateReward(PlaceholderDir + "/PH_Reward_Scroll.png", PlaceholderKind.Scroll);
        CreateReward(PlaceholderDir + "/PH_Reward_Qi.png", PlaceholderKind.Qi);
        CreateReward(PlaceholderDir + "/PH_Reward_Unknown.png", PlaceholderKind.Unknown);
        CreateEventWell(PlaceholderDir + "/PH_EventWell.png");
        CreateInkFigure(PlaceholderDir + "/PH_PlayerInkFigure.png", true);
        CreateInkFigure(PlaceholderDir + "/PH_EnemyInkFigure.png", false);
    }

    private static void CreateFallback(string assetPath)
    {
        var texture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
        for (var y = 0; y < texture.height; y++)
        {
            for (var x = 0; x < texture.width; x++)
            {
                texture.SetPixel(x, y, (x + y) % 12 < 6 ? Hex("#203E36") : Hex("#10231F"));
            }
        }
        SaveTexture(texture, assetPath, Vector4.zero);
    }

    private static void CreateInkPortrait(string assetPath)
    {
        var texture = NewTransparent(420, 300);
        FillGradient(texture, Hex("#253B36"), Hex("#0A1312"), 0.95f);
        DrawMist(texture, 0.12f);
        DrawCircle(texture, new Vector2(210, 98), 44, Hex("#D6C3A0"), 0.9f);
        DrawLine(texture, new Vector2(155, 160), new Vector2(265, 160), Hex("#11110F"), 28);
        DrawLine(texture, new Vector2(185, 120), new Vector2(142, 216), Hex("#101210"), 14);
        DrawLine(texture, new Vector2(234, 120), new Vector2(280, 218), Hex("#101210"), 14);
        DrawLine(texture, new Vector2(178, 126), new Vector2(110, 96), Hex("#0B0E0C"), 9);
        DrawLine(texture, new Vector2(236, 126), new Vector2(306, 96), Hex("#0B0E0C"), 9);
        DrawCircle(texture, new Vector2(210, 214), 82, Hex("#2B2D28"), 0.82f);
        DrawRect(texture, new Rect(36, 224, 348, 4), Hex("#BE9B54"), 0.65f);
        SaveTexture(texture, assetPath, new Vector4(16, 16, 16, 16));
    }

    private static void CreateBattleBackground(string assetPath)
    {
        var texture = new Texture2D(1280, 720, TextureFormat.RGBA32, false);
        for (var y = 0; y < texture.height; y++)
        {
            for (var x = 0; x < texture.width; x++)
            {
                var t = y / (float)(texture.height - 1);
                var color = Color.Lerp(Hex("#0A1112"), Hex("#30413E"), t);
                var mist = Mathf.Exp(-Mathf.Pow((y - 360f + Mathf.Sin(x * 0.01f) * 30f) / 96f, 2f)) * 0.18f;
                color = Color.Lerp(color, Hex("#C5D2C8"), mist);
                var hill1 = 160 + Mathf.Sin(x * 0.009f) * 50 + Mathf.Sin(x * 0.021f) * 24;
                var hill2 = 300 + Mathf.Sin(x * 0.006f) * 70 + Mathf.Sin(x * 0.017f) * 18;
                if (y < hill2) color = Color.Lerp(color, Hex("#172220"), 0.55f);
                if (y < hill1) color = Color.Lerp(color, Hex("#050908"), 0.72f);
                texture.SetPixel(x, y, new Color(color.r, color.g, color.b, 1f));
            }
        }
        DrawRect(texture, new Rect(0, 0, texture.width, 132), Hex("#050807"), 0.28f);
        SaveTexture(texture, assetPath, Vector4.zero);
    }

    private static void CreateCardArt(string assetPath, PlaceholderKind kind)
    {
        var texture = NewTransparent(360, 250);
        FillGradient(texture, kind == PlaceholderKind.Talisman ? Hex("#342537") : Hex("#173936"), Hex("#071211"), 1f);
        DrawMist(texture, 0.12f);
        if (kind == PlaceholderKind.Sword)
        {
            DrawLine(texture, new Vector2(98, 58), new Vector2(256, 198), Hex("#DDEBE4"), 10);
            DrawLine(texture, new Vector2(140, 72), new Vector2(260, 178), Hex("#85D8C4"), 4);
            DrawLine(texture, new Vector2(92, 74), new Vector2(130, 50), Hex("#D6B86E"), 6);
        }
        else if (kind == PlaceholderKind.Qi)
        {
            DrawArc(texture, new Vector2(180, 130), 74, 0.2f, 5.8f, Hex("#77E1C4"), 8);
            DrawArc(texture, new Vector2(184, 130), 42, -0.6f, 4.8f, Hex("#D7BC70"), 6);
            DrawCircle(texture, new Vector2(182, 126), 22, Hex("#C4FBEA"), 0.32f);
        }
        else
        {
            DrawRect(texture, new Rect(132, 42, 96, 164), Hex("#E7DBC0"), 0.9f);
            DrawLine(texture, new Vector2(148, 78), new Vector2(212, 78), Hex("#A54435"), 5);
            DrawLine(texture, new Vector2(180, 76), new Vector2(162, 144), Hex("#A54435"), 5);
            DrawLine(texture, new Vector2(180, 76), new Vector2(206, 152), Hex("#A54435"), 5);
        }
        SaveTexture(texture, assetPath, new Vector4(18, 18, 18, 18));
    }

    private static void CreateReward(string assetPath, PlaceholderKind kind)
    {
        var texture = NewTransparent(220, 220);
        var center = new Vector2(110, 110);
        for (var y = 0; y < texture.height; y++)
        {
            for (var x = 0; x < texture.width; x++)
            {
                var d = (new Vector2(x, y) - center).magnitude;
                var alpha = Mathf.Clamp01(1f - d / 108f);
                var color = Color.Lerp(Hex("#091311"), Hex("#1C3C34"), alpha);
                texture.SetPixel(x, y, new Color(color.r, color.g, color.b, Mathf.Clamp01(alpha * 1.4f)));
            }
        }

        if (kind == PlaceholderKind.Scroll)
        {
            DrawRect(texture, new Rect(58, 58, 104, 120), Hex("#D8C39B"), 0.92f);
            DrawLine(texture, new Vector2(78, 92), new Vector2(140, 92), Hex("#806236"), 4);
            DrawLine(texture, new Vector2(78, 124), new Vector2(132, 124), Hex("#806236"), 4);
        }
        else if (kind == PlaceholderKind.Unknown)
        {
            DrawCircle(texture, center, 46, Hex("#D6C186"), 0.76f);
            DrawLine(texture, new Vector2(104, 80), new Vector2(130, 100), Hex("#1B1A14"), 8);
            DrawLine(texture, new Vector2(130, 100), new Vector2(110, 126), Hex("#1B1A14"), 8);
            DrawCircle(texture, new Vector2(110, 154), 5, Hex("#1B1A14"), 1f);
        }
        else if (kind == PlaceholderKind.Qi)
        {
            DrawArc(texture, center, 54, 0.2f, 5.8f, Hex("#74E2C3"), 8);
            DrawCircle(texture, center, 28, Hex("#BFF9E5"), 0.22f);
        }
        else
        {
            DrawLine(texture, new Vector2(110, 46), new Vector2(156, 104), Hex("#A3E6C8"), 11);
            DrawLine(texture, new Vector2(156, 104), new Vector2(110, 176), Hex("#69C8A8"), 11);
            DrawLine(texture, new Vector2(110, 176), new Vector2(64, 104), Hex("#3EA183"), 11);
            DrawLine(texture, new Vector2(64, 104), new Vector2(110, 46), Hex("#D5FFE8"), 11);
        }
        SaveTexture(texture, assetPath, Vector4.zero);
    }

    private static void CreateEventWell(string assetPath)
    {
        var texture = NewTransparent(640, 320);
        FillGradient(texture, Hex("#203A35"), Hex("#08100F"), 1f);
        DrawMist(texture, 0.14f);
        DrawRect(texture, new Rect(0, 0, texture.width, 66), Hex("#050807"), 0.42f);
        DrawArc(texture, new Vector2(320, 132), 96, 0f, Mathf.PI * 2f, Hex("#6E695B"), 9);
        DrawArc(texture, new Vector2(320, 140), 64, 0f, Mathf.PI * 2f, Hex("#1A1D1A"), 17);
        DrawRect(texture, new Rect(240, 70, 160, 70), Hex("#34362E"), 0.85f);
        DrawCircle(texture, new Vector2(320, 156), 36, Hex("#86E8C8"), 0.22f);
        DrawLine(texture, new Vector2(220, 222), new Vector2(420, 222), Hex("#BFA45F"), 3);
        SaveTexture(texture, assetPath, new Vector4(18, 18, 18, 18));
    }

    private static void CreateInkFigure(string assetPath, bool player)
    {
        var texture = NewTransparent(320, 380);
        var ink = player ? Hex("#0B0E0C") : Hex("#141412");
        var cloth = player ? Hex("#3D3E37") : Hex("#262C2A");
        DrawCircle(texture, new Vector2(160, 96), 38, Hex("#D6C3A0"), 0.9f);
        DrawLine(texture, new Vector2(128, 132), new Vector2(192, 132), ink, 22);
        DrawCircle(texture, new Vector2(160, 218), 86, cloth, 0.92f);
        DrawLine(texture, new Vector2(126, 138), new Vector2(78, 296), ink, 16);
        DrawLine(texture, new Vector2(194, 138), new Vector2(238, 294), ink, 16);
        DrawLine(texture, new Vector2(110, 238), new Vector2(64, 342), ink, 14);
        DrawLine(texture, new Vector2(204, 238), new Vector2(260, 342), ink, 14);
        if (player)
        {
            DrawLine(texture, new Vector2(62, 120), new Vector2(252, 282), Hex("#C8DCD4"), 7);
        }
        else
        {
            DrawLine(texture, new Vector2(54, 270), new Vector2(264, 214), Hex("#8D8A7A"), 8);
        }
        SaveTexture(texture, assetPath, Vector4.zero);
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
                var noise = Hash01(x, y) * 0.04f;
                color = Color.Lerp(color, Color.white, noise);
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
                var wave = Mathf.Sin(x * 0.02f + y * 0.006f) * 0.5f + 0.5f;
                var band = Mathf.Exp(-Mathf.Pow((y - texture.height * 0.55f + Mathf.Sin(x * 0.018f) * 18f) / (texture.height * 0.18f), 2f));
                BlendPixel(texture, x, y, Hex("#C8D8C8"), wave * band * alpha);
            }
        }
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

    private static void EnsureDirectories()
    {
        Directory.CreateDirectory(ToAbsolute(TemplateDir));
        Directory.CreateDirectory(ToAbsolute(ComponentDir));
        Directory.CreateDirectory(ToAbsolute(PlaceholderDir));
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

                var normalizedStart = start < 0 ? start + Mathf.PI * 2f : start;
                var normalizedEnd = end < 0 ? end + Mathf.PI * 2f : end;
                var inRange = normalizedStart <= normalizedEnd
                    ? angle >= normalizedStart && angle <= normalizedEnd
                    : angle >= normalizedStart || angle <= normalizedEnd;

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

    private enum PlaceholderKind
    {
        Sword,
        Qi,
        Talisman,
        Jade,
        Scroll,
        Unknown
    }

    private enum CardDisplay
    {
        CompactWide,
        Compact,
        Hand,
        Detail
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

        public static RectSpec Right(Vector2 position, Vector2 size)
        {
            return new RectSpec(new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), position, size, Vector2.zero, Vector2.zero, false);
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
