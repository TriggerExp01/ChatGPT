using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using TEngine;
using UnityEngine;

namespace GameLogic.Tests
{
    public sealed class RoguelikeStage68EnemyReadabilityTests
    {
        [Test]
        public void RuntimeEnemiesResolveReadableRolesFromConfigIdentity()
        {
            Assert.That(InvokeResolveEnemyRole("slime", 22, 1.2f, false), Is.EqualTo(RoguelikeEnemyRole.Common));
            Assert.That(InvokeResolveEnemyRole("wisp", 16, 1.4f, false), Is.EqualTo(RoguelikeEnemyRole.Fast));
            Assert.That(InvokeResolveEnemyRole("mushroom_guard", 38, 0.9f, false), Is.EqualTo(RoguelikeEnemyRole.Tank));
            Assert.That(InvokeResolveEnemyRole("moon_knight", 58, 1.2f, false), Is.EqualTo(RoguelikeEnemyRole.Elite));
            Assert.That(InvokeResolveEnemyRole("dungeon_heart", 160, 0.55f, true), Is.EqualTo(RoguelikeEnemyRole.Boss));
        }

        [Test]
        public void EnemyViewsUseDistinctStage68ColorAndSizeLanguage()
        {
            RoguelikeBattleStageView view = CreateFreshStage();
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(68002);
            MemoryPool.ClearAll();

            List<RoguelikeSurvivalEnemy> enemies = GetEnemyList(game);
            enemies.Clear();
            enemies.Add(CreateEnemy(68021, "slime", false, 22, 1.2f));
            enemies.Add(CreateEnemy(68022, "wisp", false, 16, 1.4f));
            enemies.Add(CreateEnemy(68023, "mushroom_guard", false, 38, 0.9f));
            enemies.Add(CreateEnemy(68024, "moon_knight", false, 58, 1.2f));
            enemies.Add(CreateEnemy(68025, "dungeon_heart", true, 160, 0.55f));

            view.Refresh(game.CurrentRun);

            Dictionary<int, Transform> enemyViews = GetEnemyViewMap(view);
            Transform commonView = enemyViews[68021];
            Transform fastView = enemyViews[68022];
            Transform tankView = enemyViews[68023];
            Transform eliteView = enemyViews[68024];
            Transform bossView = enemyViews[68025];

            Vector2 commonSize = GetRendererSize(commonView);
            Vector2 fastSize = GetRendererSize(fastView);
            Vector2 tankSize = GetRendererSize(tankView);
            Vector2 eliteSize = GetRendererSize(eliteView);
            Vector2 bossSize = GetRendererSize(bossView);

            Assert.That(commonSize.x, Is.EqualTo(0.72f).Within(0.03f));
            Assert.That(fastSize.x, Is.LessThan(commonSize.x - 0.08f));
            Assert.That(tankSize.x, Is.GreaterThan(commonSize.x + 0.10f));
            Assert.That(eliteSize.x, Is.GreaterThan(tankSize.x + 0.06f));
            Assert.That(bossSize.x, Is.GreaterThan(eliteSize.x + 0.15f));

            Color commonColor = commonView.GetComponent<SpriteRenderer>().color;
            Color fastColor = fastView.GetComponent<SpriteRenderer>().color;
            Color tankColor = tankView.GetComponent<SpriteRenderer>().color;
            Color eliteColor = eliteView.GetComponent<SpriteRenderer>().color;
            Color bossColor = bossView.GetComponent<SpriteRenderer>().color;

            Assert.That(commonColor.r, Is.GreaterThan(commonColor.b));
            Assert.That(fastColor.b, Is.GreaterThan(fastColor.r + 0.5f));
            Assert.That(tankColor.g, Is.GreaterThan(commonColor.g + 0.3f));
            Assert.That(eliteColor.b, Is.GreaterThan(eliteColor.r + 0.3f));
            Assert.That(bossColor.b, Is.GreaterThan(bossColor.r));
            Assert.That(bossColor.r, Is.GreaterThan(eliteColor.r));
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

        private static RoguelikeSurvivalEnemy CreateEnemy(int id, string configId, bool isBoss, int health, float moveSpeed)
        {
            RoguelikeSurvivalEnemy enemy = MemoryPool.Acquire<RoguelikeSurvivalEnemy>();
            enemy.Init(id, Vector2.zero, health, 1, moveSpeed, true, configId, isBoss, ResolveRole(configId, isBoss, health, moveSpeed));
            return enemy;
        }

        private static RoguelikeEnemyRole ResolveRole(string configId, bool isBoss, int health, float moveSpeed)
        {
            if (isBoss || configId == "dungeon_heart")
            {
                return RoguelikeEnemyRole.Boss;
            }

            if (configId == "moon_knight")
            {
                return RoguelikeEnemyRole.Elite;
            }

            if (configId == "mushroom_guard" || health >= 34)
            {
                return RoguelikeEnemyRole.Tank;
            }

            if (configId == "wisp" || moveSpeed >= 1.55f)
            {
                return RoguelikeEnemyRole.Fast;
            }

            return RoguelikeEnemyRole.Common;
        }

        private static RoguelikeEnemyRole InvokeResolveEnemyRole(string configId, int health, float moveSpeed, bool isBoss)
        {
            MethodInfo method = typeof(RoguelikeGame).GetMethod("ResolveEnemyRole", BindingFlags.Static | BindingFlags.NonPublic, null, new[]
            {
                typeof(string),
                typeof(GameConfig.roguelike.EEnemyTier?),
                typeof(int),
                typeof(float),
                typeof(bool),
            }, null);
            Assert.NotNull(method);
            return (RoguelikeEnemyRole)method.Invoke(null, new object[]
            {
                configId,
                null,
                health,
                moveSpeed,
                isBoss,
            });
        }

        private static List<RoguelikeSurvivalEnemy> GetEnemyList(RoguelikeGame game)
        {
            FieldInfo field = typeof(RoguelikeGame).GetField("_enemies", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            return (List<RoguelikeSurvivalEnemy>)field.GetValue(game);
        }

        private static Dictionary<int, Transform> GetEnemyViewMap(RoguelikeBattleStageView view)
        {
            FieldInfo field = typeof(RoguelikeBattleStageView).GetField("_enemyViews", BindingFlags.Instance | BindingFlags.NonPublic);
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
