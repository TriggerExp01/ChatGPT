using GameLogic.Cultivation;
using NUnit.Framework;
using System.Collections.Generic;

namespace GameLogic.Tests
{
    public sealed class CultivationRunPrototypePresenterTests
    {
        [Test]
        public void BuildTextIncludesBranchRouteChoices()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), CultivationSeedData.CreateFirstPrototypeBranchingRoute());

            PlayFirstCard(run);
            engine.ResolveBattleResult(run);
            engine.SkipReward(run);

            var text = CultivationRunPrototypePresenter.BuildText(run);

            Assert.AreEqual(CultivationRunStatus.RouteChoice, run.Status);
            StringAssert.Contains("可选路线", text.NodeText);
            StringAssert.Contains("火蝠洞", text.NodeText);
            StringAssert.Contains("闭关调息", text.NodeText);
            StringAssert.Contains("等待操作：RouteChoice", text.BattleText);
        }

        [Test]
        public void CreateSnapshotKeepsRunStateSummary()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CultivationSeedData.CreateSwordSectStarterDeck(), CultivationSeedData.CreateFirstPrototypeBranchingRoute(), playerCurrentHp: 76, initialSpiritStones: 12);

            var snapshot = CultivationRunPrototypePresenter.CreateSnapshot(run);

            Assert.AreEqual(CultivationRunStatus.InBattle, snapshot.Status);
            Assert.AreEqual("山门石魔", snapshot.CurrentNodeName);
            Assert.AreEqual(76, snapshot.PlayerHp);
            Assert.AreEqual(100, snapshot.PlayerMaxHp);
            Assert.AreEqual(12, snapshot.SpiritStones);
            Assert.AreEqual(12, snapshot.DeckCount);
            Assert.AreEqual(5, snapshot.HandCount);
        }

        [Test]
        public void BuildTextIncludesSpiritStones()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), CultivationSeedData.CreateFirstPrototypeBranchingRoute(), initialSpiritStones: 18);

            var text = CultivationRunPrototypePresenter.BuildText(run);

            StringAssert.Contains("灵石：18", text.RunText);
        }

        [Test]
        public void BuildTextIncludesMarketItems()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), CreateMarketRoute(), initialSpiritStones: 30);

            var text = CultivationRunPrototypePresenter.BuildText(run);
            var snapshot = CultivationRunPrototypePresenter.CreateSnapshot(run);

            Assert.AreEqual(CultivationRunStatus.Market, snapshot.Status);
            Assert.AreEqual(1, snapshot.MarketItemCount);
            StringAssert.Contains("坊市商品", text.NodeText);
            StringAssert.Contains("流云护身 / 20 灵石", text.NodeText);
            StringAssert.Contains("移除卡牌：35 灵石", text.NodeText);
            StringAssert.Contains("等待操作：Market", text.BattleText);
        }

        [Test]
        public void CreateSnapshotCountsRemovedMarketCards()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), CreateMarketRoute(), initialSpiritStones: 40);

            engine.RemoveDeckCardAtMarket(run, 0);
            var snapshot = CultivationRunPrototypePresenter.CreateSnapshot(run);
            var text = CultivationRunPrototypePresenter.BuildText(run);

            Assert.AreEqual(1, snapshot.RemovedMarketCardCount);
            StringAssert.Contains("坊市删牌：1", text.RunText);
            StringAssert.Contains("已移除 1 张", text.NodeText);
        }

        [Test]
        public void FormatCardSummaryDescribesCostAndEffects()
        {
            var summary = CultivationRunPrototypePresenter.FormatCardSummary(CultivationSeedData.BreakArmor);

            StringAssert.Contains("灵力 1", summary);
            StringAssert.Contains("造成 3 伤害", summary);
            StringAssert.Contains("破防 1", summary);
        }

        [Test]
        public void FormatCardSummaryDescribesSwordKeywords()
        {
            var card = new CardDefinition(
                "keyword_test",
                "关键词测试",
                0,
                new CardEffect(CardEffectType.Damage, 6, repeatCount: 2),
                new CardEffect(CardEffectType.SwordMark, 1),
                new CardEffect(CardEffectType.Sharpness, 3, CardTarget.Self, 2),
                new CardEffect(CardEffectType.Exhaust, 1, CardTarget.Self),
                new CardEffect(CardEffectType.DamagePerSwordMark, 3));

            var summary = CultivationRunPrototypePresenter.FormatCardSummary(card);

            StringAssert.Contains("造成 6 伤害 × 2", summary);
            StringAssert.Contains("剑气印记 1", summary);
            StringAssert.Contains("锋锐 3 / 2 回合", summary);
            StringAssert.Contains("消耗", summary);
            StringAssert.Contains("每层剑气印记 +3 伤害", summary);
        }

        private static void PlayFirstCard(CultivationRunState run)
        {
            var card = run.CurrentBattle.Hand[0];
            var engine = new BattleEngine(1);
            engine.PlayCard(run.CurrentBattle, card, run.CurrentBattle.Enemies[0]);
        }

        private static CardDefinition[] CreateInstantWinDeck()
        {
            var instantWin = new CardDefinition("instant_win", "instant_win", 0, new CardEffect(CardEffectType.Damage, 999));
            return new[]
            {
                instantWin,
                instantWin,
                instantWin,
                instantWin,
                instantWin,
            };
        }

        private static IReadOnlyList<CultivationRunNode> CreateMarketRoute()
        {
            return new List<CultivationRunNode>
            {
                new CultivationRunNode(
                    "market",
                    "market",
                    CultivationRunNodeType.Market,
                    null,
                    null,
                    marketItems: new[]
                    {
                        new CultivationMarketItem("market_cloud_guard", CultivationSeedData.CloudGuard, 20),
                    }),
            };
        }
    }
}
