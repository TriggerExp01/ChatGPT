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
    public sealed class RoguelikeStage21EnemyExpansionTests
    {
        [Test]
        public void Stage21EnemyRowsAreGeneratedFromLubanBytes()
        {
            Tables tables = LoadTables();

            AssertEnemy(tables.TbRoguelikeEnemy.Get("wisp"), "星尘游灵", EEnemyTier.Common, 16, 6, 0);
            AssertEnemy(tables.TbRoguelikeEnemy.Get("mushroom_guard"), "蘑菇守卫", EEnemyTier.Common, 38, 5, 2);
            AssertEnemy(tables.TbRoguelikeEnemy.Get("crystal_bug"), "晶壳虫", EEnemyTier.Common, 28, 7, 1);
            AssertEnemy(tables.TbRoguelikeEnemy.Get("moon_knight"), "月影骑士", EEnemyTier.Elite, 58, 11, 2);
        }

        [Test]
        public void Stage21SpawnStagesReferenceNewEnemies()
        {
            Tables tables = LoadTables();

            RoguelikeSpawnStage stage1 = tables.TbRoguelikeSpawnStage.Get(1);
            RoguelikeSpawnStage stage2 = tables.TbRoguelikeSpawnStage.Get(2);
            RoguelikeSpawnStage stage3 = tables.TbRoguelikeSpawnStage.Get(3);

            CollectionAssert.Contains(stage1.CommonEnemyIds, "wisp");
            CollectionAssert.Contains(stage2.CommonEnemyIds, "mushroom_guard");
            CollectionAssert.Contains(stage3.CommonEnemyIds, "crystal_bug");
            CollectionAssert.Contains(stage3.CommonEnemyIds, "mushroom_guard");
            Assert.That(stage2.EliteEnemyId, Is.EqualTo("moon_knight"));
            Assert.That(stage3.EliteEnemyId, Is.EqualTo("moon_knight"));
            Assert.That(stage2.CommonEnemyIds.Count, Is.EqualTo(stage2.CommonWeights.Count));
            Assert.That(stage3.CommonEnemyIds.Count, Is.EqualTo(stage3.CommonWeights.Count));
        }

        [Test]
        public void Stage21ConfiguredContentCatalogContainsExpandedEnemyPool()
        {
            RoguelikeContentCatalog catalog = RoguelikeContentCatalog.CreateFromLuban(LoadTables());

            Assert.That(FindEnemy(catalog.CommonEnemies, "wisp").DisplayName, Is.EqualTo("星尘游灵"));
            Assert.That(FindEnemy(catalog.CommonEnemies, "mushroom_guard").Stats.MaxHealth, Is.EqualTo(38));
            Assert.That(FindEnemy(catalog.CommonEnemies, "crystal_bug").Stats.Defense, Is.EqualTo(1));
            Assert.That(FindEnemy(catalog.EliteEnemies, "moon_knight").DisplayName, Is.EqualTo("月影骑士"));
        }

        [Test]
        public void Stage21RuntimeWeightedPickerUsesExpandedStageEntries()
        {
            Tables tables = LoadTables();
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(21001);
            SetConfigTables(game, tables);
            RoguelikeSpawnStage stage3 = tables.TbRoguelikeSpawnStage.Get(3);

            MethodInfo method = typeof(RoguelikeGame).GetMethod("PickWeightedEnemy", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(method);
            for (int i = 0; i < 12; i++)
            {
                RoguelikeEnemy enemy = (RoguelikeEnemy)method.Invoke(game, new object[] { tables, stage3 });
                Assert.NotNull(enemy);
                CollectionAssert.Contains(stage3.CommonEnemyIds, enemy.Id);
            }
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

        private static void AssertEnemy(RoguelikeEnemy enemy, string displayName, EEnemyTier tier, int maxHealth, int attack, int defense)
        {
            Assert.NotNull(enemy);
            Assert.That(enemy.DisplayName, Is.EqualTo(displayName));
            Assert.That(enemy.Tier, Is.EqualTo(tier));
            Assert.That(enemy.MaxHealth, Is.EqualTo(maxHealth));
            Assert.That(enemy.Attack, Is.EqualTo(attack));
            Assert.That(enemy.Defense, Is.EqualTo(defense));
        }

        private static RoguelikeEnemyTemplate FindEnemy(IReadOnlyList<RoguelikeEnemyTemplate> enemies, string id)
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                if (enemies[i].Id == id)
                {
                    return enemies[i];
                }
            }

            Assert.Fail($"Enemy {id} not found.");
            return null;
        }

        private static void SetConfigTables(RoguelikeGame game, Tables tables)
        {
            FieldInfo field = typeof(RoguelikeGame).GetField("_configTables", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            field.SetValue(game, tables);
        }
    }
}
