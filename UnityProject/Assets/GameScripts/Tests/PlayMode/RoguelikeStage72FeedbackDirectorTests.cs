using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using TEngine;
using UnityEngine;

namespace GameLogic.Tests
{
    public sealed class RoguelikeStage72FeedbackDirectorTests
    {
        [Test]
        public void FeedbackDirectorTicksTransientFeedbackAndSoundCooldowns()
        {
            RoguelikeFeedbackDirector director = new RoguelikeFeedbackDirector();

            director.SetAttackFlash(0.12f);
            director.SetPickupFlash(0.14f);
            director.TriggerCameraShake(0.10f);
            director.SetSoundCooldown("test_sound", 0.08f);

            Assert.That(director.AttackFlash, Is.GreaterThan(0f));
            Assert.That(director.PickupFlash, Is.GreaterThan(0f));
            Assert.That(director.CameraShake, Is.GreaterThan(0f));
            Assert.That(director.GetSoundCooldown("test_sound"), Is.GreaterThan(0f));

            director.Tick(0.2f);

            Assert.That(director.AttackFlash, Is.EqualTo(0f));
            Assert.That(director.PickupFlash, Is.EqualTo(0f));
            Assert.That(director.CameraShake, Is.EqualTo(0f));
            Assert.That(director.GetSoundCooldown("test_sound"), Is.EqualTo(0f));
        }

        [Test]
        public void FeedbackDirectorCapsEffectCueHistory()
        {
            RoguelikeFeedbackDirector director = new RoguelikeFeedbackDirector();
            List<RoguelikeEffectCue> cues = new List<RoguelikeEffectCue>();
            int sequence = 1;

            for (int i = 0; i < 110; i++)
            {
                director.AddEffectCue(cues, ref sequence, RoguelikeEffectCueType.Hit, new Vector2(i, 0f), i);
            }

            Assert.That(cues.Count, Is.EqualTo(96));
            Assert.That(cues[0].Sequence, Is.EqualTo(15));
            Assert.That(sequence, Is.EqualTo(111));
        }

        [Test]
        public void BossVictoryTriggersSecondRoundFeedback()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(72001);
            game.CurrentRun.AddGold(6);

            List<RoguelikeSurvivalEnemy> enemies = GetEnemyList(game);
            enemies.Clear();
            RoguelikeSurvivalEnemy boss = MemoryPool.Acquire<RoguelikeSurvivalEnemy>();
            boss.Init(72001, new Vector2(1f, 0f), 1, 1, 0f, true, "dungeon_heart", true);
            boss.Health = 0;
            enemies.Add(boss);

            game.Tick(0.1f);

            Assert.That(game.Phase, Is.EqualTo(RoguelikeGamePhase.Victory));
            Assert.That(game.LastMessage, Does.Contain("生存目标达成"));
            Assert.That(game.CameraShake, Is.GreaterThan(0.20f));
            Assert.That(game.AttackFlash, Is.GreaterThan(0.18f));
            Assert.That(game.PickupFlash, Is.GreaterThan(0.16f));
            Assert.That(GetSoundCooldown(game, RoguelikeGame.BossSoundPath), Is.GreaterThan(0f));
        }

        private static List<RoguelikeSurvivalEnemy> GetEnemyList(RoguelikeGame game)
        {
            FieldInfo field = typeof(RoguelikeGame).GetField("_enemies", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            return (List<RoguelikeSurvivalEnemy>)field.GetValue(game);
        }

        private static float GetSoundCooldown(RoguelikeGame game, string path)
        {
            FieldInfo directorField = typeof(RoguelikeGame).GetField("_feedbackDirector", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(directorField);
            RoguelikeFeedbackDirector director = (RoguelikeFeedbackDirector)directorField.GetValue(game);
            return director.GetSoundCooldown(path);
        }
    }
}
