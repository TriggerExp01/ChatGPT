using System.Linq;
using GameLogic.Cultivation;
using NUnit.Framework;

namespace GameLogic.Tests
{
    public sealed class CultivationRunSessionTests
    {
        [Test]
        public void StartNewRunUsesFullBranchingCultivationRoute()
        {
            var session = new CultivationRunSession();

            var run = session.StartNewRun(CultivationSect.Sword);

            Assert.AreEqual(CultivationRunStatus.InBattle, run.Status);
            Assert.AreEqual("山门石魔", run.CurrentNode.Name);
            Assert.AreEqual(13, run.Route.Count);
            Assert.IsTrue(run.Route.Any(node => node.Type == CultivationRunNodeType.Rest));
            Assert.IsTrue(run.Route.Any(node => node.Type == CultivationRunNodeType.Market));
            Assert.IsTrue(run.Route.Any(node => node.Type == CultivationRunNodeType.Chest));
            Assert.IsTrue(run.Route.Any(node => node.Type == CultivationRunNodeType.Mystic));
            Assert.IsTrue(run.Route.Any(node => node.Type == CultivationRunNodeType.Elite));
            Assert.AreEqual(5, run.CurrentBattle.Hand.Count);
        }

        [Test]
        public void RealBattleVictoryAdvancesThroughRewardRouteAndRest()
        {
            var session = new CultivationRunSession();
            var run = session.StartNewRun(CultivationSect.Sword);
            var instantWin = new CardDefinition(
                "phase84_test_finisher",
                "验收·破境一击",
                0,
                new CardEffect(CardEffectType.Damage, 999));
            run.CurrentBattle.Hand.Insert(0, instantWin);

            Assert.IsTrue(session.PlayCardAt(0));
            Assert.AreEqual(BattleOutcome.Victory, run.CurrentBattle.Outcome);

            Assert.IsTrue(session.ResolveBattle());
            Assert.AreEqual(CultivationRunStatus.Reward, run.Status);
            Assert.AreEqual(3, run.CurrentRewards.Count);

            Assert.IsTrue(session.SkipReward());
            Assert.AreEqual(CultivationRunStatus.RouteChoice, run.Status);
            Assert.GreaterOrEqual(run.CurrentRouteChoices.Count, 2);

            Assert.IsTrue(session.ChooseRoute(1));
            Assert.AreEqual(CultivationRunStatus.Rest, run.Status);
            Assert.AreEqual("闭关调息", run.CurrentNode.Name);

            Assert.IsTrue(session.Rest());
            Assert.AreEqual(CultivationRunStatus.RouteChoice, run.Status);
        }

        [Test]
        public void RealEnemyTurnCanDefeatTheRun()
        {
            var session = new CultivationRunSession();
            var run = session.StartNewRun(CultivationSect.Sword);
            run.CurrentBattle.Player.TakeDirectDamage(run.CurrentBattle.Player.CurrentHp - 1);

            Assert.IsTrue(session.EndTurn());
            Assert.AreEqual(BattleOutcome.Defeat, run.CurrentBattle.Outcome);

            Assert.IsTrue(session.ResolveBattle());
            Assert.AreEqual(CultivationRunStatus.Defeated, run.Status);
            Assert.AreEqual(0, run.PlayerCurrentHp);
            Assert.IsNull(run.CurrentBattle);
        }
    }
}
