using GameLogic.Cultivation;
using NUnit.Framework;
using UnityEngine;

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
            Assert.NotNull(_root.transform.Find("CultivationRunPrototypeUI/Lower/ActionBar/EndTurnButton"));
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
    }
}
