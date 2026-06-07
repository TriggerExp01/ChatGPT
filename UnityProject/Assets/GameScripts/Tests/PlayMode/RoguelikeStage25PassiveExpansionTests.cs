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
    public sealed class RoguelikeStage25PassiveExpansionTests
    {
        [Test]
        public void Stage25RelicRowsAreGeneratedFromLubanBytes()
        {
            Tables tables = LoadTables();

            AssertRelic(tables.TbRoguelikeRelic.Get("vital_sigil"), "生息符文", "AddMaxHealth", "Heal");
            AssertRelic(tables.TbRoguelikeRelic.Get("eagle_eye"), "鹰眼纹章", "AddAttack", "AddCritChance");
            AssertRelic(tables.TbRoguelikeRelic.Get("starlight_soles"), "星辉轻靴", "AddMoveSpeed", "AddPickupRadius");
        }

        [Test]
        public void Stage25PassiveChoicePoolContainsExpandedRelics()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(25001);
            SetConfigTables(game, LoadTables());

            List<RoguelikeChoiceOption> pool = BuildPassiveChoices(game);

            Assert.NotNull(FindChoice(pool, "passive_vital_sigil"));
            Assert.NotNull(FindChoice(pool, "passive_eagle_eye"));
            Assert.NotNull(FindChoice(pool, "passive_starlight_soles"));
        }

        [Test]
        public void Stage25ExpandedPassivesApplyExistingRuntimeStats()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(25002);
            SetConfigTables(game, LoadTables());
            List<RoguelikeChoiceOption> pool = BuildPassiveChoices(game);

            int maxHealth = game.CurrentRun.Player.Stats.MaxHealth;
            int attack = game.CurrentRun.Player.Stats.Attack;
            float critChance = game.CurrentRun.Player.Stats.CritChance;
            float moveSpeed = game.MoveSpeed;
            float pickupRadius = game.PickupAttractRadius;

            FindChoice(pool, "passive_vital_sigil").Apply.Invoke(game.CurrentRun);
            FindChoice(pool, "passive_eagle_eye").Apply.Invoke(game.CurrentRun);
            FindChoice(pool, "passive_starlight_soles").Apply.Invoke(game.CurrentRun);

            Assert.That(game.CurrentRun.Player.Stats.MaxHealth, Is.EqualTo(maxHealth + 18));
            Assert.That(game.CurrentRun.Player.Stats.Attack, Is.EqualTo(attack + 2));
            Assert.That(game.CurrentRun.Player.Stats.CritChance, Is.EqualTo(critChance + 0.06f).Within(0.001f));
            Assert.That(game.MoveSpeed, Is.EqualTo(moveSpeed + 0.2f).Within(0.001f));
            Assert.That(game.PickupAttractRadius, Is.EqualTo(pickupRadius + 0.4f).Within(0.001f));
            Assert.That(game.PassiveSummary, Does.Contain("生息符文"));
            Assert.That(game.PassiveSummary, Does.Contain("鹰眼纹章"));
            Assert.That(game.PassiveSummary, Does.Contain("星辉轻靴"));
        }

        [Test]
        public void Stage25RepeatedPassiveStillStacksAndShowsCount()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(25003);
            SetConfigTables(game, LoadTables());
            List<RoguelikeChoiceOption> pool = BuildPassiveChoices(game);
            RoguelikeChoiceOption eagleEye = FindChoice(pool, "passive_eagle_eye");
            int attack = game.CurrentRun.Player.Stats.Attack;
            float critChance = game.CurrentRun.Player.Stats.CritChance;

            eagleEye.Apply.Invoke(game.CurrentRun);
            eagleEye.Apply.Invoke(game.CurrentRun);

            Assert.That(game.CurrentRun.Player.Stats.Attack, Is.EqualTo(attack + 4));
            Assert.That(game.CurrentRun.Player.Stats.CritChance, Is.EqualTo(critChance + 0.12f).Within(0.001f));
            Assert.That(game.PassiveSummary, Does.Contain("鹰眼纹章 x2"));
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

        private static void AssertRelic(RoguelikeRelic relic, string displayName, string firstEffect, string secondEffect)
        {
            Assert.NotNull(relic);
            Assert.That(relic.DisplayName, Is.EqualTo(displayName));
            Assert.That(relic.Effects.Count, Is.EqualTo(2));
            Assert.That(relic.Effects[0].Type.ToString(), Is.EqualTo(firstEffect));
            Assert.That(relic.Effects[1].Type.ToString(), Is.EqualTo(secondEffect));
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
    }
}
