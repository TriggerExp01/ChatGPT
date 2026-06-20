using GameLogic.Cultivation;
using NUnit.Framework;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic.Tests
{
    public sealed class CultivationRunPrototypeUITests
    {
        private GameObject _root;
        private CultivationRunPrototypeUI _ui;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("RunPrototypeUITestRoot", typeof(RectTransform));
            _ui = CultivationRunPrototypeUI.Open(_root.transform);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_root);
        }

        [Test]
        public void OpenCreatesInteractiveRunPrototype()
        {
            var snapshot = _ui.Snapshot;

            Assert.AreEqual(CultivationRunStatus.InBattle, snapshot.Status);
            Assert.AreEqual("山门石魔", snapshot.CurrentNodeName);
            Assert.AreEqual(5, snapshot.HandCount);
            Assert.GreaterOrEqual(snapshot.DeckCount, 5);
        }

        [Test]
        public void OpenBuildsCompleteRunUiLayout()
        {
            Assert.NotNull(_root.transform.Find("CultivationRunPrototypeUI/Header/TitleRow/TitleBox/Title"));
            Assert.NotNull(_root.transform.Find("CultivationRunPrototypeUI/Header/TitleRow/TitleBox/StageText"));
            Assert.NotNull(_root.transform.Find("CultivationRunPrototypeUI/Header/TitleRow/StatusText"));
            Assert.NotNull(_root.transform.Find("CultivationRunPrototypeUI/Header/StatsBar"));
            Assert.NotNull(_root.transform.Find("CultivationRunPrototypeUI/Header/MapBar"));
            Assert.NotNull(_root.transform.Find("CultivationRunPrototypeUI/Body/RunPanel/RunText"));
            Assert.NotNull(_root.transform.Find("CultivationRunPrototypeUI/Body/BattlePanel/BattleText"));
            Assert.NotNull(_root.transform.Find("CultivationRunPrototypeUI/Body/DeckPanel/DeckText"));
            Assert.NotNull(_root.transform.Find("CultivationRunPrototypeUI/Lower/ChoicesScrollPanel/Viewport/Content"));
            Assert.NotNull(_root.transform.Find("CultivationRunPrototypeUI/Lower/HandScrollPanel/Viewport/Content"));
            Assert.NotNull(_root.transform.Find("CultivationRunPrototypeUI/Lower/ActionBar/ResetButton"));
            Assert.NotNull(_root.transform.Find("CultivationRunPrototypeUI/Lower/ActionBar/SwordSectButton"));
            Assert.NotNull(_root.transform.Find("CultivationRunPrototypeUI/Lower/ActionBar/FireCloudSectButton"));
            Assert.NotNull(_root.transform.Find("CultivationRunPrototypeUI/Lower/ActionBar/ThunderSectButton"));
            Assert.NotNull(_root.transform.Find("CultivationRunPrototypeUI/Lower/ActionBar/EndTurnButton"));
        }

        [Test]
        public void SectButtonsRestartRunWithSelectedSect()
        {
            var fireCloudButton = _root.transform
                .Find("CultivationRunPrototypeUI/Lower/ActionBar/FireCloudSectButton")
                .GetComponent<Button>();
            var swordButton = _root.transform
                .Find("CultivationRunPrototypeUI/Lower/ActionBar/SwordSectButton")
                .GetComponent<Button>();
            var thunderButton = _root.transform
                .Find("CultivationRunPrototypeUI/Lower/ActionBar/ThunderSectButton")
                .GetComponent<Button>();

            fireCloudButton.onClick.Invoke();

            Assert.AreEqual(CultivationSect.FireCloud, _ui.Snapshot.Sect);
            StringAssert.Contains("火云宗", _root.transform.Find("CultivationRunPrototypeUI/Header/TitleRow/TitleBox/StageText").GetComponent<Text>().text);
            StringAssert.Contains(CultivationSeedData.BurningPalm.Name, _root.transform.Find("CultivationRunPrototypeUI/Body/DeckPanel/DeckText").GetComponent<Text>().text);

            thunderButton.onClick.Invoke();

            Assert.AreEqual(CultivationSect.Thunder, _ui.Snapshot.Sect);
            StringAssert.Contains("天雷阁", _root.transform.Find("CultivationRunPrototypeUI/Header/TitleRow/TitleBox/StageText").GetComponent<Text>().text);
            StringAssert.Contains(CultivationSeedData.ThunderTalisman.Name, _root.transform.Find("CultivationRunPrototypeUI/Body/DeckPanel/DeckText").GetComponent<Text>().text);

            swordButton.onClick.Invoke();

            Assert.AreEqual(CultivationSect.Sword, _ui.Snapshot.Sect);
            StringAssert.Contains(CultivationSeedData.SwordQi.Name, _root.transform.Find("CultivationRunPrototypeUI/Body/DeckPanel/DeckText").GetComponent<Text>().text);
        }

        [Test]
        public void MainRunUiServiceReusesAndClosesPrototypeUi()
        {
            var first = CultivationRunUIService.OpenMainRunUI(_root.transform);
            var second = CultivationRunUIService.OpenMainRunUI(_root.transform);

            Assert.AreSame(first, second);
            Assert.IsTrue(CultivationRunUIService.IsMainRunUIOpen(_root.transform));
            Assert.NotNull(_root.transform.Find(CultivationRunPrototypeUI.RootName));

            Assert.IsTrue(CultivationRunUIService.CloseMainRunUI(_root.transform));
            Assert.IsFalse(CultivationRunUIService.IsMainRunUIOpen(_root.transform));
            Assert.IsNull(_root.transform.Find(CultivationRunPrototypeUI.RootName));
        }

        [Test]
        public void RunWindowMetadataUsesGeneratedMainRunPanel()
        {
            var attribute = typeof(CultivationRunWindow).GetCustomAttribute<WindowAttribute>();

            Assert.NotNull(attribute);
            Assert.AreEqual((int)UILayer.UI, attribute.WindowLayer);
            Assert.IsTrue(attribute.FromResources);
            Assert.AreEqual(CultivationRunWindow.ResourceLocation, attribute.Location);
            Assert.IsTrue(attribute.FullScreen);
        }

        [Test]
        public void RunWindowPrefabAssetCanLoadFromResources()
        {
            var prefab = Resources.Load<GameObject>(CultivationRunWindow.ResourceLocation);

            Assert.NotNull(prefab);
            Assert.AreEqual(CultivationRunWindow.AssetLocation, prefab.name);
            Assert.NotNull(prefab.GetComponent<Canvas>());
            Assert.NotNull(prefab.GetComponent<GraphicRaycaster>());
            Assert.NotNull(prefab.transform.Find(CultivationRunPrototypeUI.RootName));
            Assert.NotNull(prefab.transform.Find($"{CultivationRunPrototypeUI.RootName}/Header"));
            Assert.NotNull(prefab.transform.Find($"{CultivationRunPrototypeUI.RootName}/Header/TitleRow/TitleBox/Title"));
            Assert.NotNull(prefab.transform.Find($"{CultivationRunPrototypeUI.RootName}/Header/StatsBar/资源Label"));
            Assert.NotNull(prefab.transform.Find($"{CultivationRunPrototypeUI.RootName}/Header/MapBar/路线图Label"));
            Assert.NotNull(prefab.transform.Find($"{CultivationRunPrototypeUI.RootName}/Body"));
            Assert.NotNull(prefab.transform.Find($"{CultivationRunPrototypeUI.RootName}/Body/RunPanel/RunText"));
            Assert.NotNull(prefab.transform.Find($"{CultivationRunPrototypeUI.RootName}/Body/BattlePanel/战斗详情Label"));
            Assert.NotNull(prefab.transform.Find($"{CultivationRunPrototypeUI.RootName}/Body/BattlePanel/BattleText"));
            Assert.NotNull(prefab.transform.Find($"{CultivationRunPrototypeUI.RootName}/Body/DeckPanel/DeckText"));
            Assert.NotNull(prefab.transform.Find($"{CultivationRunPrototypeUI.RootName}/Body/DeckPanel/LogText"));
            Assert.NotNull(prefab.transform.Find($"{CultivationRunPrototypeUI.RootName}/Lower"));
            Assert.NotNull(prefab.transform.Find($"{CultivationRunPrototypeUI.RootName}/Lower/ChoicesScrollPanel/Viewport/Content"));
            Assert.NotNull(prefab.transform.Find($"{CultivationRunPrototypeUI.RootName}/Lower/HandScrollPanel/Viewport/Content"));
            Assert.NotNull(prefab.transform.Find($"{CultivationRunPrototypeUI.RootName}/Lower/ActionBar"));
            Assert.NotNull(prefab.transform.Find($"{CultivationRunPrototypeUI.RootName}/Lower/ActionBar/ResetButton/Label"));
            Assert.NotNull(prefab.transform.Find($"{CultivationRunPrototypeUI.RootName}/Lower/ActionBar/EndTurnButton/Label"));
            Assert.NotNull(prefab.transform.Find("PrefabAnchors"));
            Assert.NotNull(prefab.transform.Find("PrefabAnchors/UIItemTemplates/InfoCardTemplate"));
            Assert.NotNull(prefab.transform.Find("PrefabAnchors/UIItemTemplates/ActionButtonTemplate"));
            Assert.NotNull(prefab.transform.Find("PrefabAnchors/UIItemTemplates/MessageTextTemplate"));
            Assert.IsFalse(prefab.transform.Find("PrefabAnchors/UIItemTemplates/InfoCardTemplate").gameObject.activeSelf);
            Assert.IsFalse(prefab.transform.Find("PrefabAnchors/UIItemTemplates/ActionButtonTemplate").gameObject.activeSelf);
            Assert.IsFalse(prefab.transform.Find("PrefabAnchors/UIItemTemplates/MessageTextTemplate").gameObject.activeSelf);
        }

        [Test]
        public void PrototypeUiReusesPrefabAnchors()
        {
            var prefab = Resources.Load<GameObject>(CultivationRunWindow.ResourceLocation);
            var instance = Object.Instantiate(prefab, _root.transform);
            var shell = instance.transform.Find(CultivationRunPrototypeUI.RootName);
            var headerAnchor = shell.Find("Header");
            var bodyAnchor = shell.Find("Body");
            var lowerAnchor = shell.Find("Lower");
            var titleAnchor = shell.Find("Header/TitleRow/TitleBox/Title");
            var choicesContentAnchor = shell.Find("Lower/ChoicesScrollPanel/Viewport/Content");
            var handContentAnchor = shell.Find("Lower/HandScrollPanel/Viewport/Content");
            var resetButtonAnchor = shell.Find("Lower/ActionBar/ResetButton");

            var ui = CultivationRunPrototypeUI.Open(instance.transform);

            Assert.NotNull(ui);
            Assert.AreSame(shell.gameObject, ui.gameObject);
            Assert.AreSame(headerAnchor.gameObject, ui.transform.Find("Header").gameObject);
            Assert.AreSame(bodyAnchor.gameObject, ui.transform.Find("Body").gameObject);
            Assert.AreSame(lowerAnchor.gameObject, ui.transform.Find("Lower").gameObject);
            Assert.AreSame(titleAnchor.gameObject, ui.transform.Find("Header/TitleRow/TitleBox/Title").gameObject);
            Assert.AreSame(choicesContentAnchor.gameObject, ui.transform.Find("Lower/ChoicesScrollPanel/Viewport/Content").gameObject);
            Assert.AreSame(handContentAnchor.gameObject, ui.transform.Find("Lower/HandScrollPanel/Viewport/Content").gameObject);
            Assert.AreSame(resetButtonAnchor.gameObject, ui.transform.Find("Lower/ActionBar/ResetButton").gameObject);
            Assert.NotNull(ui.transform.Find("Header/TitleRow/TitleBox/Title"));
            Assert.NotNull(ui.transform.Find("Lower/ActionBar/ResetButton"));
        }

        [Test]
        public void PrototypeUiClonesDynamicItemsFromPrefabTemplates()
        {
            var prefab = Resources.Load<GameObject>(CultivationRunWindow.ResourceLocation);
            var instance = Object.Instantiate(prefab, _root.transform);

            var ui = CultivationRunPrototypeUI.Open(instance.transform);

            Assert.NotNull(ui);
            Assert.NotNull(ui.transform.Find("Header/StatsBar/Stat_境界").GetComponent<CanvasGroup>());
            Assert.NotNull(ui.transform.Find("Header/MapBar/MapNode_0_node_stone_demon").GetComponent<CanvasGroup>());
            var handContent = ui.transform.Find("Lower/HandScrollPanel/Viewport/Content");
            var firstCard = FindFirstChildWithPrefix(handContent, "Card_");
            Assert.NotNull(firstCard);
            Assert.NotNull(firstCard.GetComponent<CanvasGroup>());
            Assert.NotNull(firstCard.Find("Label"));
            Assert.NotNull(firstCard.Find("Detail"));
        }

        [Test]
        public void PrototypeUiCreatesSemanticDynamicTemplatesWhenPrefabHasGenericTemplates()
        {
            var prefab = Resources.Load<GameObject>(CultivationRunWindow.ResourceLocation);
            var instance = Object.Instantiate(prefab, _root.transform);

            var ui = CultivationRunPrototypeUI.Open(instance.transform);
            var templateRoot = instance.transform.Find("PrefabAnchors/UIItemTemplates");

            Assert.NotNull(ui);
            Assert.NotNull(templateRoot.Find("StatCardTemplate"));
            Assert.NotNull(templateRoot.Find("MapNodeTemplate"));
            Assert.NotNull(templateRoot.Find("RewardCardTemplate"));
            Assert.NotNull(templateRoot.Find("RouteChoiceTemplate"));
            Assert.NotNull(templateRoot.Find("RestChoiceTemplate"));
            Assert.NotNull(templateRoot.Find("MarketItemTemplate"));
            Assert.NotNull(templateRoot.Find("ChestRewardTemplate"));
            Assert.NotNull(templateRoot.Find("MysticEventTemplate"));
            Assert.NotNull(templateRoot.Find("GoldenCorePassiveTemplate"));
            Assert.NotNull(templateRoot.Find("BattleCardTemplate"));
            Assert.NotNull(templateRoot.Find("PillItemTemplate"));
            Assert.NotNull(templateRoot.Find("MarketDeckActionTemplate"));
            Assert.IsFalse(templateRoot.Find("BattleCardTemplate").gameObject.activeSelf);
            Assert.IsFalse(templateRoot.Find("RouteChoiceTemplate").gameObject.activeSelf);
        }

        [Test]
        public void RuntimeDynamicItemsUseSemanticTemplateStyles()
        {
            var firstCard = FindFirstChildWithPrefix(_root.transform.Find("CultivationRunPrototypeUI/Lower/HandScrollPanel/Viewport/Content"), "Card_");
            Assert.NotNull(firstCard);
            AssertColorApproximately(new Color(0.155f, 0.095f, 0.075f, 1f), firstCard.GetComponent<Image>().color);

            ForceCurrentBattleVictory();
            _ui.ResolveBattle();
            var reward = FindFirstChildWithPrefix(_root.transform.Find("CultivationRunPrototypeUI/Lower/ChoicesScrollPanel/Viewport/Content"), "Reward_");
            Assert.NotNull(reward);
            AssertColorApproximately(new Color(0.190f, 0.145f, 0.088f, 1f), reward.GetComponent<Image>().color);

            _ui.SkipReward();
            var route = FindFirstChildWithPrefix(_root.transform.Find("CultivationRunPrototypeUI/Lower/ChoicesScrollPanel/Viewport/Content"), "Route_");
            Assert.NotNull(route);
            AssertColorApproximately(new Color(0.118f, 0.205f, 0.185f, 1f), route.GetComponent<Image>().color);
        }

        [Test]
        public void RunWindowPrefabCarriesCultivationVisualTheme()
        {
            var prefab = Resources.Load<GameObject>(CultivationRunWindow.ResourceLocation);

            var shell = prefab.transform.Find(CultivationRunPrototypeUI.RootName);
            var header = shell.Find("Header");
            var battlePanel = shell.Find("Body/BattlePanel");
            var actionTemplate = prefab.transform.Find("PrefabAnchors/UIItemTemplates/ActionButtonTemplate");
            var infoTemplate = prefab.transform.Find("PrefabAnchors/UIItemTemplates/InfoCardTemplate");
            var title = shell.Find("Header/TitleRow/TitleBox/Title").GetComponent<Text>();
            var runText = shell.Find("Body/RunPanel/RunText").GetComponent<Text>();

            AssertColorApproximately(new Color(0.026f, 0.032f, 0.030f, 0.99f), shell.GetComponent<Image>().color);
            AssertColorApproximately(new Color(0.070f, 0.088f, 0.082f, 0.98f), header.GetComponent<Image>().color);
            AssertColorApproximately(new Color(0.115f, 0.070f, 0.055f, 0.97f), battlePanel.GetComponent<Image>().color);
            AssertColorApproximately(new Color(0.175f, 0.145f, 0.090f, 1f), actionTemplate.GetComponent<Image>().color);
            AssertColorApproximately(new Color(0.100f, 0.128f, 0.122f, 1f), infoTemplate.GetComponent<Image>().color);
            AssertColorApproximately(new Color(0.96f, 0.80f, 0.46f, 1f), title.color);
            Assert.NotNull(header.GetComponent<CanvasGroup>());
            Assert.NotNull(header.GetComponent<Outline>());
            Assert.NotNull(header.GetComponent<Shadow>());
            Assert.NotNull(title.GetComponent<Shadow>());
            Assert.NotNull(actionTemplate.GetComponent<Outline>());
            Assert.NotNull(actionTemplate.GetComponent<Shadow>());
            Assert.IsTrue(runText.resizeTextForBestFit);
            Assert.AreEqual(VerticalWrapMode.Truncate, runText.verticalOverflow);
        }

        [Test]
        public void RuntimeGeneratedUiAppliesCultivationVisualTheme()
        {
            var shell = _root.transform.Find(CultivationRunPrototypeUI.RootName);
            var header = shell.Find("Header");
            var battlePanel = shell.Find("Body/BattlePanel");
            var endTurnButton = shell.Find("Lower/ActionBar/EndTurnButton");
            var firstStat = FindFirstChildWithPrefix(shell.Find("Header/StatsBar"), "Stat_");
            var title = shell.Find("Header/TitleRow/TitleBox/Title").GetComponent<Text>();
            var runText = shell.Find("Body/RunPanel/RunText").GetComponent<Text>();
            var battleText = shell.Find("Body/BattlePanel/BattleText").GetComponent<Text>();

            AssertColorApproximately(new Color(0.026f, 0.032f, 0.030f, 0.99f), shell.GetComponent<Image>().color);
            AssertColorApproximately(new Color(0.070f, 0.088f, 0.082f, 0.98f), header.GetComponent<Image>().color);
            AssertColorApproximately(new Color(0.115f, 0.070f, 0.055f, 0.97f), battlePanel.GetComponent<Image>().color);
            AssertColorApproximately(new Color(0.175f, 0.145f, 0.090f, 1f), endTurnButton.GetComponent<Image>().color);
            AssertColorApproximately(new Color(0.100f, 0.128f, 0.122f, 1f), firstStat.GetComponent<Image>().color);
            AssertColorApproximately(new Color(0.96f, 0.80f, 0.46f, 1f), title.color);
            Assert.NotNull(header.GetComponent<Outline>());
            Assert.NotNull(battlePanel.GetComponent<Shadow>());
            Assert.NotNull(endTurnButton.GetComponent<CanvasGroup>());
            Assert.NotNull(firstStat.GetComponent<Outline>());
            Assert.NotNull(title.GetComponent<Shadow>());
            Assert.IsTrue(runText.resizeTextForBestFit);
            Assert.AreEqual(VerticalWrapMode.Truncate, runText.verticalOverflow);
            Assert.IsTrue(battleText.resizeTextForBestFit);
            Assert.AreEqual(VerticalWrapMode.Truncate, battleText.verticalOverflow);
        }

        [Test]
        public void RunWindowResourceLoaderCreatesWindowCompatiblePanel()
        {
            var loader = new CultivationRunWindowResourceLoader(null);
            var panel = loader.LoadGameObject(CultivationRunWindow.AssetLocation, _root.transform);

            Assert.AreEqual(CultivationRunWindow.AssetLocation, panel.name);
            Assert.AreSame(_root.transform, panel.transform.parent);
            Assert.NotNull(panel.GetComponent<Canvas>());
            Assert.NotNull(panel.GetComponent<GraphicRaycaster>());

            var ui = CultivationRunPrototypeUI.Open(panel.transform);

            Assert.NotNull(ui);
            Assert.AreEqual(CultivationRunStatus.InBattle, ui.Snapshot.Status);
            Assert.NotNull(panel.transform.Find(CultivationRunPrototypeUI.RootName));
        }

        [Test]
        public void RunWindowEntryFallsBackOutsidePlayMode()
        {
            var ui = CultivationRunUIService.OpenMainRunWindowOrFallback();

            Assert.NotNull(ui);
            Assert.AreEqual(CultivationRunStatus.InBattle, ui.Snapshot.Status);
            Assert.IsFalse(UIModule.IsValid);

            CultivationRunUIService.CloseMainRunUI();
        }

        [Test]
        public void OpenBuildsResourceAndMapBarsWithReadableStatus()
        {
            var statusText = _root.transform.Find("CultivationRunPrototypeUI/Header/TitleRow/StatusText").GetComponent<UnityEngine.UI.Text>();
            var stageText = _root.transform.Find("CultivationRunPrototypeUI/Header/TitleRow/TitleBox/StageText").GetComponent<UnityEngine.UI.Text>();
            var statsBar = _root.transform.Find("CultivationRunPrototypeUI/Header/StatsBar");
            var mapBar = _root.transform.Find("CultivationRunPrototypeUI/Header/MapBar");

            StringAssert.Contains("战斗中", statusText.text);
            StringAssert.Contains("炼气", stageText.text);
            Assert.NotNull(statsBar.Find("Stat_境界"));
            Assert.NotNull(statsBar.Find("Stat_HP"));
            Assert.NotNull(statsBar.Find("Stat_灵石"));
            Assert.NotNull(mapBar.Find("MapNode_0_node_stone_demon"));
            Assert.NotNull(mapBar.Find("MapNode_12_node_golden_core_demonic_cultivator"));
        }

        [Test]
        public void ResolveRewardAndRouteChoiceCanReachRestNode()
        {
            ForceCurrentBattleVictory();

            _ui.ResolveBattle();
            Assert.AreEqual(CultivationRunStatus.Reward, _ui.Snapshot.Status);
            Assert.AreEqual(3, _ui.Snapshot.RewardCount);

            _ui.SkipReward();
            Assert.AreEqual(CultivationRunStatus.RouteChoice, _ui.Snapshot.Status);
            Assert.AreEqual(2, _ui.Snapshot.RouteChoiceCount);

            _ui.ChooseRoute(1);
            Assert.AreEqual(CultivationRunStatus.Rest, _ui.Snapshot.Status);
            Assert.AreEqual("闭关调息", _ui.Snapshot.CurrentNodeName);
            Assert.Greater(_ui.Snapshot.RestUpgradeChoiceCount, 0);
        }

        [Test]
        public void RestUpgradeFromPrototypeUiAdvancesToLeaderBattle()
        {
            ForceCurrentBattleVictory();
            _ui.ResolveBattle();
            _ui.SkipReward();
            _ui.ChooseRoute(1);

            _ui.RestAndUpgrade(0, 0);
            Assert.AreEqual(CultivationRunStatus.RouteChoice, _ui.Snapshot.Status);

            _ui.ChooseRoute(3);

            Assert.AreEqual(CultivationRunStatus.InBattle, _ui.Snapshot.Status);
            Assert.AreEqual("石魔首领", _ui.Snapshot.CurrentNodeName);
            Assert.AreEqual(5, _ui.Snapshot.HandCount);
        }

        [Test]
        public void PrototypeUiCanEnterMarketAndBuyCard()
        {
            ForceCurrentBattleVictory();
            _ui.ResolveBattle();
            _ui.SkipReward();
            _ui.ChooseRoute(1);

            Assert.AreEqual(CultivationRunStatus.Rest, _ui.Snapshot.Status);

            _ui.Rest();
            Assert.AreEqual(CultivationRunStatus.RouteChoice, _ui.Snapshot.Status);

            _ui.ChooseRoute(1);
            Assert.AreEqual(CultivationRunStatus.Market, _ui.Snapshot.Status);
            Assert.AreEqual("山脚坊市", _ui.Snapshot.CurrentNodeName);
            Assert.Greater(_ui.Snapshot.MarketItemCount, 0);
            SetRunSpiritStones(30);

            var stonesBefore = _ui.Snapshot.SpiritStones;
            var deckBefore = _ui.Snapshot.DeckCount;
            _ui.BuyMarketItem(0);

            Assert.Less(_ui.Snapshot.SpiritStones, stonesBefore);
            Assert.AreEqual(deckBefore + 1, _ui.Snapshot.DeckCount);
            Assert.AreEqual(1, _ui.Snapshot.PurchasedMarketItemCount);
        }

        [Test]
        public void PrototypeUiCanOpenChestAndGainArtifact()
        {
            ForceCurrentBattleVictory();
            _ui.ResolveBattle();
            _ui.SkipReward();
            _ui.ChooseRoute(1);
            _ui.Rest();
            _ui.ChooseRoute(0);

            Assert.AreEqual(CultivationRunStatus.Chest, _ui.Snapshot.Status);
            Assert.AreEqual("遗迹宝箱", _ui.Snapshot.CurrentNodeName);

            _ui.OpenChest();

            Assert.AreEqual(1, _ui.Snapshot.ArtifactCount);
            Assert.AreEqual(1, _ui.Snapshot.ChestArtifactCount);
            Assert.AreEqual(CultivationRunStatus.InBattle, _ui.Snapshot.Status);
            Assert.AreEqual("石魔首领", _ui.Snapshot.CurrentNodeName);
        }

        [Test]
        public void PrototypeUiCanResolveMysticEventAndGainPill()
        {
            ForceCurrentBattleVictory();
            _ui.ResolveBattle();
            _ui.SkipReward();
            _ui.ChooseRoute(1);
            _ui.Rest();
            _ui.ChooseRoute(2);

            Assert.AreEqual(CultivationRunStatus.Mystic, _ui.Snapshot.Status);
            Assert.AreEqual(3, _ui.Snapshot.MysticEventChoiceCount);

            _ui.ChooseMysticEventOption(1);

            Assert.AreEqual(1, _ui.Snapshot.PillCount);
            Assert.AreEqual(1, _ui.Snapshot.ResolvedMysticEventCount);
            Assert.AreEqual(CultivationRunStatus.InBattle, _ui.Snapshot.Status);
            Assert.AreEqual("node_stone_demon_leader", GetRun().CurrentNode.Id);
        }

        [Test]
        public void PrototypeUiCanBuyPillWithoutChangingDeck()
        {
            ForceCurrentBattleVictory();
            _ui.ResolveBattle();
            _ui.SkipReward();
            _ui.ChooseRoute(1);
            _ui.Rest();
            _ui.ChooseRoute(1);
            SetRunSpiritStones(20);

            var deckBefore = _ui.Snapshot.DeckCount;
            _ui.BuyMarketItem(2);

            Assert.AreEqual(deckBefore, _ui.Snapshot.DeckCount);
            Assert.AreEqual(1, _ui.Snapshot.PillCount);
            Assert.AreEqual(3, _ui.Snapshot.PillSlotLimit);
            Assert.AreEqual(1, _ui.Snapshot.PurchasedMarketPillCount);
        }

        [Test]
        public void PrototypeUiCanUsePillInNextBattle()
        {
            ForceCurrentBattleVictory();
            _ui.ResolveBattle();
            _ui.SkipReward();
            _ui.ChooseRoute(1);
            _ui.Rest();
            _ui.ChooseRoute(1);
            SetRunSpiritStones(20);
            _ui.BuyMarketItem(2);
            _ui.LeaveMarket();
            DamageCurrentBattlePlayer(18);

            _ui.UsePillInBattle(0);

            Assert.AreEqual(CultivationRunStatus.InBattle, _ui.Snapshot.Status);
            Assert.AreEqual(92, _ui.Snapshot.PlayerHp);
            Assert.AreEqual(0, _ui.Snapshot.PillCount);
            StringAssert.Contains("使用 小还丹，恢复 10 HP", GetCurrentBattleLogText());
        }

        [Test]
        public void PrototypeUiCanUseBigRestorePillInNextBattle()
        {
            ForceCurrentBattleVictory();
            _ui.ResolveBattle();
            _ui.SkipReward();
            _ui.ChooseRoute(1);
            _ui.Rest();
            _ui.ChooseRoute(1);
            SetRunSpiritStones(40);
            _ui.BuyMarketItem(3);
            _ui.LeaveMarket();
            DamageCurrentBattlePlayer(20);

            _ui.UsePillInBattle(0);

            Assert.AreEqual(95, _ui.Snapshot.PlayerHp);
            Assert.AreEqual(0, _ui.Snapshot.PillCount);
            StringAssert.Contains("使用 大还丹，恢复 15 HP", GetCurrentBattleLogText());
        }

        [Test]
        public void PrototypeUiCanUseSpiritPillInNextBattle()
        {
            ForceCurrentBattleVictory();
            _ui.ResolveBattle();
            _ui.SkipReward();
            _ui.ChooseRoute(1);
            _ui.Rest();
            _ui.ChooseRoute(1);
            SetRunSpiritStones(40);
            _ui.BuyMarketItem(4);
            _ui.LeaveMarket();
            var spiritBefore = GetCurrentBattleSpirit();

            _ui.UsePillInBattle(0);

            Assert.AreEqual(spiritBefore + 2, GetCurrentBattleSpirit());
            Assert.AreEqual(0, _ui.Snapshot.PillCount);
            StringAssert.Contains("使用 增元丹，本回合灵力 +2", GetCurrentBattleLogText());
        }

        [Test]
        public void PrototypeUiCanUseCleansePillInNextBattle()
        {
            ForceCurrentBattleVictory();
            _ui.ResolveBattle();
            _ui.SkipReward();
            _ui.ChooseRoute(1);
            _ui.Rest();
            _ui.ChooseRoute(1);
            SetRunSpiritStones(20);
            _ui.BuyMarketItem(5);
            _ui.LeaveMarket();
            DamageCurrentBattlePlayer(6);
            AddCurrentBattlePlayerNegativeStatuses();

            _ui.UsePillInBattle(0);

            Assert.AreEqual(97, _ui.Snapshot.PlayerHp);
            Assert.AreEqual(0, _ui.Snapshot.PillCount);
            Assert.AreEqual(0, GetCurrentBattlePlayerBurnStacks());
            Assert.AreEqual(0, GetCurrentBattlePlayerBurnTurns());
            Assert.AreEqual(0, GetCurrentBattlePlayerBreakDefenseStacks());
            StringAssert.Contains("使用 解毒丹，清除负面状态并恢复 3 HP", GetCurrentBattleLogText());
        }

        [Test]
        public void PrototypeUiCanUseBreakthroughPillInNextBattle()
        {
            ForceCurrentBattleVictory();
            _ui.ResolveBattle();
            _ui.SkipReward();
            _ui.ChooseRoute(1);
            _ui.Rest();
            _ui.ChooseRoute(1);
            SetRunSpiritStones(95);
            _ui.BuyMarketItem(6);
            _ui.LeaveMarket();

            _ui.UsePillInBattle(0);

            Assert.AreEqual(1, GetCurrentBattleSpiritCostReduction());
            Assert.AreEqual(0, _ui.Snapshot.PillCount);
            StringAssert.Contains("使用 破境丹，本场战斗功法灵力消耗 -1", GetCurrentBattleLogText());
        }

        [Test]
        public void PrototypeUiCanUseFoundationPillInMarket()
        {
            ForceCurrentBattleVictory();
            _ui.ResolveBattle();
            _ui.SkipReward();
            _ui.ChooseRoute(1);
            _ui.Rest();
            _ui.ChooseRoute(1);
            SetRunSpiritStones(75);
            SetRunPlayerCurrentHp(50);
            _ui.BuyMarketItem(7);

            _ui.UsePillInRun(0);

            Assert.AreEqual(110, _ui.Snapshot.PlayerMaxHp);
            Assert.AreEqual(60, _ui.Snapshot.PlayerHp);
            Assert.AreEqual(0, _ui.Snapshot.PillCount);
        }

        [Test]
        public void PrototypeUiCanBuySpiritStoneMineAndGainBonusAfterBattle()
        {
            ForceCurrentBattleVictory();
            _ui.ResolveBattle();
            _ui.SkipReward();
            _ui.ChooseRoute(1);
            _ui.Rest();
            _ui.ChooseRoute(1);
            SetRunSpiritStones(30);

            _ui.BuyMarketItem(8);

            Assert.AreEqual(1, _ui.Snapshot.ArtifactCount);
            Assert.AreEqual(1, _ui.Snapshot.PurchasedMarketArtifactCount);
            Assert.AreEqual(5, _ui.Snapshot.SpiritStones);

            _ui.LeaveMarket();
            ForceCurrentBattleVictory();
            _ui.ResolveBattle();

            Assert.AreEqual(45, _ui.Snapshot.SpiritStones);
        }

        [Test]
        public void PrototypeUiCanBuyRejuvenationJadeAndHealAfterBattle()
        {
            ForceCurrentBattleVictory();
            _ui.ResolveBattle();
            _ui.SkipReward();
            _ui.ChooseRoute(1);
            _ui.Rest();
            _ui.ChooseRoute(1);
            SetRunSpiritStones(35);

            _ui.BuyMarketItem(9);

            Assert.AreEqual(1, _ui.Snapshot.ArtifactCount);
            Assert.AreEqual(1, _ui.Snapshot.PurchasedMarketArtifactCount);
            Assert.AreEqual(5, _ui.Snapshot.SpiritStones);

            _ui.LeaveMarket();
            DamageCurrentBattlePlayer(30);
            ForceCurrentBattleVictory();
            _ui.ResolveBattle();

            Assert.AreEqual(73, _ui.Snapshot.PlayerHp);
            StringAssert.Contains("法宝恢复 3 HP", GetCurrentBattleLogText());
        }

        [Test]
        public void PrototypeUiCanRemoveDeckCardInMarket()
        {
            ForceCurrentBattleVictory();
            _ui.ResolveBattle();
            _ui.SkipReward();
            _ui.ChooseRoute(1);
            _ui.Rest();
            _ui.ChooseRoute(1);
            SetRunSpiritStones(40);

            var stonesBefore = _ui.Snapshot.SpiritStones;
            var deckBefore = _ui.Snapshot.DeckCount;
            _ui.RemoveDeckCardAtMarket(0);

            Assert.AreEqual(stonesBefore - CultivationRunEngine.MarketCardRemovalCost, _ui.Snapshot.SpiritStones);
            Assert.AreEqual(deckBefore - 1, _ui.Snapshot.DeckCount);
            Assert.AreEqual(1, _ui.Snapshot.RemovedMarketCardCount);
        }

        [Test]
        public void PrototypeUiCanUpgradeDeckCardInMarket()
        {
            ForceCurrentBattleVictory();
            _ui.ResolveBattle();
            _ui.SkipReward();
            _ui.ChooseRoute(1);
            _ui.Rest();
            _ui.ChooseRoute(1);
            SetRunSpiritStones(55);

            var stonesBefore = _ui.Snapshot.SpiritStones;
            _ui.UpgradeDeckCardAtMarket(0, 0);

            Assert.AreEqual(stonesBefore - CultivationRunEngine.MarketCardUpgradeCost, _ui.Snapshot.SpiritStones);
            Assert.AreEqual(1, _ui.Snapshot.MarketUpgradedCardCount);
        }

        [Test]
        public void PrototypeUiCanSellDeckCardInMarket()
        {
            ForceCurrentBattleVictory();
            _ui.ResolveBattle();
            _ui.SkipReward();
            _ui.ChooseRoute(1);
            _ui.Rest();
            _ui.ChooseRoute(1);

            var stonesBefore = _ui.Snapshot.SpiritStones;
            var deckBefore = _ui.Snapshot.DeckCount;
            _ui.SellDeckCardAtMarket(0);

            Assert.Greater(_ui.Snapshot.SpiritStones, stonesBefore);
            Assert.AreEqual(deckBefore - 1, _ui.Snapshot.DeckCount);
            Assert.AreEqual(1, _ui.Snapshot.SoldMarketCardCount);
        }

        [Test]
        public void PrototypeUiEliteVictoryDropsArtifact()
        {
            ForceCurrentBattleVictory();
            _ui.ResolveBattle();
            _ui.SkipReward();
            _ui.ChooseRoute(1);
            _ui.Rest();
            _ui.ChooseRoute(3);

            Assert.AreEqual(CultivationRunNodeType.Elite, GetRun().CurrentNode.Type);

            ForceCurrentBattleVictory();
            _ui.ResolveBattle();

            Assert.AreEqual(CultivationRunStatus.Reward, _ui.Snapshot.Status);
            Assert.AreEqual(1, _ui.Snapshot.ArtifactCount);
            Assert.AreEqual(1, _ui.Snapshot.DroppedArtifactCount);
            StringAssert.Contains("精英战获得法宝：", GetCurrentBattleLogText());
        }

        [Test]
        public void PrototypeUiLeaderVictoryBreaksThroughToFoundationRealm()
        {
            ForceCurrentBattleVictory();
            _ui.ResolveBattle();
            _ui.SkipReward();
            _ui.ChooseRoute(1);
            _ui.Rest();
            _ui.ChooseRoute(3);

            ForceCurrentBattleVictory();
            _ui.ResolveBattle();
            _ui.SkipReward();

            Assert.AreEqual(CultivationRunStatus.InBattle, _ui.Snapshot.Status);
            Assert.AreEqual(CultivationRealm.Foundation, _ui.Snapshot.CurrentRealm);
            Assert.AreEqual(1, _ui.Snapshot.RealmBreakthroughCount);
            Assert.AreEqual("筑基剑修", _ui.Snapshot.CurrentNodeName);
            Assert.AreEqual(110, _ui.Snapshot.PlayerMaxHp);
            Assert.AreEqual(110, _ui.Snapshot.PlayerHp);
            Assert.AreEqual(4, _ui.Snapshot.SpiritMax);
            Assert.AreEqual(6, _ui.Snapshot.HandLimit);
            Assert.AreEqual(6, _ui.Snapshot.HandCount);
        }

        [Test]
        public void PrototypeUiCanBreakThroughToGoldenCorePassiveChoice()
        {
            ReachFoundationSwordCultivator();

            ForceCurrentBattleVictory();
            _ui.ResolveBattle();
            _ui.SkipReward();

            Assert.AreEqual(CultivationRunStatus.RouteChoice, _ui.Snapshot.Status);
            Assert.AreEqual(2, _ui.Snapshot.RouteChoiceCount);

            _ui.ChooseRoute(0);
            Assert.AreEqual(CultivationRunStatus.InBattle, _ui.Snapshot.Status);
            Assert.AreEqual("冰霜蛇妖", _ui.Snapshot.CurrentNodeName);
            Assert.AreEqual(CultivationRealm.Foundation, _ui.Snapshot.CurrentRealm);

            ForceCurrentBattleVictory();
            _ui.ResolveBattle();
            _ui.SkipReward();
            Assert.AreEqual("风灵鸟", _ui.Snapshot.CurrentNodeName);

            ForceCurrentBattleVictory();
            _ui.ResolveBattle();
            _ui.SkipReward();
            Assert.AreEqual("双头冰火蟒", _ui.Snapshot.CurrentNodeName);

            ForceCurrentBattleVictory();
            _ui.ResolveBattle();
            _ui.SkipReward();

            Assert.AreEqual(CultivationRunStatus.GoldenCorePassiveChoice, _ui.Snapshot.Status);
            Assert.AreEqual(CultivationRealm.GoldenCore, _ui.Snapshot.CurrentRealm);
            Assert.AreEqual(2, _ui.Snapshot.RealmBreakthroughCount);
            Assert.AreEqual("金丹魔修", _ui.Snapshot.CurrentNodeName);
            Assert.AreEqual(120, _ui.Snapshot.PlayerMaxHp);
            Assert.AreEqual(120, _ui.Snapshot.PlayerHp);
            Assert.AreEqual(5, _ui.Snapshot.SpiritMax);
            Assert.AreEqual(7, _ui.Snapshot.HandLimit);
            Assert.AreEqual(3, _ui.Snapshot.GoldenCorePassiveChoiceCount);
            Assert.AreEqual(string.Empty, _ui.Snapshot.SelectedGoldenCorePassiveName);
            Assert.AreEqual(2, _ui.Snapshot.DroppedArtifactCount);
            Assert.NotNull(_root.transform.Find("CultivationRunPrototypeUI/Lower/ChoicesScrollPanel/Viewport/Content/GoldenCorePassive_0_golden_core_sword_heart"));
            Assert.NotNull(_root.transform.Find("CultivationRunPrototypeUI/Lower/ChoicesScrollPanel/Viewport/Content/GoldenCorePassive_1_golden_core_flowing_water"));
            Assert.NotNull(_root.transform.Find("CultivationRunPrototypeUI/Lower/ChoicesScrollPanel/Viewport/Content/GoldenCorePassive_2_golden_core_thunder_seed"));
        }

        [Test]
        public void PrototypeUiCanChooseGoldenCorePassiveAndEnterEntryBattle()
        {
            ReachGoldenCorePassiveChoice();

            _ui.ChooseGoldenCorePassive(1);

            Assert.AreEqual(CultivationRunStatus.InBattle, _ui.Snapshot.Status);
            Assert.AreEqual(CultivationRealm.GoldenCore, _ui.Snapshot.CurrentRealm);
            Assert.AreEqual("金丹魔修", _ui.Snapshot.CurrentNodeName);
            Assert.AreEqual("流水之势", _ui.Snapshot.SelectedGoldenCorePassiveName);
            Assert.AreEqual(0, _ui.Snapshot.GoldenCorePassiveChoiceCount);
            Assert.AreEqual(8, GetRun().CurrentBattle.HandLimit + GetRun().CurrentBattle.ExtraDrawPerTurn);
            Assert.AreEqual(1, GetRun().CurrentBattle.ExtraDrawPerTurn);
            StringAssert.Contains("选择金丹被动：流水之势", GetCurrentBattleLogText());
        }

        [Test]
        public void PrototypeUiCanCompleteGoldenCoreEntryBattle()
        {
            ReachGoldenCoreDemonicCultivator();

            ForceCurrentBattleVictory();
            _ui.ResolveBattle();
            _ui.SkipReward();

            Assert.AreEqual(CultivationRunStatus.Completed, _ui.Snapshot.Status);
            Assert.AreEqual(CultivationRealm.GoldenCore, _ui.Snapshot.CurrentRealm);
            Assert.AreEqual(2, _ui.Snapshot.RealmBreakthroughCount);
        }

        private string GetCurrentBattleLogText()
        {
            var run = GetRun();
            return string.Join("\n", run.CurrentBattle.Logs.ConvertAll(log => log.Message));
        }

        private void DamageCurrentBattlePlayer(int amount)
        {
            GetRun().CurrentBattle.Player.TakeDamage(amount);
        }

        private int GetCurrentBattleSpirit()
        {
            return GetRun().CurrentBattle.Spirit;
        }

        private void AddCurrentBattlePlayerNegativeStatuses()
        {
            var player = GetRun().CurrentBattle.Player;
            player.AddBurn(3, 2);
            player.AddBreakDefense(2);
        }

        private int GetCurrentBattlePlayerBurnStacks()
        {
            return GetRun().CurrentBattle.Player.BurnStacks;
        }

        private int GetCurrentBattlePlayerBurnTurns()
        {
            return GetRun().CurrentBattle.Player.BurnTurns;
        }

        private int GetCurrentBattlePlayerBreakDefenseStacks()
        {
            return GetRun().CurrentBattle.Player.BreakDefenseStacks;
        }

        private int GetCurrentBattleSpiritCostReduction()
        {
            return GetRun().CurrentBattle.SpiritCostReduction;
        }

        private void SetRunSpiritStones(int value)
        {
            GetRun().SpiritStones = value;
        }

        private void SetRunPlayerCurrentHp(int value)
        {
            GetRun().PlayerCurrentHp = value;
        }

        private void ForceCurrentBattleVictory()
        {
            GetRun().CurrentBattle.Outcome = BattleOutcome.Victory;
        }

        private void ReachFoundationSwordCultivator()
        {
            ForceCurrentBattleVictory();
            _ui.ResolveBattle();
            _ui.SkipReward();
            _ui.ChooseRoute(1);
            _ui.Rest();
            _ui.ChooseRoute(3);
            ForceCurrentBattleVictory();
            _ui.ResolveBattle();
            _ui.SkipReward();
        }

        private void ReachGoldenCoreDemonicCultivator()
        {
            ReachGoldenCorePassiveChoice();
            _ui.ChooseGoldenCorePassive(0);
        }

        private void ReachGoldenCorePassiveChoice()
        {
            ReachFoundationSwordCultivator();
            ForceCurrentBattleVictory();
            _ui.ResolveBattle();
            _ui.SkipReward();
            _ui.ChooseRoute(0);
            ForceCurrentBattleVictory();
            _ui.ResolveBattle();
            _ui.SkipReward();
            ForceCurrentBattleVictory();
            _ui.ResolveBattle();
            _ui.SkipReward();
            ForceCurrentBattleVictory();
            _ui.ResolveBattle();
            _ui.SkipReward();
        }

        private CultivationRunState GetRun()
        {
            var battleField = typeof(CultivationRunPrototypeUI)
                .GetField("_run", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (CultivationRunState)battleField.GetValue(_ui);
        }

        private static Transform FindFirstChildWithPrefix(Transform parent, string prefix)
        {
            for (var i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (child.name.StartsWith(prefix, System.StringComparison.Ordinal))
                {
                    return child;
                }
            }

            return null;
        }

        private static void AssertColorApproximately(Color expected, Color actual, float tolerance = 0.002f)
        {
            Assert.That(actual.r, Is.EqualTo(expected.r).Within(tolerance));
            Assert.That(actual.g, Is.EqualTo(expected.g).Within(tolerance));
            Assert.That(actual.b, Is.EqualTo(expected.b).Within(tolerance));
            Assert.That(actual.a, Is.EqualTo(expected.a).Within(tolerance));
        }
    }
}
