using System.Collections.Generic;
using System.Linq;
using GameLogic.Cultivation;
using NUnit.Framework;

namespace GameLogic.Tests
{
    public sealed class CultivationRunEngineTests
    {
        [Test]
        public void StartRunCreatesFirstBattleFromPrototypeRoute()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));

            var run = engine.StartRun();

            Assert.AreEqual(CultivationRunStatus.InBattle, run.Status);
            Assert.AreEqual(0, run.CurrentNodeIndex);
            Assert.AreEqual(4, run.Route.Count);
            Assert.AreEqual(CultivationSeedData.StoneDemon.Id, run.CurrentNode.Enemy.Id);
            Assert.NotNull(run.CurrentBattle);
            Assert.AreEqual(5, run.CurrentBattle.Hand.Count);
            Assert.AreEqual(100, run.PlayerCurrentHp);
            Assert.AreEqual(100, run.PlayerMaxHp);
            Assert.AreEqual(0, run.SpiritStones);
        }

        [Test]
        public void VictoryMovesRunToRewardState()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), CreateTestRoute(), initialSpiritStones: 5);

            PlayFirstCard(run);
            engine.ResolveBattleResult(run);

            Assert.AreEqual(CultivationRunStatus.Reward, run.Status);
            Assert.AreEqual(5, run.SpiritStones);
            Assert.AreEqual(3, run.CurrentRewards.Count);
            Assert.AreEqual(3, run.CurrentRewards.Select(reward => reward.Id).Distinct().Count());
            Assert.IsTrue(run.CurrentRewards.All(reward => run.CurrentNode.RewardPool.Contains(reward)));
        }

        [Test]
        public void RewardChoicesUseSeededRandomSelection()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1), rewardSeed: 3);
            var run = engine.StartRun(CreateInstantWinDeck(), CreateTestRoute());

            PlayFirstCard(run);
            engine.ResolveBattleResult(run);

            CollectionAssert.AreEqual(
                new[] { "reward_small_restore_pill", "reward_thrust", "reward_sevenfold_sword_qi" },
                run.CurrentRewards.Select(reward => reward.Id).ToArray());
        }

        [Test]
        public void ChoosingRewardAddsCardAndStartsNextNode()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1), rewardSeed: 3);
            var run = engine.StartRun(CreateInstantWinDeck(), CreateTestRoute());

            PlayFirstCard(run);
            engine.ResolveBattleResult(run);
            var deckCountBeforeReward = run.Deck.Count;

            engine.ChooseReward(run, 1);

            Assert.AreEqual(CultivationRunStatus.InBattle, run.Status);
            Assert.AreEqual(1, run.CurrentNodeIndex);
            Assert.AreEqual(deckCountBeforeReward + 1, run.Deck.Count);
            Assert.AreEqual("thrust", run.Deck.Last().Id);
            Assert.AreEqual(1, run.ClaimedRewards.Count);
            Assert.NotNull(run.CurrentBattle);
            Assert.AreEqual("test_enemy_2", run.CurrentNode.Enemy.Id);
        }

        [Test]
        public void SeedPrototypeBattleVictoryGrantsSpiritStones()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), CultivationSeedData.CreateFirstPrototypeRoute(), initialSpiritStones: 10);

            WinCurrentBattle(engine, run);

            Assert.AreEqual(CultivationRunStatus.Reward, run.Status);
            Assert.AreEqual(25, run.SpiritStones);
            Assert.IsTrue(run.CurrentBattle.Logs.Any(log => log.Message.Contains("获得 15 灵石")));
        }

        [Test]
        public void SeedPrototypeEliteVictoryGrantsHigherSpiritStones()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), CultivationSeedData.CreateFirstPrototypeRoute());

            WinCurrentBattle(engine, run);
            engine.SkipReward(run);
            WinCurrentBattle(engine, run);
            engine.SkipReward(run);
            engine.Rest(run);
            WinCurrentBattle(engine, run);

            Assert.AreEqual(CultivationRunStatus.Reward, run.Status);
            Assert.AreEqual(65, run.SpiritStones);
            Assert.IsTrue(run.CurrentBattle.Logs.Any(log => log.Message.Contains("获得 35 灵石")));
        }

        [Test]
        public void SeedPrototypeEliteVictoryDropsArtifact()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), CultivationSeedData.CreateFirstPrototypeRoute());

            WinCurrentBattle(engine, run);
            engine.SkipReward(run);
            WinCurrentBattle(engine, run);
            engine.SkipReward(run);
            engine.Rest(run);
            WinCurrentBattle(engine, run);

            Assert.AreEqual(CultivationRunStatus.Reward, run.Status);
            Assert.AreEqual(1, run.Artifacts.Count);
            Assert.AreEqual(1, run.DroppedArtifacts.Count);
            CollectionAssert.Contains(
                CultivationSeedData.CreatePrototypeArtifactRewardPool().Select(artifact => artifact.Id).ToArray(),
                run.Artifacts[0].Id);
            Assert.IsTrue(run.CurrentBattle.Logs.Any(log => log.Message.Contains("精英战获得法宝：")));
        }

        [Test]
        public void NormalBattleVictoryDoesNotDropArtifact()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), CultivationSeedData.CreateFirstPrototypeRoute());

            WinCurrentBattle(engine, run);

            Assert.AreEqual(0, run.Artifacts.Count);
            Assert.AreEqual(0, run.DroppedArtifacts.Count);
            Assert.IsFalse(run.CurrentBattle.Logs.Any(log => log.Message.Contains("精英战获得法宝：")));
        }

        [Test]
        public void VictoryRemovesExhaustedCardsFromRunDeck()
        {
            var exhaustWin = new CardDefinition(
                "exhaust_win",
                "exhaust_win",
                0,
                new CardEffect(CardEffectType.Damage, 999),
                new CardEffect(CardEffectType.Exhaust, 1, CardTarget.Self));
            var deck = new[]
            {
                exhaustWin,
                CultivationSeedData.SwordQi,
                CultivationSeedData.SwordQi,
                CultivationSeedData.SwordQi,
                CultivationSeedData.SwordQi,
            };
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(deck, CreateTestRoute());

            var card = run.CurrentBattle.Hand.First(item => item.Id == "exhaust_win");
            new BattleEngine(1).PlayCard(run.CurrentBattle, card, run.CurrentBattle.Enemies[0]);
            engine.ResolveBattleResult(run);

            Assert.AreEqual(CultivationRunStatus.Reward, run.Status);
            Assert.IsFalse(run.Deck.Any(card => card.Id == "exhaust_win"));
            Assert.AreEqual(4, run.Deck.Count);
        }

        [Test]
        public void VictoryKeepsNonExhaustedCardsInRunDeck()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), CreateTestRoute());

            PlayFirstCard(run);
            engine.ResolveBattleResult(run);

            Assert.AreEqual(CultivationRunStatus.Reward, run.Status);
            Assert.AreEqual(5, run.Deck.Count);
            Assert.IsTrue(run.Deck.Any(card => card.Id == "instant_win"));
        }

        [Test]
        public void RunCarriesPlayerHpIntoNextBattle()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), CreateTestRoute(), playerCurrentHp: 70);

            Assert.AreEqual(70, run.CurrentBattle.Player.CurrentHp);

            run.CurrentBattle.Player.TakeDamage(12);
            PlayFirstCard(run);
            engine.ResolveBattleResult(run);

            Assert.AreEqual(58, run.PlayerCurrentHp);

            engine.SkipReward(run);

            Assert.AreEqual(CultivationRunStatus.InBattle, run.Status);
            Assert.AreEqual(58, run.CurrentBattle.Player.CurrentHp);
        }

        [Test]
        public void RestNodeHealsPersistentHpAndStartsNextBattle()
        {
            var route = CreateRouteWithRest();
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), route, playerCurrentHp: 50);

            PlayFirstCard(run);
            engine.ResolveBattleResult(run);
            engine.SkipReward(run);

            Assert.AreEqual(CultivationRunStatus.Rest, run.Status);
            Assert.AreEqual("rest_node", run.CurrentNode.Id);
            Assert.IsNull(run.CurrentBattle);

            engine.Rest(run);

            Assert.AreEqual(80, run.PlayerCurrentHp);
            Assert.AreEqual(CultivationRunStatus.InBattle, run.Status);
            Assert.AreEqual("after_rest_enemy", run.CurrentNode.Enemy.Id);
            Assert.AreEqual(80, run.CurrentBattle.Player.CurrentHp);
        }

        [Test]
        public void RestDoesNotOverhealAboveMaxHp()
        {
            var route = CreateRouteWithRest();
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), route, playerCurrentHp: 90);

            WinCurrentBattle(engine, run);
            engine.SkipReward(run);
            engine.Rest(run);

            Assert.AreEqual(100, run.PlayerCurrentHp);
            Assert.AreEqual(100, run.CurrentBattle.Player.CurrentHp);
        }

        [Test]
        public void RestAndUpgradeReplacesSelectedDeckCardAndAdvances()
        {
            var route = CreateRouteWithRest();
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CultivationSeedData.CreateSwordSectStarterDeck(), route, playerCurrentHp: 50);

            run.CurrentBattle.Outcome = BattleOutcome.Victory;
            engine.ResolveBattleResult(run);
            engine.SkipReward(run);

            var swordQiIndex = run.Deck.FindIndex(card => card.Id == "sword_qi");
            Assert.GreaterOrEqual(swordQiIndex, 0);
            Assert.AreEqual(CultivationRunStatus.Rest, run.Status);
            Assert.IsTrue(run.RestUpgradeChoices.Any(choice => choice.DeckIndex == swordQiIndex));

            engine.RestAndUpgrade(run, swordQiIndex, 0);

            Assert.AreEqual("sword_qi_damage_1", run.Deck[swordQiIndex].Id);
            Assert.AreEqual("追魂剑气", run.Deck[swordQiIndex].Name);
            Assert.AreEqual(80, run.PlayerCurrentHp);
            Assert.AreEqual(CultivationRunStatus.InBattle, run.Status);
            Assert.AreEqual("after_rest_enemy", run.CurrentNode.Enemy.Id);
            Assert.AreEqual("sword_qi_damage_1", run.CurrentBattle.DrawPile.Concat(run.CurrentBattle.Hand).Concat(run.CurrentBattle.DiscardPile).First(card => card.Id == "sword_qi_damage_1").Id);
        }

        [Test]
        public void FirstLayerUpgradeKeepsSecondLayerChoices()
        {
            var route = CreateRouteWithRest();
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CultivationSeedData.CreateSwordSectStarterDeck(), route);

            run.CurrentBattle.Outcome = BattleOutcome.Victory;
            engine.ResolveBattleResult(run);
            engine.SkipReward(run);
            var swordQiIndex = run.Deck.FindIndex(card => card.Id == "sword_qi");

            engine.RestAndUpgrade(run, swordQiIndex, 1);

            Assert.AreEqual("sword_qi_cost_1", run.Deck[swordQiIndex].Id);
            Assert.IsTrue(run.Deck[swordQiIndex].CanUpgrade);
            Assert.AreEqual(2, run.Deck[swordQiIndex].UpgradeOptions.Count);
            Assert.AreEqual("sword_qi_cost_2_draw", run.Deck[swordQiIndex].UpgradeOptions[0].UpgradedCard.Id);
            Assert.AreEqual("sword_qi_cost_2_sharpness", run.Deck[swordQiIndex].UpgradeOptions[1].UpgradedCard.Id);
        }

        [Test]
        public void SecondLayerUpgradeCanBeChosenAtLaterRestNode()
        {
            var route = CreateTwoRestRoute();
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CultivationSeedData.CreateSwordSectStarterDeck(), route);

            run.CurrentBattle.Outcome = BattleOutcome.Victory;
            engine.ResolveBattleResult(run);
            engine.SkipReward(run);
            var swordQiIndex = run.Deck.FindIndex(card => card.Id == "sword_qi");

            engine.RestAndUpgrade(run, swordQiIndex, 1);
            run.CurrentBattle.Outcome = BattleOutcome.Victory;
            engine.ResolveBattleResult(run);
            engine.SkipReward(run);

            Assert.AreEqual(CultivationRunStatus.Rest, run.Status);
            Assert.AreEqual("sword_qi_cost_1", run.Deck[swordQiIndex].Id);
            Assert.IsTrue(run.RestUpgradeChoices.Any(choice => choice.DeckIndex == swordQiIndex));

            engine.RestAndUpgrade(run, swordQiIndex, 0);

            Assert.AreEqual("sword_qi_cost_2_draw", run.Deck[swordQiIndex].Id);
            Assert.IsFalse(run.Deck[swordQiIndex].CanUpgrade);
            Assert.AreEqual(CultivationRunStatus.InBattle, run.Status);
            Assert.AreEqual("after_second_rest_enemy", run.CurrentNode.Enemy.Id);
        }

        [Test]
        public void RestAndUpgradeRejectsNonUpgradeableCard()
        {
            var route = CreateRouteWithRest();
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var plainCard = new CardDefinition("plain", "plain", 0, new CardEffect(CardEffectType.Draw, 1, CardTarget.Self));
            var run = engine.StartRun(new[] { plainCard, plainCard, plainCard, plainCard, plainCard }, route);

            run.CurrentBattle.Outcome = BattleOutcome.Victory;
            engine.ResolveBattleResult(run);
            engine.SkipReward(run);

            Assert.Throws<System.InvalidOperationException>(() => engine.RestAndUpgrade(run, 0, 0));
        }

        [Test]
        public void BranchingRouteWaitsForRouteChoiceAfterReward()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), CreateBranchingRoute());

            WinCurrentBattle(engine, run);
            engine.SkipReward(run);

            Assert.AreEqual(CultivationRunStatus.RouteChoice, run.Status);
            Assert.IsNull(run.CurrentBattle);
            Assert.AreEqual(2, run.CurrentRouteChoices.Count);
            Assert.AreEqual("branch_rest", run.CurrentRouteChoices[0].TargetNode.Id);
            Assert.AreEqual("branch_elite", run.CurrentRouteChoices[1].TargetNode.Id);
        }

        [Test]
        public void ChoosingRouteStartsSelectedNode()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), CreateBranchingRoute(), playerCurrentHp: 70);

            WinCurrentBattle(engine, run);
            engine.SkipReward(run);

            engine.ChooseRoute(run, 1);

            Assert.AreEqual(CultivationRunStatus.InBattle, run.Status);
            Assert.AreEqual(2, run.CurrentNodeIndex);
            Assert.AreEqual("branch_elite_enemy", run.CurrentNode.Enemy.Id);
            Assert.NotNull(run.CurrentBattle);
            Assert.AreEqual(70, run.CurrentBattle.Player.CurrentHp);
            Assert.AreEqual(0, run.CurrentRouteChoices.Count);
        }

        [Test]
        public void ChoosingRouteRejectsInvalidIndex()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), CreateBranchingRoute());

            WinCurrentBattle(engine, run);
            engine.SkipReward(run);

            Assert.Throws<System.ArgumentOutOfRangeException>(() => engine.ChooseRoute(run, 2));
        }

        [Test]
        public void SeedBranchingPrototypeRouteOffersFireBatOrRestAfterFirstBattle()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), CultivationSeedData.CreateFirstPrototypeBranchingRoute());

            WinCurrentBattle(engine, run);
            engine.SkipReward(run);

            Assert.AreEqual(CultivationRunStatus.RouteChoice, run.Status);
            Assert.AreEqual(2, run.CurrentRouteChoices.Count);
            Assert.AreEqual("node_fire_bat", run.CurrentRouteChoices[0].TargetNode.Id);
            Assert.AreEqual("node_meditation", run.CurrentRouteChoices[1].TargetNode.Id);
        }

        [Test]
        public void MarketNodeLoadsItemsAndBuyingAddsCardToDeck()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), CreateMarketRoute(), initialSpiritStones: 25);

            Assert.AreEqual(CultivationRunStatus.Market, run.Status);
            Assert.AreEqual(10, run.CurrentMarketItems.Count);

            var deckCount = run.Deck.Count;
            engine.BuyMarketItem(run, 0);

            Assert.AreEqual(5, run.SpiritStones);
            Assert.AreEqual(deckCount + 1, run.Deck.Count);
            Assert.AreEqual("cloud_guard", run.Deck.Last().Id);
            Assert.AreEqual(1, run.PurchasedMarketItems.Count);
            Assert.AreEqual(9, run.CurrentMarketItems.Count);
        }

        [Test]
        public void MarketBuyingPillAddsToPillSlotsWithoutChangingDeck()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), CreateMarketRoute(), initialSpiritStones: 20);
            var deckCount = run.Deck.Count;

            engine.BuyMarketItem(run, 2);

            Assert.AreEqual(5, run.SpiritStones);
            Assert.AreEqual(deckCount, run.Deck.Count);
            Assert.AreEqual(1, run.Pills.Count);
            Assert.AreEqual(1, run.PurchasedMarketPills.Count);
            Assert.AreEqual("small_restore_pill", run.Pills[0].Id);
            Assert.AreEqual(9, run.CurrentMarketItems.Count);
        }

        [Test]
        public void MarketBuyingPillRejectsWhenPillSlotsAreFull()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), CreateMarketRoute(), initialSpiritStones: 20);
            run.Pills.Add(CultivationSeedData.SmallRestorePillItem);
            run.Pills.Add(CultivationSeedData.SmallRestorePillItem);
            run.Pills.Add(CultivationSeedData.SmallRestorePillItem);

            Assert.Throws<System.InvalidOperationException>(() => engine.BuyMarketItem(run, 2));
            Assert.AreEqual(20, run.SpiritStones);
            Assert.AreEqual(5, run.Deck.Count);
            Assert.AreEqual(3, run.Pills.Count);
            Assert.AreEqual(10, run.CurrentMarketItems.Count);
            Assert.AreEqual(0, run.PurchasedMarketPills.Count);
        }

        [Test]
        public void UsePillInBattleHealsPlayerAndConsumesPill()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), CreateTestRoute(), playerCurrentHp: 50);
            run.Pills.Add(CultivationSeedData.SmallRestorePillItem);
            run.CurrentBattle.Player.TakeDamage(12);

            engine.UsePillInBattle(run, 0);

            Assert.AreEqual(48, run.CurrentBattle.Player.CurrentHp);
            Assert.AreEqual(0, run.Pills.Count);
            Assert.IsTrue(run.CurrentBattle.Logs.Any(log => log.Message.Contains("使用 小还丹，恢复 10 HP")));
        }

        [Test]
        public void UsePillInBattleDoesNotOverheal()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), CreateTestRoute(), playerCurrentHp: 96);
            run.Pills.Add(CultivationSeedData.SmallRestorePillItem);

            engine.UsePillInBattle(run, 0);

            Assert.AreEqual(100, run.CurrentBattle.Player.CurrentHp);
            Assert.AreEqual(0, run.Pills.Count);
        }

        [Test]
        public void MarketBuyingBigRestorePillAndUsingItHealsMore()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), CreateMarketRoute(), playerCurrentHp: 50, initialSpiritStones: 40);
            engine.BuyMarketItem(run, 3);
            engine.LeaveMarket(run);
            run.CurrentBattle.Player.TakeDamage(20);

            engine.UsePillInBattle(run, 0);

            Assert.AreEqual(5, run.SpiritStones);
            Assert.AreEqual("big_restore_pill", run.PurchasedMarketPills[0].Id);
            Assert.AreEqual(45, run.CurrentBattle.Player.CurrentHp);
            Assert.AreEqual(0, run.Pills.Count);
            Assert.IsTrue(run.CurrentBattle.Logs.Any(log => log.Message.Contains("使用 大还丹，恢复 15 HP")));
        }

        [Test]
        public void UsePillInBattleRejectsInvalidStateAndKeepsPill()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), CreateMarketRoute(), initialSpiritStones: 20);
            run.Pills.Add(CultivationSeedData.SmallRestorePillItem);

            Assert.Throws<System.InvalidOperationException>(() => engine.UsePillInBattle(run, 0));
            Assert.AreEqual(1, run.Pills.Count);
        }

        [Test]
        public void MarketBuyingSpiritPillAndUsingItAddsTemporarySpirit()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), CreateMarketRoute(), initialSpiritStones: 40);
            engine.BuyMarketItem(run, 4);
            engine.LeaveMarket(run);
            var spiritBefore = run.CurrentBattle.Spirit;

            engine.UsePillInBattle(run, 0);

            Assert.AreEqual(5, run.SpiritStones);
            Assert.AreEqual("spirit_boost_pill", run.PurchasedMarketPills[0].Id);
            Assert.AreEqual(spiritBefore + 2, run.CurrentBattle.Spirit);
            Assert.AreEqual(0, run.Pills.Count);
            Assert.IsTrue(run.CurrentBattle.Logs.Any(log => log.Message.Contains("使用 增元丹，本回合灵力 +2")));
        }

        [Test]
        public void MarketBuyingCleansePillAndUsingItClearsNegativeStatusesAndHeals()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), CreateMarketRoute(), playerCurrentHp: 50, initialSpiritStones: 20);
            engine.BuyMarketItem(run, 5);
            engine.LeaveMarket(run);
            run.CurrentBattle.Player.TakeDamage(6);
            run.CurrentBattle.Player.AddBurn(3, 2);
            run.CurrentBattle.Player.AddBreakDefense(2);

            engine.UsePillInBattle(run, 0);

            Assert.AreEqual(5, run.SpiritStones);
            Assert.AreEqual("cleanse_pill", run.PurchasedMarketPills[0].Id);
            Assert.AreEqual(47, run.CurrentBattle.Player.CurrentHp);
            Assert.AreEqual(0, run.CurrentBattle.Player.BurnStacks);
            Assert.AreEqual(0, run.CurrentBattle.Player.BurnTurns);
            Assert.AreEqual(0, run.CurrentBattle.Player.BreakDefenseStacks);
            Assert.AreEqual(0, run.Pills.Count);
            Assert.IsTrue(run.CurrentBattle.Logs.Any(log => log.Message.Contains("使用 解毒丹，清除负面状态并恢复 3 HP")));
        }

        [Test]
        public void MarketBuyingBreakthroughPillAndUsingItReducesCardCostsForBattle()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), CreateMarketRoute(), initialSpiritStones: 95);
            engine.BuyMarketItem(run, 6);
            engine.LeaveMarket(run);
            var twoCostCard = new CardDefinition("two_cost_test", "二费测试", 2, new CardEffect(CardEffectType.Damage, 1));
            run.CurrentBattle.Hand.Clear();
            run.CurrentBattle.Hand.Add(twoCostCard);
            run.CurrentBattle.Spirit = 1;

            Assert.IsFalse(new BattleEngine(1).CanPlay(run.CurrentBattle, twoCostCard));

            engine.UsePillInBattle(run, 0);

            Assert.AreEqual(5, run.SpiritStones);
            Assert.AreEqual("breakthrough_pill", run.PurchasedMarketPills[0].Id);
            Assert.AreEqual(1, run.CurrentBattle.SpiritCostReduction);
            Assert.AreEqual(1, run.CurrentBattle.GetEffectiveSpiritCost(twoCostCard));
            Assert.IsTrue(new BattleEngine(1).CanPlay(run.CurrentBattle, twoCostCard));
            Assert.AreEqual(0, run.Pills.Count);
            Assert.IsTrue(run.CurrentBattle.Logs.Any(log => log.Message.Contains("使用 破境丹，本场战斗功法灵力消耗 -1")));
        }

        [Test]
        public void MarketBuyingFoundationPillAndUsingItPermanentlyIncreasesRunMaxHp()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), CreateMarketRoute(), playerCurrentHp: 50, initialSpiritStones: 75);
            engine.BuyMarketItem(run, 7);

            var logMessage = engine.UsePillInRun(run, 0);

            Assert.AreEqual(5, run.SpiritStones);
            Assert.AreEqual("foundation_pill", run.PurchasedMarketPills[0].Id);
            Assert.AreEqual(110, run.PlayerMaxHp);
            Assert.AreEqual(60, run.PlayerCurrentHp);
            Assert.AreEqual(0, run.Pills.Count);
            StringAssert.Contains("使用 筑基丹，本 Run 最大 HP +10", logMessage);
        }

        [Test]
        public void MarketBuyingSpiritStoneMineAddsBonusSpiritStonesOnVictory()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), CreateMarketRoute(), initialSpiritStones: 30);
            engine.BuyMarketItem(run, 8);

            Assert.AreEqual(5, run.SpiritStones);
            Assert.AreEqual(1, run.Artifacts.Count);
            Assert.AreEqual(1, run.PurchasedMarketArtifacts.Count);
            Assert.AreEqual("spirit_stone_mine", run.Artifacts[0].Id);

            engine.LeaveMarket(run);
            new BattleEngine(1).PlayCard(run.CurrentBattle, run.CurrentBattle.Hand[0], run.CurrentBattle.Enemies[0]);
            engine.ResolveBattleResult(run);

            Assert.AreEqual(10, run.SpiritStones);
            Assert.IsTrue(run.CurrentBattle.Logs.Any(log => log.Message.Contains("法宝额外获得 5 灵石")));
        }

        [Test]
        public void ChestNodeOpensArtifactAndAdvancesToNextNode()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), CreateChestRoute());

            Assert.AreEqual(CultivationRunStatus.Chest, run.Status);
            Assert.AreEqual("chest", run.CurrentNode.Id);

            var artifact = engine.OpenChest(run);

            Assert.NotNull(artifact);
            Assert.AreEqual(1, run.Artifacts.Count);
            Assert.AreEqual(1, run.ChestArtifacts.Count);
            Assert.AreEqual(artifact.Id, run.Artifacts[0].Id);
            Assert.AreEqual(artifact.Id, run.ChestArtifacts[0].Id);
            Assert.AreEqual(CultivationRunStatus.InBattle, run.Status);
            Assert.AreEqual("after_chest_enemy", run.CurrentNode.Enemy.Id);
        }

        [Test]
        public void OpenChestRejectsNonChestState()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), CreateTestRoute());

            Assert.Throws<System.InvalidOperationException>(() => engine.OpenChest(run));
            Assert.AreEqual(0, run.Artifacts.Count);
            Assert.AreEqual(0, run.ChestArtifacts.Count);
        }

        [Test]
        public void OpenChestRejectsMissingArtifactPool()
        {
            var route = new[]
            {
                new CultivationRunNode(
                    "empty_chest",
                    "empty_chest",
                    CultivationRunNodeType.Chest,
                    null,
                    null),
            };
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), route);

            Assert.Throws<System.InvalidOperationException>(() => engine.OpenChest(run));
            Assert.AreEqual(CultivationRunStatus.Chest, run.Status);
            Assert.AreEqual(0, run.Artifacts.Count);
            Assert.AreEqual(0, run.ChestArtifacts.Count);
        }

        [Test]
        public void MysticEventHealOptionRestoresHpAndAdvances()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), CreateMysticRoute(), playerCurrentHp: 50);

            Assert.AreEqual(CultivationRunStatus.Mystic, run.Status);
            Assert.AreEqual(3, run.MysticEventChoices.Count);

            var option = engine.ChooseMysticEventOption(run, 0);

            Assert.AreEqual("drink_spirit_spring", option.Id);
            Assert.AreEqual(80, run.PlayerCurrentHp);
            Assert.AreEqual(1, run.ResolvedMysticEventOptions.Count);
            Assert.AreEqual(CultivationRunStatus.InBattle, run.Status);
            Assert.AreEqual("after_mystic_enemy", run.CurrentNode.Enemy.Id);
            Assert.AreEqual(0, run.MysticEventChoices.Count);
        }

        [Test]
        public void MysticEventPillOptionAddsPillAndAdvances()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), CreateMysticRoute());

            var option = engine.ChooseMysticEventOption(run, 1);

            Assert.AreEqual("collect_small_restore_pill", option.Id);
            Assert.AreEqual(1, run.Pills.Count);
            Assert.AreEqual("small_restore_pill", run.Pills[0].Id);
            Assert.AreEqual(1, run.ResolvedMysticEventOptions.Count);
            Assert.AreEqual(CultivationRunStatus.InBattle, run.Status);
        }

        [Test]
        public void ChooseMysticEventOptionRejectsNonMysticState()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), CreateTestRoute());

            Assert.Throws<System.InvalidOperationException>(() => engine.ChooseMysticEventOption(run, 0));
            Assert.AreEqual(0, run.ResolvedMysticEventOptions.Count);
        }

        [Test]
        public void MarketBuyingRejuvenationJadeHealsAfterVictory()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), CreateMarketRoute(), playerCurrentHp: 70, initialSpiritStones: 35);
            engine.BuyMarketItem(run, 9);

            Assert.AreEqual(5, run.SpiritStones);
            Assert.AreEqual(1, run.Artifacts.Count);
            Assert.AreEqual(1, run.PurchasedMarketArtifacts.Count);
            Assert.AreEqual("rejuvenation_jade", run.Artifacts[0].Id);

            engine.LeaveMarket(run);
            run.CurrentBattle.Player.TakeDamage(30);
            new BattleEngine(1).PlayCard(run.CurrentBattle, run.CurrentBattle.Hand[0], run.CurrentBattle.Enemies[0]);
            engine.ResolveBattleResult(run);

            Assert.AreEqual(43, run.PlayerCurrentHp);
            Assert.AreEqual(43, run.CurrentBattle.Player.CurrentHp);
            Assert.IsTrue(run.CurrentBattle.Logs.Any(log => log.Message.Contains("法宝恢复 3 HP")));
        }

        [Test]
        public void RejuvenationJadeDoesNotOverhealAfterVictory()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), CreateTestRoute(), playerCurrentHp: 99);
            run.Artifacts.Add(CultivationSeedData.RejuvenationJadeArtifact);

            WinCurrentBattle(engine, run);

            Assert.AreEqual(100, run.PlayerCurrentHp);
            Assert.AreEqual(100, run.CurrentBattle.Player.CurrentHp);
        }

        [Test]
        public void UsePillInRunRejectsBattleStateAndKeepsPill()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), CreateTestRoute());
            run.Pills.Add(CultivationSeedData.FoundationPillItem);

            Assert.Throws<System.InvalidOperationException>(() => engine.UsePillInRun(run, 0));
            Assert.AreEqual(100, run.PlayerMaxHp);
            Assert.AreEqual(1, run.Pills.Count);
        }

        [Test]
        public void MarketRejectsPurchaseWithoutEnoughSpiritStones()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), CreateMarketRoute(), initialSpiritStones: 10);

            Assert.Throws<System.InvalidOperationException>(() => engine.BuyMarketItem(run, 0));
            Assert.AreEqual(10, run.SpiritStones);
            Assert.AreEqual(5, run.Deck.Count);
            Assert.AreEqual(0, run.PurchasedMarketItems.Count);
        }

        [Test]
        public void MarketCardRemovalCostsSpiritStonesAndRemovesSelectedDeckCard()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var deck = CreateUniqueTestDeck();
            var run = engine.StartRun(deck, CreateMarketRoute(), initialSpiritStones: 40);
            var deckCount = run.Deck.Count;
            var removedCard = run.Deck[1];

            engine.RemoveDeckCardAtMarket(run, 1);

            Assert.AreEqual(5, run.SpiritStones);
            Assert.AreEqual(deckCount - 1, run.Deck.Count);
            Assert.AreSame(removedCard, run.RemovedMarketCards.Single());
            Assert.IsFalse(run.Deck.Contains(removedCard));
        }

        [Test]
        public void MarketCardRemovalRejectsWithoutEnoughSpiritStones()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), CreateMarketRoute(), initialSpiritStones: 34);

            Assert.Throws<System.InvalidOperationException>(() => engine.RemoveDeckCardAtMarket(run, 0));
            Assert.AreEqual(34, run.SpiritStones);
            Assert.AreEqual(5, run.Deck.Count);
            Assert.AreEqual(0, run.RemovedMarketCards.Count);
        }

        [Test]
        public void MarketCardRemovalRejectsInvalidDeckIndex()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), CreateMarketRoute(), initialSpiritStones: 40);

            Assert.Throws<System.ArgumentOutOfRangeException>(() => engine.RemoveDeckCardAtMarket(run, 99));
            Assert.AreEqual(40, run.SpiritStones);
            Assert.AreEqual(5, run.Deck.Count);
            Assert.AreEqual(0, run.RemovedMarketCards.Count);
        }

        [Test]
        public void MarketCardRemovalRejectsLastDeckCard()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var oneCard = new[] { new CardDefinition("only_card", "only_card", 0, new CardEffect(CardEffectType.Damage, 1)) };
            var run = engine.StartRun(oneCard, CreateMarketRoute(), initialSpiritStones: 40);

            Assert.Throws<System.InvalidOperationException>(() => engine.RemoveDeckCardAtMarket(run, 0));
            Assert.AreEqual(40, run.SpiritStones);
            Assert.AreEqual(1, run.Deck.Count);
            Assert.AreEqual(0, run.RemovedMarketCards.Count);
        }

        [Test]
        public void MarketCardUpgradeCostsSpiritStonesAndReplacesSelectedDeckCard()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CultivationSeedData.CreateSwordSectStarterDeck(), CreateMarketRoute(), initialSpiritStones: 55);
            var swordQiIndex = run.Deck.FindIndex(card => card.Id == "sword_qi");

            engine.UpgradeDeckCardAtMarket(run, swordQiIndex, 0);

            Assert.AreEqual(5, run.SpiritStones);
            Assert.AreEqual("sword_qi_damage_1", run.Deck[swordQiIndex].Id);
            Assert.AreEqual(1, run.MarketUpgradedCards.Count);
            Assert.AreEqual("sword_qi_damage_1", run.MarketUpgradedCards[0].Id);
        }

        [Test]
        public void MarketCardUpgradeRejectsWithoutEnoughSpiritStones()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CultivationSeedData.CreateSwordSectStarterDeck(), CreateMarketRoute(), initialSpiritStones: 49);
            var swordQiIndex = run.Deck.FindIndex(card => card.Id == "sword_qi");

            Assert.Throws<System.InvalidOperationException>(() => engine.UpgradeDeckCardAtMarket(run, swordQiIndex, 0));
            Assert.AreEqual(49, run.SpiritStones);
            Assert.AreEqual("sword_qi", run.Deck[swordQiIndex].Id);
            Assert.AreEqual(0, run.MarketUpgradedCards.Count);
        }

        [Test]
        public void MarketCardUpgradeRejectsNonUpgradeableCard()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), CreateMarketRoute(), initialSpiritStones: 55);

            Assert.Throws<System.InvalidOperationException>(() => engine.UpgradeDeckCardAtMarket(run, 0, 0));
            Assert.AreEqual(55, run.SpiritStones);
            Assert.AreEqual("instant_win", run.Deck[0].Id);
            Assert.AreEqual(0, run.MarketUpgradedCards.Count);
        }

        [Test]
        public void MarketCardUpgradeRejectsInvalidUpgradeOption()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CultivationSeedData.CreateSwordSectStarterDeck(), CreateMarketRoute(), initialSpiritStones: 55);
            var swordQiIndex = run.Deck.FindIndex(card => card.Id == "sword_qi");

            Assert.Throws<System.ArgumentOutOfRangeException>(() => engine.UpgradeDeckCardAtMarket(run, swordQiIndex, 99));
            Assert.AreEqual(55, run.SpiritStones);
            Assert.AreEqual("sword_qi", run.Deck[swordQiIndex].Id);
            Assert.AreEqual(0, run.MarketUpgradedCards.Count);
        }

        [Test]
        public void MarketCardSellingGrantsHalfOfMatchingMarketItemPriceAndRemovesCard()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var deck = new[]
            {
                CultivationSeedData.CloudGuard,
                CultivationSeedData.SwordQi,
                CultivationSeedData.BreakArmor,
                CultivationSeedData.GuardQi,
                CultivationSeedData.LightBody,
            };
            var run = engine.StartRun(deck, CreateMarketRoute(), initialSpiritStones: 3);

            Assert.AreEqual(10, engine.GetMarketSellValue(run, 0));
            engine.SellDeckCardAtMarket(run, 0);

            Assert.AreEqual(13, run.SpiritStones);
            Assert.AreEqual(4, run.Deck.Count);
            Assert.AreEqual(1, run.SoldMarketCards.Count);
            Assert.AreEqual("cloud_guard", run.SoldMarketCards[0].Id);
            Assert.IsFalse(run.Deck.Any(card => card.Id == "cloud_guard"));
        }

        [Test]
        public void MarketCardSellingUsesLowestMarketPriceFallbackForUnlistedCard()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateUniqueTestDeck(), CreateMarketRoute(), initialSpiritStones: 3);

            Assert.AreEqual(10, engine.GetMarketSellValue(run, 0));
            engine.SellDeckCardAtMarket(run, 0);

            Assert.AreEqual(13, run.SpiritStones);
            Assert.AreEqual(4, run.Deck.Count);
            Assert.AreEqual(1, run.SoldMarketCards.Count);
        }

        [Test]
        public void MarketCardSellingRejectsInvalidDeckIndex()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), CreateMarketRoute(), initialSpiritStones: 3);

            Assert.Throws<System.ArgumentOutOfRangeException>(() => engine.SellDeckCardAtMarket(run, 99));
            Assert.AreEqual(3, run.SpiritStones);
            Assert.AreEqual(5, run.Deck.Count);
            Assert.AreEqual(0, run.SoldMarketCards.Count);
        }

        [Test]
        public void MarketCardSellingRejectsLastDeckCard()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var oneCard = new[] { new CardDefinition("only_card", "only_card", 0, new CardEffect(CardEffectType.Damage, 1)) };
            var run = engine.StartRun(oneCard, CreateMarketRoute(), initialSpiritStones: 3);

            Assert.Throws<System.InvalidOperationException>(() => engine.SellDeckCardAtMarket(run, 0));
            Assert.AreEqual(3, run.SpiritStones);
            Assert.AreEqual(1, run.Deck.Count);
            Assert.AreEqual(0, run.SoldMarketCards.Count);
        }

        [Test]
        public void LeavingMarketAdvancesToNextNode()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), CreateMarketRoute(), initialSpiritStones: 25);

            engine.LeaveMarket(run);

            Assert.AreEqual(CultivationRunStatus.InBattle, run.Status);
            Assert.AreEqual("after_market_enemy", run.CurrentNode.Enemy.Id);
            Assert.AreEqual(0, run.CurrentMarketItems.Count);
        }

        [Test]
        public void SwordSectRewardPoolIncludesSwordMarkEntryAndPayoffCards()
        {
            var rewards = CultivationSeedData.CreateSwordSectRewardPool();

            Assert.IsTrue(rewards.Any(reward => reward.Card.Id == "thrust"));
            Assert.IsTrue(rewards.Any(reward => reward.Card.Id == "sevenfold_sword_qi"));
        }

        [Test]
        public void SkippingFinalRewardCompletesRun()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), CreateTestRoute());

            WinCurrentBattle(engine, run);
            engine.SkipReward(run);
            WinCurrentBattle(engine, run);
            engine.SkipReward(run);

            Assert.AreEqual(CultivationRunStatus.Completed, run.Status);
            Assert.AreEqual(1, run.CurrentNodeIndex);
            Assert.IsNull(run.CurrentBattle);
            Assert.AreEqual(0, run.CurrentRewards.Count);
        }

        [Test]
        public void BattleDefeatEndsRun()
        {
            var deadlyEnemy = new EnemyDefinition(
                "deadly_enemy",
                "deadly_enemy",
                10,
                0,
                new EnemyIntent(EnemyIntentType.Attack, 150));
            var route = new[]
            {
                new CultivationRunNode("deadly", "deadly", CultivationRunNodeType.Battle, deadlyEnemy, CultivationSeedData.CreateSwordSectRewardPool()),
            };
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CultivationSeedData.CreateSwordSectStarterDeck(), route);

            var battleEngine = new BattleEngine(1);
            battleEngine.EndPlayerTurn(run.CurrentBattle);
            engine.ResolveBattleResult(run);

            Assert.AreEqual(CultivationRunStatus.Defeated, run.Status);
            Assert.AreEqual(0, run.PlayerCurrentHp);
            Assert.AreEqual(0, run.SpiritStones);
            Assert.IsNull(run.CurrentBattle);
            Assert.AreEqual(0, run.CurrentRewards.Count);
        }

        private static void WinCurrentBattle(CultivationRunEngine engine, CultivationRunState run)
        {
            PlayFirstCard(run);
            engine.ResolveBattleResult(run);
        }

        private static void PlayFirstCard(CultivationRunState run)
        {
            var engine = new BattleEngine(1);
            var card = run.CurrentBattle.Hand.First(item => item.Id == "instant_win");
            engine.PlayCard(run.CurrentBattle, card, run.CurrentBattle.Enemies[0]);
            Assert.AreEqual(BattleOutcome.Victory, run.CurrentBattle.Outcome);
        }

        private static IReadOnlyList<CardDefinition> CreateInstantWinDeck()
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

        private static IReadOnlyList<CardDefinition> CreateUniqueTestDeck()
        {
            return new[]
            {
                new CardDefinition("unique_card_1", "unique_card_1", 0, new CardEffect(CardEffectType.Damage, 1)),
                new CardDefinition("unique_card_2", "unique_card_2", 0, new CardEffect(CardEffectType.Damage, 1)),
                new CardDefinition("unique_card_3", "unique_card_3", 0, new CardEffect(CardEffectType.Damage, 1)),
                new CardDefinition("unique_card_4", "unique_card_4", 0, new CardEffect(CardEffectType.Damage, 1)),
                new CardDefinition("unique_card_5", "unique_card_5", 0, new CardEffect(CardEffectType.Damage, 1)),
            };
        }

        private static IReadOnlyList<CultivationRunNode> CreateTestRoute()
        {
            var rewards = CultivationSeedData.CreateSwordSectRewardPool();
            return new List<CultivationRunNode>
            {
                new CultivationRunNode(
                    "test_node_1",
                    "test_node_1",
                    CultivationRunNodeType.Battle,
                    new EnemyDefinition("test_enemy_1", "test_enemy_1", 1, 0, new EnemyIntent(EnemyIntentType.Attack, 1)),
                    rewards),
                new CultivationRunNode(
                    "test_node_2",
                    "test_node_2",
                    CultivationRunNodeType.Elite,
                    new EnemyDefinition("test_enemy_2", "test_enemy_2", 1, 0, new EnemyIntent(EnemyIntentType.Attack, 1)),
                    rewards,
                    artifactRewardPool: new[] { CultivationSeedData.SpiritStoneMineArtifact }),
            };
        }

        private static IReadOnlyList<CultivationRunNode> CreateRouteWithRest()
        {
            var rewards = CultivationSeedData.CreateSwordSectRewardPool();
            return new List<CultivationRunNode>
            {
                new CultivationRunNode(
                    "before_rest",
                    "before_rest",
                    CultivationRunNodeType.Battle,
                    new EnemyDefinition("before_rest_enemy", "before_rest_enemy", 1, 0, new EnemyIntent(EnemyIntentType.Attack, 1)),
                    rewards),
                new CultivationRunNode(
                    "rest_node",
                    "rest_node",
                    CultivationRunNodeType.Rest,
                    null,
                    null,
                    restHealAmount: 30),
                new CultivationRunNode(
                    "after_rest",
                    "after_rest",
                    CultivationRunNodeType.Elite,
                    new EnemyDefinition("after_rest_enemy", "after_rest_enemy", 1, 0, new EnemyIntent(EnemyIntentType.Attack, 1)),
                rewards),
            };
        }

        private static IReadOnlyList<CultivationRunNode> CreateTwoRestRoute()
        {
            var rewards = CultivationSeedData.CreateSwordSectRewardPool();
            return new List<CultivationRunNode>
            {
                new CultivationRunNode(
                    "before_first_rest",
                    "before_first_rest",
                    CultivationRunNodeType.Battle,
                    new EnemyDefinition("before_first_rest_enemy", "before_first_rest_enemy", 1, 0, new EnemyIntent(EnemyIntentType.Attack, 1)),
                    rewards),
                new CultivationRunNode(
                    "first_rest",
                    "first_rest",
                    CultivationRunNodeType.Rest,
                    null,
                    null,
                    restHealAmount: 30),
                new CultivationRunNode(
                    "before_second_rest",
                    "before_second_rest",
                    CultivationRunNodeType.Battle,
                    new EnemyDefinition("before_second_rest_enemy", "before_second_rest_enemy", 1, 0, new EnemyIntent(EnemyIntentType.Attack, 1)),
                    rewards),
                new CultivationRunNode(
                    "second_rest",
                    "second_rest",
                    CultivationRunNodeType.Rest,
                    null,
                    null,
                    restHealAmount: 30),
                new CultivationRunNode(
                    "after_second_rest",
                    "after_second_rest",
                    CultivationRunNodeType.Elite,
                    new EnemyDefinition("after_second_rest_enemy", "after_second_rest_enemy", 1, 0, new EnemyIntent(EnemyIntentType.Attack, 1)),
                    rewards),
            };
        }

        private static IReadOnlyList<CultivationRunNode> CreateBranchingRoute()
        {
            var rewards = CultivationSeedData.CreateSwordSectRewardPool();
            return new List<CultivationRunNode>
            {
                new CultivationRunNode(
                    "branch_start",
                    "branch_start",
                    CultivationRunNodeType.Battle,
                    new EnemyDefinition("branch_start_enemy", "branch_start_enemy", 1, 0, new EnemyIntent(EnemyIntentType.Attack, 1)),
                    rewards,
                    nextNodeIndices: new[] { 1, 2 }),
                new CultivationRunNode(
                    "branch_rest",
                    "branch_rest",
                    CultivationRunNodeType.Rest,
                    null,
                    null,
                    restHealAmount: 20),
                new CultivationRunNode(
                    "branch_elite",
                    "branch_elite",
                    CultivationRunNodeType.Elite,
                    new EnemyDefinition("branch_elite_enemy", "branch_elite_enemy", 1, 0, new EnemyIntent(EnemyIntentType.Attack, 1)),
                rewards),
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
                    nextNodeIndices: new[] { 1 },
                    marketItems: new[]
                    {
                        new CultivationMarketItem("market_cloud_guard", CultivationSeedData.CloudGuard, 20),
                        new CultivationMarketItem("market_thrust", CultivationSeedData.Thrust, 25),
                        new CultivationMarketItem("market_small_restore_pill", CultivationSeedData.SmallRestorePillItem, 15),
                        new CultivationMarketItem("market_big_restore_pill", CultivationSeedData.BigRestorePillItem, 35),
                        new CultivationMarketItem("market_spirit_boost_pill", CultivationSeedData.SpiritBoostPillItem, 35),
                        new CultivationMarketItem("market_cleanse_pill", CultivationSeedData.CleansePillItem, 15),
                        new CultivationMarketItem("market_breakthrough_pill", CultivationSeedData.BreakthroughPillItem, 90),
                        new CultivationMarketItem("market_foundation_pill", CultivationSeedData.FoundationPillItem, 70),
                        new CultivationMarketItem("market_spirit_stone_mine", CultivationSeedData.SpiritStoneMineArtifact, 25),
                        new CultivationMarketItem("market_rejuvenation_jade", CultivationSeedData.RejuvenationJadeArtifact, 30),
                    }),
                new CultivationRunNode(
                    "after_market",
                    "after_market",
                    CultivationRunNodeType.Battle,
                    new EnemyDefinition("after_market_enemy", "after_market_enemy", 1, 0, new EnemyIntent(EnemyIntentType.Attack, 1)),
                    CultivationSeedData.CreateSwordSectRewardPool()),
            };
        }

        private static IReadOnlyList<CultivationRunNode> CreateChestRoute()
        {
            return new List<CultivationRunNode>
            {
                new CultivationRunNode(
                    "chest",
                    "chest",
                    CultivationRunNodeType.Chest,
                    null,
                    null,
                    nextNodeIndices: new[] { 1 },
                    artifactRewardPool: new[] { CultivationSeedData.SpiritStoneMineArtifact }),
                new CultivationRunNode(
                    "after_chest",
                    "after_chest",
                    CultivationRunNodeType.Battle,
                    new EnemyDefinition("after_chest_enemy", "after_chest_enemy", 1, 0, new EnemyIntent(EnemyIntentType.Attack, 1)),
                    CultivationSeedData.CreateSwordSectRewardPool()),
            };
        }

        private static IReadOnlyList<CultivationRunNode> CreateMysticRoute()
        {
            return new List<CultivationRunNode>
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
        }
    }
}
