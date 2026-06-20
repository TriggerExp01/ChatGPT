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
            Assert.AreEqual(3, run.Route.Count);
            Assert.AreEqual(CultivationSeedData.StoneDemon.Id, run.CurrentNode.Enemy.Id);
            Assert.NotNull(run.CurrentBattle);
            Assert.AreEqual(5, run.CurrentBattle.Hand.Count);
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
    }
}
