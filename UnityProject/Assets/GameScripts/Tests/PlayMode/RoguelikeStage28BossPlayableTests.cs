using System.Collections.Generic;
using System.IO;
using System.Reflection;
using GameConfig;
using GameConfig.roguelike;
using Luban;
using NUnit.Framework;
using TEngine;
using UnityEngine;

namespace GameLogic.Tests
{
    public sealed class RoguelikeStage28BossPlayableTests
    {
        private const string MetaGoldKey = "Roguelike.MetaGold";

        [SetUp]
        public void ClearMetaGold()
        {
            PlayerPrefs.DeleteKey(MetaGoldKey);
            PlayerPrefs.Save();
        }

        [Test]
        public void BossSpawnHasReadableCueAndBossIdentity()
        {
            Tables tables = LoadTables();
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(28001);
            SetConfigTables(game, tables);
            game.DebugSkipRoom(20);
            SetSpawnTimer(game, 0f);

            game.Tick(0.1f);

            Assert.That(game.Enemies.Count, Is.GreaterThanOrEqualTo(1));
            RoguelikeSurvivalEnemy boss = FindBoss(game.Enemies);
            Assert.NotNull(boss);
            Assert.That(boss.ConfigId, Is.EqualTo("dungeon_heart"));
            Assert.That(boss.MaxHealth, Is.GreaterThanOrEqualTo(tables.TbRoguelikeEnemy.Get("dungeon_heart").MaxHealth));
            Assert.That(game.LastMessage, Does.Contain("地牢之心出现"));
            Assert.That(game.CameraShake, Is.GreaterThan(0.16f));
        }

        [Test]
        public void BossViewUsesDistinctScaleAndColor()
        {
            GameObject oldStage = GameObject.Find("Roguelike2DSurvivalStage");
            if (oldStage != null)
            {
                Object.DestroyImmediate(oldStage);
            }

            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(28002);
            MemoryPool.ClearAll();

            List<RoguelikeSurvivalEnemy> enemies = GetEnemyList(game);
            enemies.Clear();
            RoguelikeSurvivalEnemy common = MemoryPool.Acquire<RoguelikeSurvivalEnemy>();
            common.Init(28021, new Vector2(-1f, 0f), 20, 1, 0f, true, "slime", false);
            RoguelikeSurvivalEnemy boss = MemoryPool.Acquire<RoguelikeSurvivalEnemy>();
            boss.Init(28022, new Vector2(1f, 0f), 120, 10, 0f, true, "dungeon_heart", true);
            enemies.Add(common);
            enemies.Add(boss);

            RoguelikeBattleStageView view = RoguelikeBattleStageView.Ensure();
            view.Refresh(game.CurrentRun);

            Dictionary<int, Transform> enemyViews = GetEnemyViewMap(view);
            Transform commonView = enemyViews[common.Id];
            Transform bossView = enemyViews[boss.Id];
            Color commonColor = commonView.GetComponent<SpriteRenderer>().color;
            Color bossColor = bossView.GetComponent<SpriteRenderer>().color;

            Assert.That(bossView.localScale.x, Is.GreaterThan(commonView.localScale.x + 0.4f));
            Assert.That(bossColor.b, Is.GreaterThan(commonColor.b));
            Assert.That(bossColor.r, Is.LessThan(commonColor.r));
        }

        [Test]
        public void BossKillGrantsRewardAndVictorySettlement()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(28003);
            game.CurrentRun.AddGold(6);

            List<RoguelikeSurvivalEnemy> enemies = GetEnemyList(game);
            enemies.Clear();
            RoguelikeSurvivalEnemy boss = MemoryPool.Acquire<RoguelikeSurvivalEnemy>();
            boss.Init(28031, new Vector2(1f, 0f), 1, 1, 0f, true, "dungeon_heart", true);
            boss.Health = 0;
            enemies.Add(boss);

            game.Tick(0.1f);

            Assert.That(game.Phase, Is.EqualTo(RoguelikeGamePhase.Victory));
            Assert.That(game.CurrentRun.Gold, Is.EqualTo(31));
            Assert.That(game.MetaGold, Is.EqualTo(31));
            Assert.That(game.LastMessage, Does.Contain("额外获得 25 金币"));
            Assert.That(game.SettlementSummary, Does.Contain("永久金币 31"));
        }

        private static Tables LoadTables()
        {
            string configPath = Path.Combine(Application.dataPath, "AssetRaw", "Configs", "bytes");
            return new Tables(file =>
            {
                string path = Path.Combine(configPath, file + ".bytes");
                Assert.IsTrue(File.Exists(path), $"Config bytes not found: {path}");
                return new ByteBuf(File.ReadAllBytes(path));
            });
        }

        private static RoguelikeSurvivalEnemy FindBoss(IReadOnlyList<RoguelikeSurvivalEnemy> enemies)
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                if (enemies[i].IsBoss)
                {
                    return enemies[i];
                }
            }

            return null;
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

        private static void SetConfigTables(RoguelikeGame game, Tables tables)
        {
            FieldInfo field = typeof(RoguelikeGame).GetField("_configTables", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            field.SetValue(game, tables);
        }

        private static void SetSpawnTimer(RoguelikeGame game, float value)
        {
            FieldInfo field = typeof(RoguelikeGame).GetField("_spawnTimer", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            field.SetValue(game, value);
        }
    }
}
