using System.IO;
using GameConfig;
using GameConfig.roguelike;
using Luban;
using NUnit.Framework;
using UnityEngine;

namespace GameLogic.Tests
{
    public sealed class RoguelikeStage64StructureSplitTests
    {
        [Test]
        public void Stage64SpawnDirectorSelectsActiveStageWithoutRoguelikeGameState()
        {
            Tables tables = LoadTables();
            RoguelikeSpawnDirector director = new RoguelikeSpawnDirector();

            Assert.That(director.GetActiveSpawnStage(tables, 0f).Id, Is.EqualTo(1));
            Assert.That(director.GetActiveSpawnStage(tables, 179.9f).Id, Is.EqualTo(2));
            Assert.That(director.GetActiveSpawnStage(tables, 180f).Id, Is.EqualTo(3));
            Assert.That(director.GetActiveSpawnStage(tables, 600f).Id, Is.EqualTo(5));
        }

        [Test]
        public void Stage64SpawnDirectorKeepsBossSpawnAsResettablePureRuntimeState()
        {
            Tables tables = LoadTables();
            RoguelikeSpawnDirector director = new RoguelikeSpawnDirector();
            RoguelikeSpawnStage stage5 = tables.TbRoguelikeSpawnStage.Get(5);

            RoguelikeEnemy firstPick = director.PickEnemyConfig(tables, stage5, 21, 600f, new System.Random(64001));
            Assert.NotNull(firstPick);
            Assert.That(firstPick.Id, Is.EqualTo("dungeon_heart"));
            Assert.IsTrue(director.BossSpawned);

            RoguelikeEnemy secondPick = director.PickEnemyConfig(tables, stage5, 21, 600f, new System.Random(64002));
            Assert.NotNull(secondPick);
            Assert.That(secondPick.Id, Is.Not.EqualTo("dungeon_heart"));

            director.Reset();
            RoguelikeEnemy afterResetPick = director.PickEnemyConfig(tables, stage5, 21, 600f, new System.Random(64003));
            Assert.NotNull(afterResetPick);
            Assert.That(afterResetPick.Id, Is.EqualTo("dungeon_heart"));
        }

        [Test]
        public void Stage64SpawnDirectorUsesStageWeightedCommonPool()
        {
            Tables tables = LoadTables();
            RoguelikeSpawnDirector director = new RoguelikeSpawnDirector();
            RoguelikeSpawnStage stage3 = tables.TbRoguelikeSpawnStage.Get(3);

            for (int i = 0; i < 16; i++)
            {
                RoguelikeEnemy enemy = director.PickWeightedEnemy(tables, stage3, new System.Random(64010 + i));
                Assert.NotNull(enemy);
                CollectionAssert.Contains(stage3.CommonEnemyIds, enemy.Id);
            }
        }

        [Test]
        public void Stage64RoguelikeGameStillDelegatesSpawnSelectionThroughExistingPrivateBoundary()
        {
            Tables tables = LoadTables();
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(64004);
            SetConfigTables(game, tables);
            game.DebugSkipRoom(20);

            RoguelikeSpawnStage stage = InvokeGetActiveSpawnStage(game);

            Assert.NotNull(stage);
            Assert.That(stage.Id, Is.EqualTo(5));
            Assert.That(InvokePickEnemyConfig(game, 21, stage).Id, Is.EqualTo("dungeon_heart"));
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

        private static RoguelikeSpawnStage InvokeGetActiveSpawnStage(RoguelikeGame game)
        {
            System.Reflection.MethodInfo method = typeof(RoguelikeGame).GetMethod("GetActiveSpawnStage", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.NotNull(method);
            return (RoguelikeSpawnStage)method.Invoke(game, null);
        }

        private static RoguelikeEnemy InvokePickEnemyConfig(RoguelikeGame game, int wave, RoguelikeSpawnStage spawnStage)
        {
            System.Reflection.MethodInfo method = typeof(RoguelikeGame).GetMethod("PickEnemyConfig", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.NotNull(method);
            return (RoguelikeEnemy)method.Invoke(game, new object[] { wave, spawnStage });
        }

        private static void SetConfigTables(RoguelikeGame game, Tables tables)
        {
            System.Reflection.FieldInfo field = typeof(RoguelikeGame).GetField("_configTables", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.NotNull(field);
            field.SetValue(game, tables);
        }
    }
}
