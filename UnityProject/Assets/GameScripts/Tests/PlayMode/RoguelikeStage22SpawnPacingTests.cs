using System.Collections.Generic;
using System.IO;
using System.Reflection;
using GameConfig;
using GameConfig.roguelike;
using Luban;
using NUnit.Framework;
using UnityEngine;

namespace GameLogic.Tests
{
    public sealed class RoguelikeStage22SpawnPacingTests
    {
        [Test]
        public void Stage22SpawnStagesCoverTenMinutePacing()
        {
            Tables tables = LoadTables();

            Assert.That(tables.TbRoguelikeSpawnStage.DataList.Count, Is.GreaterThanOrEqualTo(5));
            AssertStage(tables.TbRoguelikeSpawnStage.Get(1), 0f, 1, 9999f, 9999f, string.Empty);
            AssertStage(tables.TbRoguelikeSpawnStage.Get(2), 60f, 2, 120f, 9999f, string.Empty);
            AssertStage(tables.TbRoguelikeSpawnStage.Get(3), 180f, 3, 210f, 9999f, string.Empty);
            AssertStage(tables.TbRoguelikeSpawnStage.Get(4), 360f, 4, 390f, 9999f, string.Empty);
            AssertStage(tables.TbRoguelikeSpawnStage.Get(5), 540f, 5, 540f, 600f, "dungeon_heart");
        }

        [Test]
        public void Stage22RuntimeSelectsActiveStageByElapsedTime()
        {
            Tables tables = LoadTables();
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(22001);
            SetConfigTables(game, tables);

            AssertActiveStage(game, 1);
            game.DebugSkipRoom(2);
            AssertActiveStage(game, 2);
            game.DebugSkipRoom(4);
            AssertActiveStage(game, 3);
            game.DebugSkipRoom(6);
            AssertActiveStage(game, 4);
            game.DebugSkipRoom(6);
            AssertActiveStage(game, 5);
        }

        [Test]
        public void Stage22BossNodeSpawnsOnceAndThenFallsBackToStagePool()
        {
            Tables tables = LoadTables();
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(22002);
            SetConfigTables(game, tables);
            game.DebugSkipRoom(20);

            RoguelikeSpawnStage stage5 = InvokeGetActiveSpawnStage(game);
            Assert.NotNull(stage5);
            Assert.That(stage5.Id, Is.EqualTo(5));

            RoguelikeEnemy firstPick = InvokePickEnemyConfig(game, 21, stage5);
            Assert.NotNull(firstPick);
            Assert.That(firstPick.Id, Is.EqualTo("dungeon_heart"));

            for (int i = 0; i < 8; i++)
            {
                RoguelikeEnemy nextPick = InvokePickEnemyConfig(game, 21, stage5);
                Assert.NotNull(nextPick);
                Assert.That(nextPick.Id, Is.Not.EqualTo("dungeon_heart"));
            }
        }

        [Test]
        public void Stage22PacingDoesNotBreakDefeatVictoryAndRestartFlow()
        {
            Tables tables = LoadTables();
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(22003);
            SetConfigTables(game, tables);
            game.DebugSkipRoom(20);
            AssertActiveStage(game, 5);

            game.DebugForceVictory();
            Assert.That(game.Phase, Is.EqualTo(RoguelikeGamePhase.Victory));

            game.StartNewRun(22004);
            SetConfigTables(game, tables);
            Assert.That(game.Phase, Is.EqualTo(RoguelikeGamePhase.Running));
            Assert.That(game.ElapsedTime, Is.EqualTo(0f).Within(0.001f));
            AssertActiveStage(game, 1);

            game.DebugForceDefeat();
            Assert.That(game.Phase, Is.EqualTo(RoguelikeGamePhase.Defeated));
            Assert.That(game.SettlementSummary, Does.Contain("本局结束"));
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

        private static void AssertStage(RoguelikeSpawnStage stage, float startTime, int spawnCount, float eliteStartTime, float bossStartTime, string bossId)
        {
            Assert.NotNull(stage);
            Assert.That(stage.StartTime, Is.EqualTo(startTime));
            Assert.That(stage.CommonSpawnCount, Is.EqualTo(spawnCount));
            Assert.That(stage.CommonEnemyIds.Count, Is.EqualTo(stage.CommonWeights.Count));
            Assert.That(stage.EliteStartTime, Is.EqualTo(eliteStartTime));
            Assert.That(stage.BossStartTime, Is.EqualTo(bossStartTime));
            Assert.That(stage.BossEnemyId ?? string.Empty, Is.EqualTo(bossId));
        }

        private static void AssertActiveStage(RoguelikeGame game, int expectedStageId)
        {
            RoguelikeSpawnStage active = InvokeGetActiveSpawnStage(game);
            Assert.NotNull(active);
            Assert.That(active.Id, Is.EqualTo(expectedStageId));
        }

        private static RoguelikeSpawnStage InvokeGetActiveSpawnStage(RoguelikeGame game)
        {
            MethodInfo method = typeof(RoguelikeGame).GetMethod("GetActiveSpawnStage", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(method);
            return (RoguelikeSpawnStage)method.Invoke(game, null);
        }

        private static RoguelikeEnemy InvokePickEnemyConfig(RoguelikeGame game, int wave, RoguelikeSpawnStage spawnStage)
        {
            MethodInfo method = typeof(RoguelikeGame).GetMethod("PickEnemyConfig", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(method);
            return (RoguelikeEnemy)method.Invoke(game, new object[] { wave, spawnStage });
        }

        private static void SetConfigTables(RoguelikeGame game, Tables tables)
        {
            FieldInfo field = typeof(RoguelikeGame).GetField("_configTables", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            field.SetValue(game, tables);
        }
    }
}
