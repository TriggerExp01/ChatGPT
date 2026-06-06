using System.Collections.Generic;
using System.IO;
using System.Reflection;
using GameConfig;
using Luban;
using NUnit.Framework;
using UnityEngine;

namespace GameLogic.Tests
{
    public sealed class RoguelikeStage13ConfigTests
    {
        [Test]
        public void RoguelikeLubanTablesCanLoadFromGeneratedBytes()
        {
            Tables tables = LoadTables();

            Assert.GreaterOrEqual(tables.TbRoguelikeEnemy.DataList.Count, 3);
            Assert.GreaterOrEqual(tables.TbRoguelikeChoice.DataList.Count, 3);
            Assert.GreaterOrEqual(tables.TbRoguelikeRelic.DataList.Count, 1);
            Assert.GreaterOrEqual(tables.TbRoguelikeWeapon.DataList.Count, 2);
            Assert.GreaterOrEqual(tables.TbRoguelikeSpawnStage.DataList.Count, 1);
        }

        [Test]
        public void ConfiguredRewardChoicesApplyGeneratedEffects()
        {
            Tables tables = LoadTables();
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(13001);
            SetConfigTables(game, tables);

            List<RoguelikeChoiceOption> pool = new List<RoguelikeChoiceOption>();
            Assert.IsTrue(InvokeAddConfiguredRewardChoices(game, pool));

            RoguelikeChoiceOption attack = FindChoice(pool, "atk_2");
            int startAttack = game.CurrentRun.Player.Stats.Attack;
            attack.Apply.Invoke(game.CurrentRun);

            Assert.AreEqual(startAttack + 2, game.CurrentRun.Player.Stats.Attack);
            Assert.That(attack.Title, Does.Contain("磨砺"));
        }

        [Test]
        public void SurvivalSpawnUsesConfiguredEnemyStatsWhenTablesAreAvailable()
        {
            Tables tables = LoadTables();
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(13002);
            SetConfigTables(game, tables);

            game.Tick(0.1f);
            game.Tick(0.1f);

            Assert.GreaterOrEqual(game.Enemies.Count, 1);
            HashSet<int> configuredCommonHealth = new HashSet<int>();
            for (int i = 0; i < tables.TbRoguelikeEnemy.DataList.Count; i++)
            {
                GameConfig.roguelike.RoguelikeEnemy enemy = tables.TbRoguelikeEnemy.DataList[i];
                if (enemy.Tier == GameConfig.roguelike.EEnemyTier.Common)
                {
                    configuredCommonHealth.Add(enemy.MaxHealth);
                }
            }

            Assert.IsTrue(configuredCommonHealth.Contains(game.Enemies[0].Health));
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

        private static bool InvokeAddConfiguredRewardChoices(RoguelikeGame game, List<RoguelikeChoiceOption> pool)
        {
            MethodInfo method = typeof(RoguelikeGame).GetMethod("AddConfiguredRewardChoices", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(method);
            return (bool)method.Invoke(game, new object[] { pool });
        }

        private static void SetConfigTables(RoguelikeGame game, Tables tables)
        {
            FieldInfo field = typeof(RoguelikeGame).GetField("_configTables", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            field.SetValue(game, tables);
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
    }
}
