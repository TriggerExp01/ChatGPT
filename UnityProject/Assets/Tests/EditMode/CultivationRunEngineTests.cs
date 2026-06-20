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
        }

        [Test]
        public void VictoryMovesRunToRewardState()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), CreateTestRoute());

            PlayFirstCard(run);
            engine.ResolveBattleResult(run);

            Assert.AreEqual(CultivationRunStatus.Reward, run.Status);
            Assert.AreEqual(3, run.CurrentRewards.Count);
            Assert.AreEqual("reward_flying_sword", run.CurrentRewards[0].Id);
        }

        [Test]
        public void ChoosingRewardAddsCardAndStartsNextNode()
        {
            var engine = new CultivationRunEngine(new BattleEngine(1));
            var run = engine.StartRun(CreateInstantWinDeck(), CreateTestRoute());

            PlayFirstCard(run);
            engine.ResolveBattleResult(run);
            var deckCountBeforeReward = run.Deck.Count;

            engine.ChooseReward(run, 1);

            Assert.AreEqual(CultivationRunStatus.InBattle, run.Status);
            Assert.AreEqual(1, run.CurrentNodeIndex);
            Assert.AreEqual(deckCountBeforeReward + 1, run.Deck.Count);
            Assert.AreEqual("cloud_guard", run.Deck.Last().Id);
            Assert.AreEqual(1, run.ClaimedRewards.Count);
            Assert.NotNull(run.CurrentBattle);
            Assert.AreEqual("test_enemy_2", run.CurrentNode.Enemy.Id);
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
                    rewards),
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
    }
}
