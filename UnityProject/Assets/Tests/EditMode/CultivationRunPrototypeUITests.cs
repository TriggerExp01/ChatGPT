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

            _ui.ChooseRoute(1);

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

            _ui.ChooseRoute(0);
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
        public void PrototypeUiCanBuyPillWithoutChangingDeck()
        {
            ForceCurrentBattleVictory();
            _ui.ResolveBattle();
            _ui.SkipReward();
            _ui.ChooseRoute(1);
            _ui.Rest();
            _ui.ChooseRoute(0);
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
            _ui.ChooseRoute(0);
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
        public void PrototypeUiCanUseSpiritPillInNextBattle()
        {
            ForceCurrentBattleVictory();
            _ui.ResolveBattle();
            _ui.SkipReward();
            _ui.ChooseRoute(1);
            _ui.Rest();
            _ui.ChooseRoute(0);
            SetRunSpiritStones(40);
            _ui.BuyMarketItem(3);
            _ui.LeaveMarket();
            var spiritBefore = GetCurrentBattleSpirit();

            _ui.UsePillInBattle(0);

            Assert.AreEqual(spiritBefore + 2, GetCurrentBattleSpirit());
            Assert.AreEqual(0, _ui.Snapshot.PillCount);
            StringAssert.Contains("使用 增元丹，本回合灵力 +2", GetCurrentBattleLogText());
        }

        [Test]
        public void PrototypeUiCanRemoveDeckCardInMarket()
        {
            ForceCurrentBattleVictory();
            _ui.ResolveBattle();
            _ui.SkipReward();
            _ui.ChooseRoute(1);
            _ui.Rest();
            _ui.ChooseRoute(0);
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
            _ui.ChooseRoute(0);
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
            _ui.ChooseRoute(0);

            var stonesBefore = _ui.Snapshot.SpiritStones;
            var deckBefore = _ui.Snapshot.DeckCount;
            _ui.SellDeckCardAtMarket(0);

            Assert.Greater(_ui.Snapshot.SpiritStones, stonesBefore);
            Assert.AreEqual(deckBefore - 1, _ui.Snapshot.DeckCount);
            Assert.AreEqual(1, _ui.Snapshot.SoldMarketCardCount);
        }

        private string GetCurrentBattleLogText()
        {
            var battleField = typeof(CultivationRunPrototypeUI)
                .GetField("_run", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var run = (CultivationRunState)battleField.GetValue(_ui);
            return string.Join("\n", run.CurrentBattle.Logs.ConvertAll(log => log.Message));
        }

        private void DamageCurrentBattlePlayer(int amount)
        {
            var battleField = typeof(CultivationRunPrototypeUI)
                .GetField("_run", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var run = (CultivationRunState)battleField.GetValue(_ui);
            run.CurrentBattle.Player.TakeDamage(amount);
        }

        private int GetCurrentBattleSpirit()
        {
            var battleField = typeof(CultivationRunPrototypeUI)
                .GetField("_run", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var run = (CultivationRunState)battleField.GetValue(_ui);
            return run.CurrentBattle.Spirit;
        }

        private void SetRunSpiritStones(int value)
        {
            var battleField = typeof(CultivationRunPrototypeUI)
                .GetField("_run", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var run = (CultivationRunState)battleField.GetValue(_ui);
            run.SpiritStones = value;
        }

        private void ForceCurrentBattleVictory()
        {
            var battleField = typeof(CultivationRunPrototypeUI)
                .GetField("_run", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var run = (CultivationRunState)battleField.GetValue(_ui);
            run.CurrentBattle.Outcome = BattleOutcome.Victory;
        }
    }
}
