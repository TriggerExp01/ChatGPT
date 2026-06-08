using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using TEngine;
using UnityEngine;

namespace GameLogic.Tests
{
    public sealed class RoguelikeStage35PrefabPresentationTests
    {
        [Test]
        public void Stage35ActorPrefabAssetsExistInActorCollectorFolder()
        {
            string actorPath = Path.Combine(Application.dataPath, "AssetRaw", "Actor");

            Assert.IsTrue(File.Exists(Path.Combine(actorPath, RoguelikeBattleStageView.CommonEnemyPrefabAddress + ".prefab")));
            Assert.IsTrue(File.Exists(Path.Combine(actorPath, RoguelikeBattleStageView.BossEnemyPrefabAddress + ".prefab")));
            Assert.IsTrue(File.Exists(Path.Combine(actorPath, RoguelikeBattleStageView.MagicBoltProjectilePrefabAddress + ".prefab")));
            Assert.IsTrue(File.Exists(Path.Combine(actorPath, RoguelikeBattleStageView.PiercingDartProjectilePrefabAddress + ".prefab")));
        }

        [Test]
        public void Stage35EnemyAndProjectileViewsFallbackWhenPrefabAddressMissing()
        {
            RoguelikeBattleStageView view = CreateFreshStage();
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(35001);
            MemoryPool.ClearAll();

            List<RoguelikeSurvivalEnemy> enemies = GetList<RoguelikeSurvivalEnemy>(game, "_enemies");
            List<RoguelikeSurvivalProjectile> projectiles = GetList<RoguelikeSurvivalProjectile>(game, "_projectiles");
            enemies.Clear();
            projectiles.Clear();

            view.DebugSetEnemyPrefabAddressOverride(false, "Missing_Roguelike_Enemy_Common");
            view.DebugSetProjectilePrefabAddressOverride(RoguelikeWeaponType.MagicBolt, "Missing_Roguelike_Projectile_MagicBolt");
            enemies.Add(CreateEnemy(35011, false, "slime", new Vector2(-1f, 0f), 20));
            projectiles.Add(CreateProjectile(35012, RoguelikeWeaponType.MagicBolt, new Vector2(0.5f, 0f), Vector2.right));

            view.Refresh(game.CurrentRun);

            Dictionary<int, Transform> enemyViews = GetViewMap("_enemyViews", view);
            Dictionary<int, Transform> projectileViews = GetViewMap("_projectileViews", view);
            Assert.That(enemyViews[35011].GetComponent<SpriteRenderer>(), Is.Not.Null);
            Assert.That(projectileViews[35012].GetComponent<SpriteRenderer>(), Is.Not.Null);
            Assert.That(view.PresentationFallbackCount, Is.GreaterThanOrEqualTo(2));
        }

        [Test]
        public void Stage35PresentationSyncsBossScaleProjectileRotationAndPoolReuse()
        {
            RoguelikeBattleStageView view = CreateFreshStage();
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(35002);
            MemoryPool.ClearAll();

            List<RoguelikeSurvivalEnemy> enemies = GetList<RoguelikeSurvivalEnemy>(game, "_enemies");
            List<RoguelikeSurvivalProjectile> projectiles = GetList<RoguelikeSurvivalProjectile>(game, "_projectiles");
            enemies.Clear();
            projectiles.Clear();

            RoguelikeSurvivalEnemy common = CreateEnemy(35021, false, "slime", new Vector2(-1f, 0f), 20);
            RoguelikeSurvivalEnemy boss = CreateEnemy(35022, true, "dungeon_heart", new Vector2(1f, 0f), 120);
            RoguelikeSurvivalProjectile bolt = CreateProjectile(35023, RoguelikeWeaponType.MagicBolt, new Vector2(0f, 0f), Vector2.right);
            RoguelikeSurvivalProjectile dart = CreateProjectile(35024, RoguelikeWeaponType.PiercingDart, new Vector2(0f, 0.6f), Vector2.up);
            enemies.Add(common);
            enemies.Add(boss);
            projectiles.Add(bolt);
            projectiles.Add(dart);

            view.Refresh(game.CurrentRun);

            Dictionary<int, Transform> enemyViews = GetViewMap("_enemyViews", view);
            Dictionary<int, Transform> projectileViews = GetViewMap("_projectileViews", view);
            Transform commonView = enemyViews[common.Id];
            Transform bossView = enemyViews[boss.Id];
            Transform boltView = projectileViews[bolt.Id];
            Transform dartView = projectileViews[dart.Id];
            Vector2 commonSize = GetRendererSize(commonView);
            Vector2 bossSize = GetRendererSize(bossView);
            Vector2 boltSize = GetRendererSize(boltView);
            Vector2 dartSize = GetRendererSize(dartView);

            Assert.That(commonSize.x, Is.EqualTo(0.72f).Within(0.03f));
            Assert.That(commonSize.y, Is.EqualTo(0.72f).Within(0.03f));
            Assert.That(bossSize.x, Is.EqualTo(1.18f).Within(0.04f));
            Assert.That(bossSize.y, Is.EqualTo(1.18f).Within(0.04f));
            Assert.That(bossSize.x, Is.GreaterThan(commonSize.x + 0.4f));
            Assert.That(bossView.GetComponent<SpriteRenderer>().color.b, Is.GreaterThan(commonView.GetComponent<SpriteRenderer>().color.b));
            Assert.That(boltSize.x, Is.EqualTo(0.28f).Within(0.03f));
            Assert.That(boltSize.y, Is.EqualTo(0.14f).Within(0.03f));
            Assert.That(Mathf.Max(dartSize.x, dartSize.y), Is.EqualTo(0.44f).Within(0.03f));
            Assert.That(Mathf.Min(dartSize.x, dartSize.y), Is.EqualTo(0.09f).Within(0.03f));
            Assert.That(dartView.localRotation.eulerAngles.z, Is.EqualTo(90f).Within(0.001f));

            common.Health = 1;
            boss.Health = 1;
            view.Refresh(game.CurrentRun);

            Assert.That(GetRendererSize(commonView).x, Is.EqualTo(commonSize.x).Within(0.001f));
            Assert.That(GetRendererSize(bossView).x, Is.EqualTo(bossSize.x).Within(0.001f));

            enemies.Clear();
            projectiles.Clear();
            view.Refresh(game.CurrentRun);

            Assert.That(view.EnemyViewPoolCount, Is.GreaterThanOrEqualTo(2));
            Assert.That(view.ProjectileViewPoolCount, Is.GreaterThanOrEqualTo(2));

            int reuseBefore = view.ViewReuseCount;
            enemies.Add(CreateEnemy(35031, false, "slime", new Vector2(-0.4f, 0f), 20));
            projectiles.Add(CreateProjectile(35032, RoguelikeWeaponType.MagicBolt, new Vector2(0.4f, 0f), Vector2.right));
            view.Refresh(game.CurrentRun);

            Assert.That(view.ViewReuseCount, Is.GreaterThanOrEqualTo(reuseBefore + 2));
            Dictionary<int, Transform> reusedEnemyViews = GetViewMap("_enemyViews", view);
            Dictionary<int, Transform> reusedProjectileViews = GetViewMap("_projectileViews", view);
            Assert.That(reusedEnemyViews[35031], Is.SameAs(commonView));
            Assert.That(reusedProjectileViews[35032], Is.SameAs(boltView));
            Assert.That(reusedEnemyViews[35031], Is.Not.SameAs(bossView));
            Assert.That(reusedProjectileViews[35032], Is.Not.SameAs(dartView));
            Assert.That(GetRendererSize(reusedEnemyViews[35031]).x, Is.EqualTo(commonSize.x).Within(0.001f));
            Assert.That(GetRendererSize(reusedProjectileViews[35032]).x, Is.EqualTo(0.28f).Within(0.03f));
        }

        private static RoguelikeBattleStageView CreateFreshStage()
        {
            GameObject oldStage = GameObject.Find("Roguelike2DSurvivalStage");
            if (oldStage != null)
            {
                Object.DestroyImmediate(oldStage);
            }

            return RoguelikeBattleStageView.Ensure();
        }

        private static RoguelikeSurvivalEnemy CreateEnemy(int id, bool isBoss, string configId, Vector2 position, int health)
        {
            RoguelikeSurvivalEnemy enemy = MemoryPool.Acquire<RoguelikeSurvivalEnemy>();
            enemy.Init(id, position, health, 1, 0f, true, configId, isBoss);
            return enemy;
        }

        private static RoguelikeSurvivalProjectile CreateProjectile(int id, RoguelikeWeaponType weaponType, Vector2 position, Vector2 direction)
        {
            RoguelikeSurvivalProjectile projectile = MemoryPool.Acquire<RoguelikeSurvivalProjectile>();
            projectile.Init(id, weaponType, position, direction, 0f, 5f, 12);
            return projectile;
        }

        private static List<T> GetList<T>(RoguelikeGame game, string fieldName)
        {
            FieldInfo field = typeof(RoguelikeGame).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            return (List<T>)field.GetValue(game);
        }

        private static Dictionary<int, Transform> GetViewMap(string fieldName, RoguelikeBattleStageView view)
        {
            FieldInfo field = typeof(RoguelikeBattleStageView).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            return (Dictionary<int, Transform>)field.GetValue(view);
        }

        private static Vector2 GetRendererSize(Transform view)
        {
            SpriteRenderer[] renderers = view.GetComponentsInChildren<SpriteRenderer>(true);
            Assert.That(renderers.Length, Is.GreaterThan(0));

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return new Vector2(bounds.size.x, bounds.size.y);
        }
    }
}
