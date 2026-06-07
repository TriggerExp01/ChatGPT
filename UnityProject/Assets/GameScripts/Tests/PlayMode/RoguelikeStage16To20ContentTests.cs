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
    public sealed class RoguelikeStage16To20ContentTests
    {
        [Test]
        public void Stage16TablesContainCleanTextAndNewStructures()
        {
            Tables tables = LoadTables();

            Assert.GreaterOrEqual(tables.TbRoguelikeWeapon.DataList.Count, 3);
            Assert.GreaterOrEqual(tables.TbRoguelikeSpawnStage.DataList.Count, 3);
            Assert.That(tables.TbRoguelikeEnemy.Get("slime").DisplayName, Is.EqualTo("黏液怪"));
            Assert.That(tables.TbRoguelikeChoice.Get("atk_2").Title, Is.EqualTo("磨砺锋刃"));
            Assert.That(tables.TbRoguelikeWeapon.Get("piercing_dart").DisplayName, Is.EqualTo("穿透飞镖"));
        }

        [Test]
        public void Stage17SpawnStageCanDriveRuntimeSpawning()
        {
            Tables tables = LoadTables();
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(17001);
            SetConfigTables(game, tables);
            game.DebugSkipRoom(18);

            RoguelikeSpawnStage stage = InvokeGetActiveSpawnStage(game);

            Assert.NotNull(stage);
            Assert.AreEqual(5, stage.Id);
            Assert.AreEqual(5, stage.CommonSpawnCount);
            Assert.That(stage.BossEnemyId, Is.EqualTo("dungeon_heart"));
        }

        [Test]
        public void Stage18PiercingDartUnlocksAndCanHitMultipleEnemies()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(18001);
            SetConfigTables(game, LoadTables());
            InvokeAddOrUpgradeWeapon(game, RoguelikeWeaponType.PiercingDart);

            List<RoguelikeSurvivalEnemy> enemies = GetEnemyList(game);
            enemies.Clear();
            enemies.Add(new RoguelikeSurvivalEnemy(18001, new Vector2(1.0f, 0f), 40, 1, 0f));
            enemies.Add(new RoguelikeSurvivalEnemy(18002, new Vector2(1.5f, 0f), 40, 1, 0f));

            game.Tick(0.1f);
            game.Tick(0.05f);

            AssertWeaponLevel(game, RoguelikeWeaponType.PiercingDart, 1);
            Assert.Less(enemies[0].Health, 40);
            Assert.Less(enemies[1].Health, 40);
            Assert.That(game.WeaponSummary, Does.Contain("穿透飞镖"));
        }

        [Test]
        public void Stage19ConfiguredRuntimeEffectsAffectGameFields()
        {
            Tables tables = LoadTables();
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(19001);
            SetConfigTables(game, tables);

            List<RoguelikeChoiceOption> pool = BuildPassiveChoices(game);
            float pickupRadius = game.PickupAttractRadius;
            float moveSpeed = game.MoveSpeed;

            FindChoice(pool, "passive_magnet_core").Apply.Invoke(game.CurrentRun);
            FindChoice(pool, "passive_wind_boots").Apply.Invoke(game.CurrentRun);

            Assert.Greater(game.PickupAttractRadius, pickupRadius);
            Assert.Greater(game.MoveSpeed, moveSpeed);
            Assert.That(game.PassiveSummary, Does.Contain("磁力核心"));
            Assert.That(game.PassiveSummary, Does.Contain("疾风靴"));
        }

        [Test]
        public void Stage20MinimalAudioAssetsExist()
        {
            string audioPath = Path.Combine(Application.dataPath, "AssetRaw", "Audios");

            Assert.IsTrue(File.Exists(Path.Combine(audioPath, "Roguelike_Hit.wav")));
            Assert.IsTrue(File.Exists(Path.Combine(audioPath, "Roguelike_Pickup.wav")));
            Assert.IsTrue(File.Exists(Path.Combine(audioPath, "Roguelike_LevelUp.wav")));
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

        private static List<RoguelikeChoiceOption> BuildPassiveChoices(RoguelikeGame game)
        {
            List<RoguelikeChoiceOption> pool = new List<RoguelikeChoiceOption>();
            MethodInfo method = typeof(RoguelikeGame).GetMethod("AddPassiveChoices", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(method);
            method.Invoke(game, new object[] { pool });
            return pool;
        }

        private static RoguelikeChoiceOption FindChoice(List<RoguelikeChoiceOption> choices, string id)
        {
            for (int i = 0; i < choices.Count; i++)
            {
                if (choices[i].Id == id)
                {
                    return choices[i];
                }
            }

            Assert.Fail($"Choice {id} not found.");
            return null;
        }

        private static void InvokeAddOrUpgradeWeapon(RoguelikeGame game, RoguelikeWeaponType type)
        {
            MethodInfo method = typeof(RoguelikeGame).GetMethod("AddOrUpgradeWeapon", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(method);
            method.Invoke(game, new object[] { type });
        }

        private static RoguelikeSpawnStage InvokeGetActiveSpawnStage(RoguelikeGame game)
        {
            MethodInfo method = typeof(RoguelikeGame).GetMethod("GetActiveSpawnStage", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(method);
            return (RoguelikeSpawnStage)method.Invoke(game, null);
        }

        private static List<RoguelikeSurvivalEnemy> GetEnemyList(RoguelikeGame game)
        {
            FieldInfo field = typeof(RoguelikeGame).GetField("_enemies", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            return (List<RoguelikeSurvivalEnemy>)field.GetValue(game);
        }

        private static void SetConfigTables(RoguelikeGame game, Tables tables)
        {
            FieldInfo field = typeof(RoguelikeGame).GetField("_configTables", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            field.SetValue(game, tables);
        }

        private static void AssertWeaponLevel(RoguelikeGame game, RoguelikeWeaponType type, int level)
        {
            for (int i = 0; i < game.Weapons.Count; i++)
            {
                if (game.Weapons[i].Type == type)
                {
                    Assert.AreEqual(level, game.Weapons[i].Level);
                    return;
                }
            }

            Assert.Fail($"Weapon {type} not found.");
        }
    }
}
