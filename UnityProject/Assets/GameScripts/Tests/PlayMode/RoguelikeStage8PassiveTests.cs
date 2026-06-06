using System.Collections.Generic;
using System.IO;
using System.Reflection;
using GameConfig;
using Luban;
using NUnit.Framework;
using UnityEngine;

namespace GameLogic.Tests
{
    public sealed class RoguelikeStage8PassiveTests
    {
        [Test]
        public void PassiveChoicesApplyRelicsAndRuntimeBonuses()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(8001);
            SetConfigTables(game, LoadTables());

            List<RoguelikeChoiceOption> choices = BuildPassiveChoices(game);
            Assert.GreaterOrEqual(choices.Count, 3);

            float startPickupRadius = game.PickupAttractRadius;
            float startMoveSpeed = game.MoveSpeed;

            FindChoice(choices, "passive_magnet_core").Apply.Invoke(game.CurrentRun);
            FindChoice(choices, "passive_wind_boots").Apply.Invoke(game.CurrentRun);
            FindChoice(choices, "passive_magnet_core").Apply.Invoke(game.CurrentRun);

            Assert.Greater(game.PickupAttractRadius, startPickupRadius + 1.5f);
            Assert.Greater(game.MoveSpeed, startMoveSpeed);
            Assert.AreEqual(3, game.CurrentRun.Relics.Count);
            Assert.That(game.PassiveSummary, Does.Contain("磁力核心 x2"));
            Assert.That(game.PassiveSummary, Does.Contain("疾风靴"));
        }

        [Test]
        public void DamagePassiveAffectsRealtimeProjectiles()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(8002);
            SetConfigTables(game, LoadTables());

            List<RoguelikeChoiceOption> choices = BuildPassiveChoices(game);
            FindChoice(choices, "passive_power_charm").Apply.Invoke(game.CurrentRun);

            List<RoguelikeSurvivalEnemy> enemies = GetEnemyList(game);
            enemies.Clear();
            enemies.Add(new RoguelikeSurvivalEnemy(9101, new Vector2(4f, 0f), 999, 1, 0f));

            game.Tick(0.1f);

            RoguelikeSurvivalProjectile bolt = FindProjectile(game.Projectiles, RoguelikeWeaponType.MagicBolt);
            Assert.NotNull(bolt);
            Assert.Greater(bolt.Damage, 12);
            Assert.That(game.PassiveSummary, Does.Contain("聚能护符"));
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

        private static RoguelikeSurvivalProjectile FindProjectile(IReadOnlyList<RoguelikeSurvivalProjectile> projectiles, RoguelikeWeaponType type)
        {
            for (int i = 0; i < projectiles.Count; i++)
            {
                if (projectiles[i].WeaponType == type)
                {
                    return projectiles[i];
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

        private static void SetConfigTables(RoguelikeGame game, Tables tables)
        {
            FieldInfo field = typeof(RoguelikeGame).GetField("_configTables", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            field.SetValue(game, tables);
        }
    }
}
