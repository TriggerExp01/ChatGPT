using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using TEngine;
using UnityEngine;

namespace GameLogic.Tests
{
    public sealed class RoguelikeStage34EffectPrefabTests
    {
        [Test]
        public void Stage34EffectPrefabAssetsExistInEffectCollectorFolder()
        {
            string effectPath = Path.Combine(Application.dataPath, "AssetRaw", "Effects");

            Assert.IsTrue(File.Exists(Path.Combine(effectPath, "Roguelike_EffectPixel.png")));
            Assert.IsTrue(File.Exists(Path.Combine(effectPath, RoguelikeBattleStageView.HitEffectPrefabAddress + ".prefab")));
            Assert.IsTrue(File.Exists(Path.Combine(effectPath, RoguelikeBattleStageView.KillEffectPrefabAddress + ".prefab")));
            Assert.IsTrue(File.Exists(Path.Combine(effectPath, RoguelikeBattleStageView.PickupEffectPrefabAddress + ".prefab")));
        }

        [Test]
        public void CombatEventsEmitHitKillAndPickupEffectCues()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(34001);

            List<RoguelikeSurvivalEnemy> enemies = GetList<RoguelikeSurvivalEnemy>(game, "_enemies");
            List<RoguelikeSurvivalProjectile> projectiles = GetList<RoguelikeSurvivalProjectile>(game, "_projectiles");
            enemies.Clear();
            projectiles.Clear();

            RoguelikeSurvivalEnemy enemy = MemoryPool.Acquire<RoguelikeSurvivalEnemy>();
            enemy.Init(34011, new Vector2(1f, 0f), 2, 1, 0f);
            enemies.Add(enemy);

            RoguelikeSurvivalProjectile projectile = MemoryPool.Acquire<RoguelikeSurvivalProjectile>();
            projectile.Init(34012, RoguelikeWeaponType.MagicBolt, new Vector2(0.9f, 0f), Vector2.right, 0f, 1f, 3);
            projectiles.Add(projectile);

            game.Tick(0.1f);

            AssertCueExists(game, RoguelikeEffectCueType.Hit);

            game.Tick(0.1f);

            AssertCueExists(game, RoguelikeEffectCueType.Kill);

            List<RoguelikeSurvivalPickup> pickups = GetList<RoguelikeSurvivalPickup>(game, "_pickups");
            pickups.Clear();
            RoguelikeSurvivalPickup pickup = MemoryPool.Acquire<RoguelikeSurvivalPickup>();
            pickup.Init(34013, RoguelikePickupType.Experience, new Vector2(0.1f, 0f), 4);
            pickups.Add(pickup);

            game.Tick(0.1f);

            AssertCueExists(game, RoguelikeEffectCueType.Pickup);
        }

        [Test]
        public void StageViewSpawnsRecyclesAndFallsBackWhenEffectPrefabMissing()
        {
            GameObject oldStage = GameObject.Find("Roguelike2DSurvivalStage");
            if (oldStage != null)
            {
                Object.DestroyImmediate(oldStage);
            }

            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(34002);

            List<RoguelikeSurvivalPickup> pickups = GetList<RoguelikeSurvivalPickup>(game, "_pickups");
            pickups.Clear();
            RoguelikeSurvivalPickup pickup = MemoryPool.Acquire<RoguelikeSurvivalPickup>();
            pickup.Init(34021, RoguelikePickupType.Experience, new Vector2(0.1f, 0f), 4);
            pickups.Add(pickup);
            game.Tick(0.1f);

            RoguelikeBattleStageView view = RoguelikeBattleStageView.Ensure();
            view.DebugSetEffectPrefabAddressOverride(RoguelikeEffectCueType.Pickup, "Missing_Roguelike_PickupEffect");
            view.Refresh(game.CurrentRun);

            Assert.That(view.ActiveEffectViewCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(view.EffectFallbackCount, Is.GreaterThanOrEqualTo(1));

            int guard = 0;
            while (view.ActiveEffectViewCount > 0 && guard++ < 32)
            {
                view.Refresh(game.CurrentRun);
            }

            Assert.That(view.ActiveEffectViewCount, Is.EqualTo(0));
            Assert.That(view.EffectViewPoolCount, Is.GreaterThanOrEqualTo(1));
        }

        private static void AssertCueExists(RoguelikeGame game, RoguelikeEffectCueType type)
        {
            for (int i = 0; i < game.EffectCues.Count; i++)
            {
                if (game.EffectCues[i].Type == type)
                {
                    return;
                }
            }

            Assert.Fail($"Missing effect cue: {type}");
        }

        private static List<T> GetList<T>(RoguelikeGame game, string fieldName)
        {
            FieldInfo field = typeof(RoguelikeGame).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            return (List<T>)field.GetValue(game);
        }
    }
}
