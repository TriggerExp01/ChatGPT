using System.Collections.Generic;
using NUnit.Framework;
using TEngine;
using UnityEngine;

namespace GameLogic.Tests
{
    public sealed class RoguelikeStage46ShowcaseCompositionTests
    {
        [Test]
        public void Stage46ShowcaseCompositionUsesNaturalNonGridLayout()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            MemoryPool.ClearAll();

            RoguelikePerformanceSnapshot snapshot = game.DebugPopulateShowcaseRuntimeObjects(46001);

            Assert.That(snapshot.EnemyCount, Is.EqualTo(22));
            Assert.That(snapshot.PickupCount, Is.EqualTo(10));
            Assert.That(snapshot.ProjectileCount, Is.EqualTo(20));
            Assert.That(snapshot.EffectCueCount, Is.EqualTo(6));
            Assert.That(game.Level, Is.EqualTo(5));
            Assert.That(game.ElapsedTime, Is.EqualTo(185f));
            Assert.That(game.KillCount, Is.EqualTo(42));
            Assert.That(game.CurrentRun.Gold, Is.EqualTo(18));
            Assert.That(game.LastMessage, Does.Contain("展示构图"));

            Assert.That(CountBosses(game.Enemies), Is.EqualTo(1));
            Assert.That(CountEnemiesWithConfig(game.Enemies, "wisp"), Is.GreaterThanOrEqualTo(1));
            Assert.That(CountEnemiesWithConfig(game.Enemies, "mushroom_guard"), Is.GreaterThanOrEqualTo(1));
            Assert.That(CountEnemiesWithConfig(game.Enemies, "crystal_bug"), Is.GreaterThanOrEqualTo(1));
            Assert.That(CountEnemiesWithConfig(game.Enemies, "moon_knight"), Is.GreaterThanOrEqualTo(1));

            Assert.That(CountUniqueRoundedAxis(game.Enemies, true), Is.GreaterThanOrEqualTo(18));
            Assert.That(CountUniqueRoundedAxis(game.Enemies, false), Is.GreaterThanOrEqualTo(18));
            Assert.IsTrue(HasEnemiesAroundPlayer(game));
            Assert.IsTrue(HasAllWeapons(game));
            Assert.IsTrue(HasProjectileType(game.Projectiles, RoguelikeWeaponType.StarRingPulse));
            Assert.IsTrue(HasCriticalProjectile(game.Projectiles));

            Assert.IsTrue(HasCue(game.EffectCues, RoguelikeEffectCueType.Hit, false));
            Assert.IsTrue(HasCue(game.EffectCues, RoguelikeEffectCueType.Hit, true));
            Assert.IsTrue(HasCue(game.EffectCues, RoguelikeEffectCueType.Critical, true));
            Assert.IsTrue(HasCue(game.EffectCues, RoguelikeEffectCueType.Kill, false));
            Assert.IsTrue(HasCue(game.EffectCues, RoguelikeEffectCueType.Pickup, false));
        }

        [Test]
        public void Stage46ShowcaseCompositionRefreshesStageEffectsAndDamageNumbers()
        {
            DestroyOldStage();

            RoguelikeGame game = RoguelikeGame.Instance;
            MemoryPool.ClearAll();
            game.DebugPopulateShowcaseRuntimeObjects(46002);

            RoguelikeBattleStageView view = RoguelikeBattleStageView.Ensure();
            view.DebugSetEffectPrefabAddressOverride(RoguelikeEffectCueType.Hit, "Missing_Showcase_HitEffect");
            view.DebugSetEffectPrefabAddressOverride(RoguelikeEffectCueType.Critical, "Missing_Showcase_CriticalEffect");
            view.DebugSetEffectPrefabAddressOverride(RoguelikeEffectCueType.Kill, "Missing_Showcase_KillEffect");
            view.DebugSetEffectPrefabAddressOverride(RoguelikeEffectCueType.Pickup, "Missing_Showcase_PickupEffect");
            view.Refresh(game.CurrentRun);

            Assert.That(view.ActiveEffectViewCount, Is.GreaterThanOrEqualTo(4));
            Assert.That(view.ActiveDamageNumberCount, Is.GreaterThanOrEqualTo(3));
            Assert.That(view.EffectFallbackCount, Is.GreaterThanOrEqualTo(4));
            Assert.IsTrue(view.PickupFeedbackVisible);
            Assert.That(view.LastCameraShakeMagnitude, Is.GreaterThan(0f));

            RoguelikePerformanceSnapshot snapshot = game.CapturePerformanceSnapshot(0.1f, view);
            Assert.That(snapshot.ActiveEffectViewCount, Is.GreaterThanOrEqualTo(4));
            Assert.That(snapshot.EnemyCount, Is.EqualTo(22));
            Assert.That(snapshot.ProjectileCount, Is.EqualTo(20));
            Assert.That(snapshot.PickupCount, Is.EqualTo(10));
        }

        private static void DestroyOldStage()
        {
            GameObject oldStage = GameObject.Find("Roguelike2DSurvivalStage");
            if (oldStage != null)
            {
                Object.DestroyImmediate(oldStage);
            }
        }

        private static int CountBosses(IReadOnlyList<RoguelikeSurvivalEnemy> enemies)
        {
            int count = 0;
            for (int i = 0; i < enemies.Count; i++)
            {
                if (enemies[i].IsBoss)
                {
                    count++;
                }
            }

            return count;
        }

        private static int CountEnemiesWithConfig(IReadOnlyList<RoguelikeSurvivalEnemy> enemies, string configId)
        {
            int count = 0;
            for (int i = 0; i < enemies.Count; i++)
            {
                if (enemies[i].ConfigId == configId)
                {
                    count++;
                }
            }

            return count;
        }

        private static int CountUniqueRoundedAxis(IReadOnlyList<RoguelikeSurvivalEnemy> enemies, bool xAxis)
        {
            HashSet<int> values = new HashSet<int>();
            for (int i = 0; i < enemies.Count; i++)
            {
                float value = xAxis ? enemies[i].Position.x : enemies[i].Position.y;
                values.Add(Mathf.RoundToInt(value * 10f));
            }

            return values.Count;
        }

        private static bool HasEnemiesAroundPlayer(RoguelikeGame game)
        {
            bool left = false;
            bool right = false;
            bool upper = false;
            bool lower = false;
            for (int i = 0; i < game.Enemies.Count; i++)
            {
                Vector2 offset = game.Enemies[i].Position - game.PlayerPosition;
                left |= offset.x < -0.8f;
                right |= offset.x > 0.8f;
                upper |= offset.y > 0.8f;
                lower |= offset.y < -0.8f;
            }

            return left && right && upper && lower;
        }

        private static bool HasAllWeapons(RoguelikeGame game)
        {
            return HasWeapon(game, RoguelikeWeaponType.MagicBolt)
                && HasWeapon(game, RoguelikeWeaponType.SpinningBlade)
                && HasWeapon(game, RoguelikeWeaponType.PiercingDart)
                && HasWeapon(game, RoguelikeWeaponType.StarRingPulse);
        }

        private static bool HasWeapon(RoguelikeGame game, RoguelikeWeaponType type)
        {
            for (int i = 0; i < game.Weapons.Count; i++)
            {
                if (game.Weapons[i].Type == type)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasProjectileType(IReadOnlyList<RoguelikeSurvivalProjectile> projectiles, RoguelikeWeaponType type)
        {
            for (int i = 0; i < projectiles.Count; i++)
            {
                if (projectiles[i].WeaponType == type)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasCriticalProjectile(IReadOnlyList<RoguelikeSurvivalProjectile> projectiles)
        {
            for (int i = 0; i < projectiles.Count; i++)
            {
                if (projectiles[i].IsCritical)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasCue(IReadOnlyList<RoguelikeEffectCue> cues, RoguelikeEffectCueType type, bool critical)
        {
            for (int i = 0; i < cues.Count; i++)
            {
                RoguelikeEffectCue cue = cues[i];
                if (cue.Type == type && cue.IsCritical == critical)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
