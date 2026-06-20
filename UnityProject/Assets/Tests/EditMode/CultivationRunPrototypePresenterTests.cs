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
            StringAssert.Contains("丹药：0/3", text.RunText);
            StringAssert.Contains("移除卡牌：35 灵石", text.NodeText);
            StringAssert.Contains("升级卡牌：50 灵石", text.NodeText);
            StringAssert.Contains("出售卡牌：半价回收", text.NodeText);
            StringAssert.Contains("等待操作：Market", text.BattleText);
        }

        [Test]
        public void BuildTextIncludesMarketPillsAndPillSlots()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), CreateMarketRouteWithPill(), initialSpiritStones: 20);

            var text = CultivationRunPrototypePresenter.BuildText(run);
            engine.BuyMarketItem(run, 1);
            var snapshot = CultivationRunPrototypePresenter.CreateSnapshot(run);
            var updatedText = CultivationRunPrototypePresenter.BuildText(run);

            StringAssert.Contains("小还丹（丹药） / 15 灵石", text.NodeText);
            StringAssert.Contains("增元丹（丹药） / 35 灵石", text.NodeText);
            StringAssert.Contains("解毒丹（丹药） / 15 灵石", text.NodeText);
            StringAssert.Contains("破境丹（丹药） / 90 灵石", text.NodeText);
            StringAssert.Contains("筑基丹（丹药） / 70 灵石", text.NodeText);
            Assert.AreEqual(1, snapshot.PillCount);
            Assert.AreEqual(3, snapshot.PillSlotLimit);
            Assert.AreEqual(1, snapshot.PurchasedMarketPillCount);
            StringAssert.Contains("丹药：1/3", updatedText.RunText);
        }

        [Test]
        public void SnapshotUsesCurrentBattleHpAfterUsingPill()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), CreateSingleBattleRoute(), playerCurrentHp: 70);
            run.Pills.Add(CultivationSeedData.SmallRestorePillItem);
            run.CurrentBattle.Player.TakeDamage(20);

            engine.UsePillInBattle(run, 0);
            var snapshot = CultivationRunPrototypePresenter.CreateSnapshot(run);

            Assert.AreEqual(60, snapshot.PlayerHp);
            Assert.AreEqual(0, snapshot.PillCount);
            Assert.AreEqual(100, snapshot.PlayerMaxHp);
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
        public void CreateSnapshotCountsMarketUpgradedCards()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CultivationSeedData.CreateSwordSectStarterDeck(), CreateMarketRoute(), initialSpiritStones: 55);

            engine.UpgradeDeckCardAtMarket(run, 0, 0);
            var snapshot = CultivationRunPrototypePresenter.CreateSnapshot(run);
            var text = CultivationRunPrototypePresenter.BuildText(run);

            Assert.AreEqual(1, snapshot.MarketUpgradedCardCount);
            StringAssert.Contains("已升级 1 张", text.NodeText);
        }

        [Test]
        public void CreateSnapshotCountsSoldMarketCards()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), CreateMarketRoute(), initialSpiritStones: 3);

            engine.SellDeckCardAtMarket(run, 0);
            var snapshot = CultivationRunPrototypePresenter.CreateSnapshot(run);
            var text = CultivationRunPrototypePresenter.BuildText(run);

            Assert.AreEqual(1, snapshot.SoldMarketCardCount);
            StringAssert.Contains("坊市售牌：1", text.RunText);
            StringAssert.Contains("已出售 1 张", text.NodeText);
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

        private static IReadOnlyList<CultivationRunNode> CreateMarketRouteWithPill()
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
                        new CultivationMarketItem("market_small_restore_pill", CultivationSeedData.SmallRestorePillItem, 15),
                        new CultivationMarketItem("market_spirit_boost_pill", CultivationSeedData.SpiritBoostPillItem, 35),
                        new CultivationMarketItem("market_cleanse_pill", CultivationSeedData.CleansePillItem, 15),
                        new CultivationMarketItem("market_breakthrough_pill", CultivationSeedData.BreakthroughPillItem, 90),
                        new CultivationMarketItem("market_foundation_pill", CultivationSeedData.FoundationPillItem, 70),
                    }),
            };
        }

        private static IReadOnlyList<CultivationRunNode> CreateSingleBattleRoute()
        {
            return new List<CultivationRunNode>
            {
                new CultivationRunNode(
                    "battle",
                    "battle",
                    CultivationRunNodeType.Battle,
                    new EnemyDefinition("enemy", "enemy", 1, 0, new EnemyIntent(EnemyIntentType.Attack, 1)),
                    CultivationSeedData.CreateSwordSectRewardPool()),
            };
        }
    }
}
