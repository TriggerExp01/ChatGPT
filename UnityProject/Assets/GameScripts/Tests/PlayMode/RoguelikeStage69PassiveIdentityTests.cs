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
    public sealed class RoguelikeStage69PassiveIdentityTests
    {
        [Test]
        public void Stage69RelicTableCoversSixPassiveRoles()
        {
            Tables tables = LoadTables();

            AssertRelicEffect(tables.TbRoguelikeRelic.Get("eagle_eye"), EEffectType.AddAttack, EEffectType.AddCritChance);
            AssertRelicEffect(tables.TbRoguelikeRelic.Get("starlight_soles"), EEffectType.AddMoveSpeed, EEffectType.AddPickupRadius);
            AssertRelicEffect(tables.TbRoguelikeRelic.Get("vital_sigil"), EEffectType.AddMaxHealth, EEffectType.Heal);
            AssertRelicEffect(tables.TbRoguelikeRelic.Get("coin_badge"), EEffectType.AddGoldMultiplier);
        }

        [Test]
        public void Stage69PassivePoolContainsGoldIncomeRelic()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(69001);
            SetConfigTables(game, LoadTables());

            List<RoguelikeChoiceOption> pool = BuildPassiveChoices(game);

            Assert.NotNull(FindChoice(pool, "passive_coin_badge"));
            Assert.NotNull(FindChoice(pool, "passive_eagle_eye"));
            Assert.NotNull(FindChoice(pool, "passive_starlight_soles"));
            Assert.NotNull(FindChoice(pool, "passive_vital_sigil"));
        }

        [Test]
        public void Stage69GoldIncomePassiveScalesGoldPickupOnly()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(69002);
            SetConfigTables(game, LoadTables());
            List<RoguelikeChoiceOption> pool = BuildPassiveChoices(game);
            RoguelikeChoiceOption coinBadge = FindChoice(pool, "passive_coin_badge");
            int gold = game.CurrentRun.Gold;

            coinBadge.Apply.Invoke(game.CurrentRun);

            Assert.That(game.GoldPickupMultiplier, Is.EqualTo(1.25f).Within(0.001f));
            Assert.That(game.CurrentRun.Gold, Is.EqualTo(gold));
            Assert.That(HasRelic(game.CurrentRun, "coin_badge"), Is.True);

            InvokeCreatePickup(game, 690020, RoguelikePickupType.Gold, Vector2.zero, 4);
            InvokeUpdatePickups(game, 0.02f);

            Assert.That(game.CurrentRun.Gold, Is.EqualTo(gold + 5));
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

        private static void AssertRelicEffect(RoguelikeRelic relic, params EEffectType[] expected)
        {
            Assert.NotNull(relic);
            Assert.That(relic.Effects.Count, Is.EqualTo(expected.Length));
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.That(relic.Effects[i].Type, Is.EqualTo(expected[i]));
            }
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

        private static void SetConfigTables(RoguelikeGame game, Tables tables)
        {
            FieldInfo field = typeof(RoguelikeGame).GetField("_configTables", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            field.SetValue(game, tables);
        }

        private static bool HasRelic(RoguelikeRunState run, string id)
        {
            for (int i = 0; i < run.Relics.Count; i++)
            {
                if (run.Relics[i].Id == id)
                {
                    return true;
                }
            }

            return false;
        }

        private static void InvokeCreatePickup(RoguelikeGame game, int id, RoguelikePickupType type, Vector2 position, int amount)
        {
            MethodInfo method = typeof(RoguelikeGame).GetMethod("CreatePickup", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.NotNull(method);
            RoguelikeSurvivalPickup pickup = (RoguelikeSurvivalPickup)method.Invoke(game, new object[] { id, type, position, amount });
            FieldInfo field = typeof(RoguelikeGame).GetField("_pickups", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            ((List<RoguelikeSurvivalPickup>)field.GetValue(game)).Add(pickup);
        }

        private static void InvokeUpdatePickups(RoguelikeGame game, float deltaTime)
        {
            MethodInfo method = typeof(RoguelikeGame).GetMethod("UpdatePickups", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(method);
            method.Invoke(game, new object[] { deltaTime });
        }
    }
}
