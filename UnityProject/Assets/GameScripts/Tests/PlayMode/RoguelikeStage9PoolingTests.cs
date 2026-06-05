using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using TEngine;
using UnityEngine;

namespace GameLogic.Tests
{
    public sealed class RoguelikeStage9PoolingTests
    {
        [Test]
        public void SurvivalRuntimeObjectsReturnToTEngineMemoryPoolOnRunReset()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(9001);
            MemoryPool.ClearAll();

            List<RoguelikeSurvivalEnemy> enemies = GetList<RoguelikeSurvivalEnemy>(game, "_enemies");
            List<RoguelikeSurvivalPickup> pickups = GetList<RoguelikeSurvivalPickup>(game, "_pickups");
            List<RoguelikeSurvivalProjectile> projectiles = GetList<RoguelikeSurvivalProjectile>(game, "_projectiles");

            enemies.Add(CreateEnemy(9901));
            pickups.Add(CreatePickup(9902));
            projectiles.Add(CreateProjectile(9903));

            game.StartNewRun(9002);

            AssertMemoryPoolReleased<RoguelikeSurvivalEnemy>();
            AssertMemoryPoolReleased<RoguelikeSurvivalPickup>();
            AssertMemoryPoolReleased<RoguelikeSurvivalProjectile>();
            Assert.AreEqual(0, game.Enemies.Count);
            Assert.AreEqual(0, game.Pickups.Count);
            Assert.AreEqual(0, game.Projectiles.Count);
        }

        [Test]
        public void BattleStageReusesTransientViewsAfterEntitiesDisappear()
        {
            GameObject oldStage = GameObject.Find("Roguelike2DSurvivalStage");
            if (oldStage != null)
            {
                UnityEngine.Object.DestroyImmediate(oldStage);
            }

            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(9003);
            MemoryPool.ClearAll();

            RoguelikeBattleStageView view = RoguelikeBattleStageView.Ensure();
            List<RoguelikeSurvivalEnemy> enemies = GetList<RoguelikeSurvivalEnemy>(game, "_enemies");
            List<RoguelikeSurvivalPickup> pickups = GetList<RoguelikeSurvivalPickup>(game, "_pickups");
            List<RoguelikeSurvivalProjectile> projectiles = GetList<RoguelikeSurvivalProjectile>(game, "_projectiles");

            RoguelikeSurvivalEnemy firstEnemy = CreateEnemy(9911);
            RoguelikeSurvivalPickup firstPickup = CreatePickup(9912);
            RoguelikeSurvivalProjectile firstProjectile = CreateProjectile(9913);
            enemies.Add(firstEnemy);
            pickups.Add(firstPickup);
            projectiles.Add(firstProjectile);
            view.Refresh(game.CurrentRun);

            ReleaseAndClear(enemies, pickups, projectiles);
            view.Refresh(game.CurrentRun);

            Assert.GreaterOrEqual(view.EnemyViewPoolCount, 1);
            Assert.GreaterOrEqual(view.PickupViewPoolCount, 1);
            Assert.GreaterOrEqual(view.ProjectileViewPoolCount, 1);

            int reuseBefore = view.ViewReuseCount;
            enemies.Add(CreateEnemy(9921));
            pickups.Add(CreatePickup(9922));
            projectiles.Add(CreateProjectile(9923));
            view.Refresh(game.CurrentRun);

            Assert.GreaterOrEqual(view.ViewReuseCount, reuseBefore + 3);
            Assert.AreEqual(0, view.EnemyViewPoolCount);
            Assert.AreEqual(0, view.PickupViewPoolCount);
            Assert.AreEqual(0, view.ProjectileViewPoolCount);

            ReleaseAndClear(enemies, pickups, projectiles);
            view.Refresh(game.CurrentRun);
        }

        private static RoguelikeSurvivalEnemy CreateEnemy(int id)
        {
            RoguelikeSurvivalEnemy enemy = MemoryPool.Acquire<RoguelikeSurvivalEnemy>();
            enemy.Init(id, new Vector2(2f, 0f), 10, 1, 0f);
            return enemy;
        }

        private static RoguelikeSurvivalPickup CreatePickup(int id)
        {
            RoguelikeSurvivalPickup pickup = MemoryPool.Acquire<RoguelikeSurvivalPickup>();
            pickup.Init(id, RoguelikePickupType.Experience, new Vector2(0.5f, 0f), 4);
            return pickup;
        }

        private static RoguelikeSurvivalProjectile CreateProjectile(int id)
        {
            RoguelikeSurvivalProjectile projectile = MemoryPool.Acquire<RoguelikeSurvivalProjectile>();
            projectile.Init(id, RoguelikeWeaponType.MagicBolt, Vector2.zero, Vector2.right, 9f, 5f, 12);
            return projectile;
        }

        private static void ReleaseAndClear(
            List<RoguelikeSurvivalEnemy> enemies,
            List<RoguelikeSurvivalPickup> pickups,
            List<RoguelikeSurvivalProjectile> projectiles)
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                MemoryPool.Release(enemies[i]);
            }

            for (int i = 0; i < pickups.Count; i++)
            {
                MemoryPool.Release(pickups[i]);
            }

            for (int i = 0; i < projectiles.Count; i++)
            {
                MemoryPool.Release(projectiles[i]);
            }

            enemies.Clear();
            pickups.Clear();
            projectiles.Clear();
        }

        private static void AssertMemoryPoolReleased<T>()
        {
            MemoryPoolInfo info = FindMemoryPoolInfo(typeof(T));
            Assert.GreaterOrEqual(info.ReleaseMemoryCount, 1);
            Assert.GreaterOrEqual(info.UnusedMemoryCount, 1);
            Assert.AreEqual(0, info.UsingMemoryCount);
        }

        private static MemoryPoolInfo FindMemoryPoolInfo(Type type)
        {
            MemoryPoolInfo[] infos = MemoryPool.GetAllMemoryPoolInfos();
            for (int i = 0; i < infos.Length; i++)
            {
                if (infos[i].Type == type)
                {
                    return infos[i];
                }
            }

            Assert.Fail($"MemoryPoolInfo not found for {type.Name}.");
            return default;
        }

        private static List<T> GetList<T>(RoguelikeGame game, string fieldName)
        {
            FieldInfo field = typeof(RoguelikeGame).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            return (List<T>)field.GetValue(game);
        }
    }
}
