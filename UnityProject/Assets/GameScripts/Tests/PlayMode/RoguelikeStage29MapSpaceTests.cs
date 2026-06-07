using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using TEngine;
using UnityEngine;

namespace GameLogic.Tests
{
    public sealed class RoguelikeStage29MapSpaceTests
    {
        [Test]
        public void PlayerAndEnemyPositionsAreResolvedOutsideFixedObstacles()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(29001);
            RoguelikeArenaObstacle obstacle = game.ArenaObstacles[0];

            game.SyncPlayerPosition(obstacle.Center);

            Assert.IsFalse(obstacle.Contains(game.PlayerPosition, 0.48f));

            List<RoguelikeSurvivalEnemy> enemies = GetList<RoguelikeSurvivalEnemy>(game, "_enemies");
            enemies.Clear();
            RoguelikeSurvivalEnemy enemy = MemoryPool.Acquire<RoguelikeSurvivalEnemy>();
            enemy.Init(29011, obstacle.Center, 30, 1, 1.5f, true, "slime", false);
            enemies.Add(enemy);

            game.Tick(0.1f);

            Assert.IsFalse(obstacle.Contains(enemy.Position, 0.42f));
        }

        [Test]
        public void ProjectilesAreRemovedWhenTheyHitFixedObstacles()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(29002);
            RoguelikeArenaObstacle obstacle = game.ArenaObstacles[0];

            List<RoguelikeSurvivalProjectile> projectiles = GetList<RoguelikeSurvivalProjectile>(game, "_projectiles");
            projectiles.Clear();
            RoguelikeSurvivalProjectile projectile = MemoryPool.Acquire<RoguelikeSurvivalProjectile>();
            Vector2 start = new Vector2(obstacle.Min.x - 0.05f, obstacle.Center.y);
            projectile.Init(29021, RoguelikeWeaponType.MagicBolt, start, Vector2.right, 2f, 5f, 10);
            projectiles.Add(projectile);

            game.Tick(0.1f);

            Assert.That(game.Projectiles.Count, Is.EqualTo(0));
        }

        [Test]
        public void BattleStageDrawsConfiguredObstacleViews()
        {
            GameObject oldStage = GameObject.Find("Roguelike2DSurvivalStage");
            if (oldStage != null)
            {
                Object.DestroyImmediate(oldStage);
            }

            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(29003);

            RoguelikeBattleStageView view = RoguelikeBattleStageView.Ensure();
            view.Refresh(game.CurrentRun);

            List<Transform> obstacleViews = GetObstacleViews(view);
            Assert.That(obstacleViews.Count, Is.EqualTo(game.ArenaObstacles.Count));
            Assert.That(obstacleViews[0].name, Does.Contain("障碍"));
            Assert.That(obstacleViews[0].localScale.x, Is.EqualTo(game.ArenaObstacles[0].Size.x).Within(0.001f));
        }

        private static List<T> GetList<T>(RoguelikeGame game, string fieldName)
        {
            FieldInfo field = typeof(RoguelikeGame).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            return (List<T>)field.GetValue(game);
        }

        private static List<Transform> GetObstacleViews(RoguelikeBattleStageView view)
        {
            FieldInfo field = typeof(RoguelikeBattleStageView).GetField("_obstacleViews", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            return (List<Transform>)field.GetValue(view);
        }
    }
}
