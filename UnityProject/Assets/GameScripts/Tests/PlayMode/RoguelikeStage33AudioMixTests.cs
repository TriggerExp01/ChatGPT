using System.Collections.Generic;
using System.IO;
using System.Reflection;
using GameConfig;
using Luban;
using NUnit.Framework;
using UnityEngine;

namespace GameLogic.Tests
{
    public sealed class RoguelikeStage33AudioMixTests
    {
        [Test]
        public void Stage33AudioAssetsCoverCombatBossAndUiCues()
        {
            string audioPath = Path.Combine(Application.dataPath, "AssetRaw", "Audios");

            Assert.IsTrue(File.Exists(Path.Combine(audioPath, RoguelikeGame.HitSoundPath + ".wav")));
            Assert.IsTrue(File.Exists(Path.Combine(audioPath, RoguelikeGame.PickupSoundPath + ".wav")));
            Assert.IsTrue(File.Exists(Path.Combine(audioPath, RoguelikeGame.LevelUpSoundPath + ".wav")));
            Assert.IsTrue(File.Exists(Path.Combine(audioPath, RoguelikeGame.BossSoundPath + ".wav")));
            Assert.IsTrue(File.Exists(Path.Combine(audioPath, RoguelikeGame.UiConfirmSoundPath + ".wav")));
        }

        [Test]
        public void Stage33AudioMixRulesUseSafeVolumeAndShortCooldowns()
        {
            Assert.That(RoguelikeGame.HitSoundVolume, Is.InRange(0.2f, 0.8f));
            Assert.That(RoguelikeGame.PickupSoundVolume, Is.InRange(0.2f, 0.8f));
            Assert.That(RoguelikeGame.LevelUpSoundVolume, Is.InRange(0.2f, 0.8f));
            Assert.That(RoguelikeGame.BossSoundVolume, Is.InRange(0.2f, 0.8f));
            Assert.That(RoguelikeGame.UiConfirmSoundVolume, Is.InRange(0.1f, 0.6f));

            Assert.That(RoguelikeGame.HitSoundCooldown, Is.LessThanOrEqualTo(0.08f));
            Assert.That(RoguelikeGame.PickupSoundCooldown, Is.LessThanOrEqualTo(0.08f));
            Assert.That(RoguelikeGame.LevelUpSoundCooldown, Is.LessThanOrEqualTo(0.20f));
            Assert.That(RoguelikeGame.BossSoundCooldown, Is.InRange(0.25f, 0.70f));
            Assert.That(RoguelikeGame.UiConfirmSoundCooldown, Is.LessThanOrEqualTo(0.12f));
        }

        [Test]
        public void UiConfirmSoundUsesIndependentCooldown()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(33001);

            game.PlayUiConfirmSound();

            Dictionary<string, float> cooldowns = GetSoundCooldowns(game);
            Assert.IsTrue(cooldowns.ContainsKey(RoguelikeGame.UiConfirmSoundPath));
            Assert.That(cooldowns[RoguelikeGame.UiConfirmSoundPath], Is.GreaterThan(0f));

            game.Tick(0.1f);
            game.Tick(0.1f);
            game.Tick(0.1f);

            Assert.IsFalse(GetSoundCooldowns(game).ContainsKey(RoguelikeGame.UiConfirmSoundPath));
        }

        [Test]
        public void BossSpawnSoundDoesNotBlockUiConfirmCue()
        {
            Tables tables = LoadTables();
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(33002);
            SetConfigTables(game, tables);
            game.DebugSkipRoom(20);
            SetSpawnTimer(game, 0f);

            game.Tick(0.1f);
            game.PlayUiConfirmSound();

            Dictionary<string, float> cooldowns = GetSoundCooldowns(game);
            Assert.IsTrue(cooldowns.ContainsKey(RoguelikeGame.BossSoundPath));
            Assert.IsTrue(cooldowns.ContainsKey(RoguelikeGame.UiConfirmSoundPath));
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

        private static Dictionary<string, float> GetSoundCooldowns(RoguelikeGame game)
        {
            FieldInfo directorField = typeof(RoguelikeGame).GetField("_feedbackDirector", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(directorField);
            object director = directorField.GetValue(game);
            FieldInfo field = typeof(RoguelikeFeedbackDirector).GetField("_soundCooldowns", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            return (Dictionary<string, float>)field.GetValue(director);
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
