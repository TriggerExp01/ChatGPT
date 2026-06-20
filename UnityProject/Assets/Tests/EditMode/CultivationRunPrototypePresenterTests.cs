using GameLogic.Cultivation;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

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
            StringAssert.Contains("等待操作：路线选择", text.BattleText);
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
        public void BuildTextAndSnapshotIncludeSelectedSect()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(sect: CultivationSect.FireCloud);
            var thunderRun = engine.StartRun(sect: CultivationSect.Thunder);
            var demonicRun = engine.StartRun(sect: CultivationSect.Demonic);

            var text = CultivationRunPrototypePresenter.BuildText(run);
            var view = CultivationRunPrototypePresenter.BuildViewModel(run);
            var snapshot = CultivationRunPrototypePresenter.CreateSnapshot(run);
            var thunderText = CultivationRunPrototypePresenter.BuildText(thunderRun);
            var thunderView = CultivationRunPrototypePresenter.BuildViewModel(thunderRun);
            var thunderSnapshot = CultivationRunPrototypePresenter.CreateSnapshot(thunderRun);
            var demonicText = CultivationRunPrototypePresenter.BuildText(demonicRun);
            var demonicView = CultivationRunPrototypePresenter.BuildViewModel(demonicRun);
            var demonicSnapshot = CultivationRunPrototypePresenter.CreateSnapshot(demonicRun);

            Assert.AreEqual(CultivationSect.FireCloud, snapshot.Sect);
            StringAssert.Contains("门派：火云宗", text.RunText);
            StringAssert.Contains("火云宗", view.PhaseTitle);
            Assert.AreEqual(CultivationSect.Thunder, thunderSnapshot.Sect);
            StringAssert.Contains("门派：天雷阁", thunderText.RunText);
            StringAssert.Contains("天雷阁", thunderView.PhaseTitle);
            Assert.AreEqual(CultivationSect.Demonic, demonicSnapshot.Sect);
            StringAssert.Contains("门派：魔道", demonicText.RunText);
            StringAssert.Contains("魔道", demonicView.PhaseTitle);
        }

        [Test]
        public void BuildTextIncludesFreezeStatusForFoundationBattles()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateDefensiveDeck(), CreateFreezeRoute());

            run.CurrentBattle.Player.AddFreeze(1, 2);
            var text = CultivationRunPrototypePresenter.BuildText(run);

            StringAssert.Contains("冰冻 1/2", text.BattleText);
            StringAssert.Contains("寒冰吐息 5 + 冰冻 1", text.BattleText);
        }

        [Test]
        public void BuildTextIncludesGoldenCoreRealmSummary()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), CultivationSeedData.CreateFirstPrototypeBranchingRoute());

            ReachGoldenCorePassiveChoice(engine, run);

            var text = CultivationRunPrototypePresenter.BuildText(run);
            var snapshot = CultivationRunPrototypePresenter.CreateSnapshot(run);

            Assert.AreEqual(CultivationRunStatus.GoldenCorePassiveChoice, snapshot.Status);
            Assert.AreEqual(CultivationRealm.GoldenCore, snapshot.CurrentRealm);
            StringAssert.Contains("境界：金丹", text.RunText);
            StringAssert.Contains("金丹被动：待选择", text.RunText);
            StringAssert.Contains("境界层：金丹", text.NodeText);
            StringAssert.Contains("敌人：金丹魔修", text.NodeText);
            StringAssert.Contains("金丹被动三选一", text.NodeText);
        }

        [Test]
        public void BuildViewModelIncludesCompleteRunUiSections()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CultivationSeedData.CreateSwordSectStarterDeck(), CultivationSeedData.CreateFirstPrototypeBranchingRoute());

            var view = CultivationRunPrototypePresenter.BuildViewModel(run);

            Assert.AreEqual("仙途·天命", view.Title);
            StringAssert.Contains("炼气", view.PhaseTitle);
            StringAssert.Contains("战斗中", view.StatusSummary);
            Assert.GreaterOrEqual(view.Stats.Length, 8);
            Assert.GreaterOrEqual(view.MapNodes.Length, 10);
            Assert.IsTrue(view.MapNodes.Any(node => node.IsCurrent && node.Name == "山门石魔"));
            Assert.IsTrue(view.DeckItems.Any(card => card.Name == "剑气诀"));
            Assert.IsTrue(view.PrimaryActions.Any(action => action.Label == "出牌"));
            Assert.IsTrue(view.ContextActions.Any(action => action.Label == "手牌"));
        }

        [Test]
        public void BuildViewModelMarksRouteChoicesAndGoldenCoreNode()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), CultivationSeedData.CreateFirstPrototypeBranchingRoute());

            WinCurrentBattle(engine, run);
            engine.SkipReward(run);
            var routeChoiceView = CultivationRunPrototypePresenter.BuildViewModel(run);

            Assert.AreEqual(CultivationRunStatus.RouteChoice, run.Status);
            Assert.AreEqual(2, routeChoiceView.MapNodes.Count(node => node.IsChoice));
            Assert.IsTrue(routeChoiceView.PrimaryActions.Any(action => action.Label == "选择路线"));

            var goldenCoreChoiceRun = engine.StartRun(CreateInstantWinDeck(), CultivationSeedData.CreateFirstPrototypeBranchingRoute());
            ReachGoldenCorePassiveChoice(engine, goldenCoreChoiceRun);
            var goldenCoreView = CultivationRunPrototypePresenter.BuildViewModel(goldenCoreChoiceRun);

            StringAssert.Contains("金丹", goldenCoreView.PhaseTitle);
            StringAssert.Contains("金丹被动选择", goldenCoreView.StatusSummary);
            Assert.IsTrue(goldenCoreView.MapNodes.Any(node => node.IsCurrent && node.Name == "金丹魔修" && node.RealmName == "金丹"));
            Assert.IsTrue(goldenCoreView.Stats.Any(stat => stat.Label == "境界" && stat.Value == "金丹"));
            Assert.IsTrue(goldenCoreView.Stats.Any(stat => stat.Label == "金丹" && stat.Value == "待选择"));
            Assert.IsTrue(goldenCoreView.PrimaryActions.Any(action => action.Label == "选择金丹被动"));
        }

        [Test]
        public void BuildTextIncludesChosenGoldenCorePassiveBattleBonuses()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), CultivationSeedData.CreateFirstPrototypeBranchingRoute());

            ReachGoldenCorePassiveChoice(engine, run);
            engine.ChooseGoldenCorePassive(run, 2);
            var text = CultivationRunPrototypePresenter.BuildText(run);
            var snapshot = CultivationRunPrototypePresenter.CreateSnapshot(run);

            Assert.AreEqual(CultivationRunStatus.InBattle, snapshot.Status);
            Assert.AreEqual("雷种入体", snapshot.SelectedGoldenCorePassiveName);
            Assert.AreEqual(0, snapshot.GoldenCorePassiveChoiceCount);
            StringAssert.Contains("金丹被动：雷种入体", text.RunText);
            StringAssert.Contains("灵力消耗 -1", text.BattleText);
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
            StringAssert.Contains("法宝：0", text.RunText);
            StringAssert.Contains("移除卡牌：35 灵石", text.NodeText);
            StringAssert.Contains("升级卡牌：50 灵石", text.NodeText);
            StringAssert.Contains("出售卡牌：半价回收", text.NodeText);
            StringAssert.Contains("等待操作：坊市交易", text.BattleText);
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
            StringAssert.Contains("大还丹（丹药） / 35 灵石", text.NodeText);
            StringAssert.Contains("增元丹（丹药） / 35 灵石", text.NodeText);
            StringAssert.Contains("解毒丹（丹药） / 15 灵石", text.NodeText);
            StringAssert.Contains("破境丹（丹药） / 90 灵石", text.NodeText);
            StringAssert.Contains("筑基丹（丹药） / 70 灵石", text.NodeText);
            StringAssert.Contains("灵石矿（法宝） / 25 灵石", text.NodeText);
            StringAssert.Contains("回春玉佩（法宝） / 30 灵石", text.NodeText);
            Assert.AreEqual(1, snapshot.PillCount);
            Assert.AreEqual(3, snapshot.PillSlotLimit);
            Assert.AreEqual(1, snapshot.PurchasedMarketPillCount);
            StringAssert.Contains("丹药：1/3", updatedText.RunText);
        }

        [Test]
        public void BuildTextIncludesMarketArtifactsAndArtifactCount()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), CreateMarketRouteWithPill(), initialSpiritStones: 30);

            engine.BuyMarketItem(run, 7);
            var snapshot = CultivationRunPrototypePresenter.CreateSnapshot(run);
            var text = CultivationRunPrototypePresenter.BuildText(run);

            Assert.AreEqual(1, snapshot.ArtifactCount);
            Assert.AreEqual(1, snapshot.PurchasedMarketArtifactCount);
            StringAssert.Contains("法宝：1", text.RunText);
        }

        [Test]
        public void BuildTextIncludesDroppedArtifactCount()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var route = new[]
            {
                new CultivationRunNode(
                    "elite",
                    "elite",
                    CultivationRunNodeType.Elite,
                    new EnemyDefinition("enemy", "enemy", 1, 0, new EnemyIntent(EnemyIntentType.Attack, 1)),
                    CultivationSeedData.CreateSwordSectRewardPool(),
                    artifactRewardPool: new[] { CultivationSeedData.SpiritStoneMineArtifact }),
            };
            var run = engine.StartRun(CreateInstantWinDeck(), route);

            PlayFirstCard(run);
            engine.ResolveBattleResult(run);
            var snapshot = CultivationRunPrototypePresenter.CreateSnapshot(run);
            var text = CultivationRunPrototypePresenter.BuildText(run);

            Assert.AreEqual(1, snapshot.ArtifactCount);
            Assert.AreEqual(1, snapshot.DroppedArtifactCount);
            StringAssert.Contains("精英法宝：1", text.RunText);
        }

        [Test]
        public void BuildTextIncludesChestStateAndChestArtifactCount()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var route = new[]
            {
                new CultivationRunNode(
                    "chest",
                    "chest",
                    CultivationRunNodeType.Chest,
                    null,
                    null,
                    artifactRewardPool: new[] { CultivationSeedData.SpiritStoneMineArtifact }),
            };
            var run = engine.StartRun(CreateInstantWinDeck(), route);

            var text = CultivationRunPrototypePresenter.BuildText(run);
            var beforeSnapshot = CultivationRunPrototypePresenter.CreateSnapshot(run);
            engine.OpenChest(run);
            var afterSnapshot = CultivationRunPrototypePresenter.CreateSnapshot(run);
            var afterText = CultivationRunPrototypePresenter.BuildText(run);

            Assert.AreEqual(CultivationRunStatus.Chest, beforeSnapshot.Status);
            StringAssert.Contains("宝箱法宝池：1 件", text.NodeText);
            StringAssert.Contains("等待操作：开启宝箱", text.BattleText);
            Assert.AreEqual(1, afterSnapshot.ArtifactCount);
            Assert.AreEqual(1, afterSnapshot.ChestArtifactCount);
            StringAssert.Contains("宝箱法宝：1", afterText.RunText);
        }

        [Test]
        public void BuildTextIncludesMysticStateAndResolvedChoiceCount()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var route = new[]
            {
                new CultivationRunNode(
                    "mystic",
                    "mystic",
                    CultivationRunNodeType.Mystic,
                    null,
                    null,
                    nextNodeIndices: new[] { 1 },
                    mysticEvent: CultivationSeedData.SpiritSpringMysticEvent),
                new CultivationRunNode(
                    "after_mystic",
                    "after_mystic",
                    CultivationRunNodeType.Battle,
                    new EnemyDefinition("after_mystic_enemy", "after_mystic_enemy", 1, 0, new EnemyIntent(EnemyIntentType.Attack, 1)),
                    CultivationSeedData.CreateSwordSectRewardPool()),
            };
            var run = engine.StartRun(CreateInstantWinDeck(), route);

            var text = CultivationRunPrototypePresenter.BuildText(run);
            var beforeSnapshot = CultivationRunPrototypePresenter.CreateSnapshot(run);
            engine.ChooseMysticEventOption(run, 1);
            var afterSnapshot = CultivationRunPrototypePresenter.CreateSnapshot(run);

            Assert.AreEqual(CultivationRunStatus.Mystic, beforeSnapshot.Status);
            Assert.AreEqual(3, beforeSnapshot.MysticEventChoiceCount);
            StringAssert.Contains("Mystic event:", text.NodeText);
            StringAssert.Contains(CultivationSeedData.SpiritSpringMysticEvent.Name, text.NodeText);
            Assert.AreEqual(1, afterSnapshot.PillCount);
            Assert.AreEqual(1, afterSnapshot.ResolvedMysticEventCount);
            Assert.AreEqual(CultivationRunStatus.InBattle, afterSnapshot.Status);
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

        [Test]
        public void FormatCardSummaryDescribesChanceKeywords()
        {
            var card = new CardDefinition(
                "chance_keyword_test",
                "概率关键词测试",
                1,
                new CardEffect(CardEffectType.ChanceDamage, 10, chancePercent: 30, fallbackValue: 5),
                new CardEffect(CardEffectType.ChainOnChanceDamage, 15, chancePercent: 30, fallbackValue: 5, secondaryValue: 7),
                new CardEffect(CardEffectType.ChanceDamage, 4, repeatCount: 3, chancePercent: 10, fallbackValue: 4),
                new CardEffect(CardEffectType.ChanceDamageWithStun, 4, duration: 1, repeatCount: 3, chancePercent: 25, fallbackValue: 20),
                new CardEffect(CardEffectType.ChanceDamageWithChain, 4, repeatCount: 3, chancePercent: 25, fallbackValue: 2, secondaryValue: 30),
                new CardEffect(CardEffectType.DamageAfterCriticalTriggered, 22, fallbackValue: 10),
                new CardEffect(CardEffectType.DamageAfterCriticalTriggeredWithStun, 22, duration: 1, fallbackValue: 18),
                new CardEffect(CardEffectType.DamageAfterCriticalTriggeredChainAll, 27, fallbackValue: 10),
                new CardEffect(CardEffectType.Dodge, 1, CardTarget.Self),
                new CardEffect(CardEffectType.DodgeCounter, 8, CardTarget.Self),
                new CardEffect(CardEffectType.AttackCounter, 4, CardTarget.Self, chancePercent: 30),
                new CardEffect(CardEffectType.ChanceStun, 0, duration: 1, chancePercent: 40),
                new CardEffect(CardEffectType.ChainOnChanceStun, 10, duration: 1, chancePercent: 40, secondaryValue: 5),
                new CardEffect(CardEffectType.ChanceChainDamage, 8, chancePercent: 50, secondaryValue: 4),
                new CardEffect(CardEffectType.ChanceChainDamageWithStun, 12, duration: 1, chancePercent: 50, fallbackValue: 20, secondaryValue: 4),
                new CardEffect(CardEffectType.ChanceChainDamageRepeatTarget, 8, chancePercent: 75, secondaryValue: 4, repeatCount: 4),
                new CardEffect(CardEffectType.ChargeDamage, 2, CardTarget.Self));

            var summary = CultivationRunPrototypePresenter.FormatCardSummary(card);

            StringAssert.Contains("30% 概率造成 10 伤害，失败造成 5 伤害", summary);
            StringAssert.Contains("30% 概率造成 15 伤害，失败造成 5 伤害；命中连锁 7 伤害", summary);
            StringAssert.Contains("造成 4 伤害 × 3，每击 10% 概率暴击", summary);
            StringAssert.Contains("造成 4 伤害 × 3，每击 25% 概率暴击；暴击时 20% 概率眩晕", summary);
            StringAssert.Contains("造成 4 伤害 × 3，每击 25% 概率暴击；每击 30% 概率连锁 2 伤害", summary);
            StringAssert.Contains("造成 22 伤害；本回合已暴击时额外造成 10 伤害", summary);
            StringAssert.Contains("造成 22 伤害；本回合已暴击时额外造成 18 伤害并眩晕 1 回合", summary);
            StringAssert.Contains("造成 27 伤害；本回合已暴击时奖励连锁全体，各造成 10 伤害", summary);
            StringAssert.Contains("获得 1 次闪避", summary);
            StringAssert.Contains("闪避成功时反击 8 伤害", summary);
            StringAssert.Contains("40% 概率眩晕 1 回合", summary);
            StringAssert.Contains("造成 10 伤害，40% 概率眩晕 1 回合；成功连锁 5 伤害", summary);
            StringAssert.Contains("造成 8 伤害，50% 概率连锁 4 伤害", summary);
            StringAssert.Contains("造成 12 伤害，50% 概率连锁 4 伤害；连锁有 20% 概率眩晕", summary);
            StringAssert.Contains("造成 8 伤害，75% 概率连锁 4 伤害；可重复目标，最多 4 次", summary);
            StringAssert.Contains("下次攻击伤害 ×2", summary);
        }

        [Test]
        public void FormatCardSummaryDescribesAttackCounter()
        {
            var card = new CardDefinition(
                "attack_counter_summary_test",
                "受击反伤摘要测试",
                1,
                new CardEffect(CardEffectType.AttackCounter, 4, CardTarget.Self, chancePercent: 30));

            var summary = CultivationRunPrototypePresenter.FormatCardSummary(card);

            StringAssert.Contains("30% 概率受击反伤 4 伤害", summary);
        }

        private static void PlayFirstCard(CultivationRunState run)
        {
            var card = run.CurrentBattle.Hand[0];
            var engine = new BattleEngine(1);
            engine.PlayCard(run.CurrentBattle, card, run.CurrentBattle.Enemies[0]);
        }

        private static void WinCurrentBattle(CultivationRunEngine engine, CultivationRunState run)
        {
            var card = run.CurrentBattle.Hand.First(item => item.Id == "instant_win");
            var battleEngine = new BattleEngine(1);
            battleEngine.PlayCard(run.CurrentBattle, card, run.CurrentBattle.Enemies[0]);
            Assert.AreEqual(BattleOutcome.Victory, run.CurrentBattle.Outcome);
            engine.ResolveBattleResult(run);
        }

        private static void ReachFoundationSwordCultivator(CultivationRunEngine engine, CultivationRunState run)
        {
            WinCurrentBattle(engine, run);
            engine.SkipReward(run);
            engine.ChooseRoute(run, 1);
            engine.Rest(run);
            engine.ChooseRoute(run, 3);
            WinCurrentBattle(engine, run);
            engine.SkipReward(run);
        }

        private static void ReachGoldenCoreDemonicCultivator(CultivationRunEngine engine, CultivationRunState run)
        {
            ReachGoldenCorePassiveChoice(engine, run);
            engine.ChooseGoldenCorePassive(run, 0);
        }

        private static void ReachGoldenCorePassiveChoice(CultivationRunEngine engine, CultivationRunState run)
        {
            ReachFoundationSwordCultivator(engine, run);
            WinCurrentBattle(engine, run);
            engine.SkipReward(run);
            engine.ChooseRoute(run, 0);
            WinCurrentBattle(engine, run);
            engine.SkipReward(run);
            WinCurrentBattle(engine, run);
            engine.SkipReward(run);
            WinCurrentBattle(engine, run);
            engine.SkipReward(run);
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
                        new CultivationMarketItem("market_big_restore_pill", CultivationSeedData.BigRestorePillItem, 35),
                        new CultivationMarketItem("market_spirit_boost_pill", CultivationSeedData.SpiritBoostPillItem, 35),
                        new CultivationMarketItem("market_cleanse_pill", CultivationSeedData.CleansePillItem, 15),
                        new CultivationMarketItem("market_breakthrough_pill", CultivationSeedData.BreakthroughPillItem, 90),
                        new CultivationMarketItem("market_foundation_pill", CultivationSeedData.FoundationPillItem, 70),
                        new CultivationMarketItem("market_spirit_stone_mine", CultivationSeedData.SpiritStoneMineArtifact, 25),
                        new CultivationMarketItem("market_rejuvenation_jade", CultivationSeedData.RejuvenationJadeArtifact, 30),
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

        private static IReadOnlyList<CultivationRunNode> CreateFreezeRoute()
        {
            return new List<CultivationRunNode>
            {
                new CultivationRunNode(
                    "freeze",
                    "freeze",
                    CultivationRunNodeType.Battle,
                    CultivationSeedData.FrostSerpentDemon,
                    CultivationSeedData.CreateSwordSectRewardPool(),
                    realm: CultivationRealm.Foundation),
            };
        }

        private static IReadOnlyList<CultivationRunNode> CreateGoldenCoreRoute()
        {
            return new List<CultivationRunNode>
            {
                new CultivationRunNode(
                    "golden_core",
                    "golden_core",
                    CultivationRunNodeType.Battle,
                    CultivationSeedData.GoldenCoreDemonicCultivator,
                    CultivationSeedData.CreateSwordSectRewardPool(),
                    realm: CultivationRealm.GoldenCore),
            };
        }

        private static IReadOnlyList<CardDefinition> CreateDefensiveDeck()
        {
            var guard = new CardDefinition("guard", "guard", 0, new CardEffect(CardEffectType.Shield, 20, CardTarget.Self));
            return new[]
            {
                guard,
                guard,
                guard,
                guard,
                guard,
                guard,
            };
        }
    }
}
