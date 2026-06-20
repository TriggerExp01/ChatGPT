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
