using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using TEngine;
using UnityEngine;

namespace GameLogic.Tests
{
    public sealed class RoguelikeStage11FeedbackTests
    {
        [Test]
        public void ProjectileHitTriggersHitFlashAndCameraShake()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(11001);

            List<RoguelikeSurvivalEnemy> enemies = GetList<RoguelikeSurvivalEnemy>(game, "_enemies");
            List<RoguelikeSurvivalProjectile> projectiles = GetList<RoguelikeSurvivalProjectile>(game, "_projectiles");
            RoguelikeSurvivalEnemy enemy = MemoryPool.Acquire<RoguelikeSurvivalEnemy>();
            enemy.Init(11101, new Vector2(1f, 0f), 10, 1, 0f);
            enemies.Add(enemy);

            RoguelikeSurvivalProjectile projectile = MemoryPool.Acquire<RoguelikeSurvivalProjectile>();
            projectile.Init(11102, RoguelikeWeaponType.MagicBolt, new Vector2(0.9f, 0f), Vector2.right, 0f, 1f, 3);
            projectiles.Add(projectile);

            game.Tick(0.1f);

            Assert.Less(enemy.Health, 10);
            Assert.Greater(enemy.HitFlash, 0f);
            Assert.Greater(game.CameraShake, 0f);
        }

        [Test]
        public void PickupCollectionTriggersPulseAndStageFeedbackView()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(11002);

            List<RoguelikeSurvivalPickup> pickups = GetList<RoguelikeSurvivalPickup>(game, "_pickups");
            RoguelikeSurvivalPickup pickup = MemoryPool.Acquire<RoguelikeSurvivalPickup>();
            pickup.Init(11201, RoguelikePickupType.Experience, new Vector2(0.1f, 0f), 4);
            pickups.Add(pickup);

            game.Tick(0.1f);
            RoguelikeBattleStageView view = RoguelikeBattleStageView.Ensure();
            view.Refresh(game.CurrentRun);

            Assert.AreEqual(4, game.Experience);
            Assert.Greater(game.PickupFlash, 0f);
            Assert.Greater(game.CameraShake, 0f);
            Assert.IsTrue(view.PickupFeedbackVisible);
            Assert.Greater(view.LastCameraShakeMagnitude, 0f);
        }

        [Test]
        public void FeedbackTimersDecayDuringRealtimeTick()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(11003);

            List<RoguelikeSurvivalPickup> pickups = GetList<RoguelikeSurvivalPickup>(game, "_pickups");
            RoguelikeSurvivalPickup pickup = MemoryPool.Acquire<RoguelikeSurvivalPickup>();
            pickup.Init(11301, RoguelikePickupType.Experience, new Vector2(0.1f, 0f), 4);
            pickups.Add(pickup);

            game.Tick(0.1f);
            Assert.Greater(game.PickupFlash, 0f);

            game.Tick(0.1f);
            game.Tick(0.1f);

            Assert.AreEqual(0f, game.PickupFlash);
            Assert.AreEqual(0f, game.CameraShake);
        }

        private static List<T> GetList<T>(RoguelikeGame game, string fieldName)
        {
            FieldInfo field = typeof(RoguelikeGame).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field);
            return (List<T>)field.GetValue(game);
        }
    }
}
