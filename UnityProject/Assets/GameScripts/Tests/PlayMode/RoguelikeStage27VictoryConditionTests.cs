using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using TEngine;
using UnityEngine;

namespace GameLogic.Tests
{
    public sealed class RoguelikeStage27VictoryConditionTests
    {
        private const string MetaGoldKey = "Roguelike.MetaGold";

        [SetUp]
        public void ClearMetaGold()
        {
            PlayerPrefs.DeleteKey(MetaGoldKey);
            PlayerPrefs.Save();
        }

        [Test]
        public void KillingBossCompletesRunAndBanksGold()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(27001);
            game.CurrentRun.AddGold(12);

            List<RoguelikeSurvivalEnemy> enemies = GetEnemyList(game);
            enemies.Clear();
            RoguelikeSurvivalEnemy boss = MemoryPool.Acquire<RoguelikeSurvivalEnemy>();
            boss.Init(27001, new Vector2(1f, 0f), 1, 1, 0f, true, "dungeon_heart", true);
            boss.Health = 0;
            enemies.Add(boss);

            game.Tick(0.1f);

            Assert.That(game.Phase, Is.EqualTo(RoguelikeGamePhase.Victory));
            Assert.That(game.KillCount, Is.EqualTo(1));
            Assert.That(game.MetaGold, Is.EqualTo(37));
            Assert.That(game.LastMessage, Does.Contain("生存目标达成"));
            Assert.That(game.SettlementSummary, Does.Contain("永久金币 37"));
        }

        [Test]
        public void KillingCommonEnemyDoesNotCompleteRun()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(27002);

            List<RoguelikeSurvivalEnemy> enemies = GetEnemyList(game);
            enemies.Clear();
            RoguelikeSurvivalEnemy enemy = MemoryPool.Acquire<RoguelikeSurvivalEnemy>();
            enemy.Init(27002, new Vector2(1f, 0f), 1, 1, 0f, true, "slime", false);
            enemy.Health = 0;
            enemies.Add(enemy);

            game.Tick(0.1f);

            Assert.That(game.Phase, Is.EqualTo(RoguelikeGamePhase.Running));
            Assert.That(game.KillCount, Is.EqualTo(1));
            Assert.That(game.Pickups.Count, Is.GreaterThanOrEqualTo(1));
            Assert.That(game.MetaGold, Is.EqualTo(0));
        }

        [Test]
        public void RestartAfterVictoryResetsRunState()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(27003);
            game.DebugForceVictory();
            Assert.That(game.Phase, Is.EqualTo(RoguelikeGamePhase.Victory));

            game.StartNewRun(27004);

            Assert.That(game.Phase, Is.EqualTo(RoguelikeGamePhase.Running));
            Assert.That(game.ElapsedTime, Is.EqualTo(0f).Within(0.001f));
            Assert.That(game.KillCount, Is.EqualTo(0));
            Assert.That(game.CurrentRun.Gold, Is.EqualTo(0));
        }

        private static List<RoguelikeSurvivalEnemy> GetEnemyList(RoguelikeGame game)
        {
            FieldInfo field = typeof(RoguelikeGame).GetField("_enemies", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            return (List<RoguelikeSurvivalEnemy>)field.GetValue(game);
        }
    }
}
