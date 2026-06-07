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
    public sealed class RoguelikeStage24FourthWeaponTests
    {
        [Test]
        public void Stage24WeaponTableContainsStarRingPulse()
        {
            Tables tables = LoadTables();

            RoguelikeWeapon weapon = tables.TbRoguelikeWeapon.Get("star_ring_pulse");

            Assert.NotNull(weapon);
            Assert.That(weapon.DisplayName, Is.EqualTo("星环脉冲"));
            Assert.That(weapon.WeaponType, Is.EqualTo("StarRingPulse"));
            Assert.That(weapon.BaseDamage, Is.EqualTo(7));
            Assert.That(weapon.BaseRange, Is.EqualTo(2.4f).Within(0.001f));
        }

        [Test]
        public void Stage24RewardPoolCanUnlockAndUpgradeFourthWeapon()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(24001);
            SetConfigTables(game, LoadTables());

            RoguelikeChoiceOption unlock = BuildSingleWeaponChoice(game, RoguelikeWeaponType.StarRingPulse);
            unlock.Apply.Invoke(game.CurrentRun);

            AssertWeaponLevel(game, RoguelikeWeaponType.StarRingPulse, 1);
            Assert.That(game.WeaponSummary, Does.Contain("星环脉冲"));

            RoguelikeChoiceOption upgrade = BuildSingleWeaponChoice(game, RoguelikeWeaponType.StarRingPulse);
            upgrade.Apply.Invoke(game.CurrentRun);

            AssertWeaponLevel(game, RoguelikeWeaponType.StarRingPulse, 2);
        }

        [Test]
        public void Stage24StarRingPulseFiresShortRangeRingProjectiles()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(24002);
            SetConfigTables(game, LoadTables());
            InvokeAddOrUpgradeWeapon(game, RoguelikeWeaponType.StarRingPulse);
            GetEnemyList(game).Add(new RoguelikeSurvivalEnemy(24002, new Vector2(1.4f, 0f), 999, 1, 0f));

            game.Tick(0.1f);

            Assert.That(CountProjectiles(game, RoguelikeWeaponType.StarRingPulse), Is.EqualTo(8));
            RoguelikeSurvivalProjectile projectile = FindProjectile(game.Projectiles, RoguelikeWeaponType.StarRingPulse);
            Assert.That(projectile.Damage, Is.EqualTo(7));
            Assert.That(projectile.RemainingDistance, Is.LessThan(2.4f));
            Assert.That(projectile.RemainingDistance, Is.GreaterThan(1.7f));
            Assert.That(projectile.Speed, Is.EqualTo(6.2f).Within(0.001f));
        }

        [Test]
        public void Stage24AllFourWeaponsCanFireTogether()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(24003);
            SetConfigTables(game, LoadTables());
            InvokeAddOrUpgradeWeapon(game, RoguelikeWeaponType.SpinningBlade);
            InvokeAddOrUpgradeWeapon(game, RoguelikeWeaponType.PiercingDart);
            InvokeAddOrUpgradeWeapon(game, RoguelikeWeaponType.StarRingPulse);
            GetEnemyList(game).Add(new RoguelikeSurvivalEnemy(24003, new Vector2(2.5f, 0f), 999, 1, 0f));

            game.Tick(0.1f);

            Assert.That(CountProjectiles(game, RoguelikeWeaponType.MagicBolt), Is.EqualTo(1));
            Assert.That(CountProjectiles(game, RoguelikeWeaponType.SpinningBlade), Is.GreaterThanOrEqualTo(5));
            Assert.That(CountProjectiles(game, RoguelikeWeaponType.PiercingDart), Is.EqualTo(1));
            Assert.That(CountProjectiles(game, RoguelikeWeaponType.StarRingPulse), Is.EqualTo(8));
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

        private static RoguelikeChoiceOption BuildSingleWeaponChoice(RoguelikeGame game, RoguelikeWeaponType type)
        {
            List<RoguelikeChoiceOption> pool = new List<RoguelikeChoiceOption>();
            MethodInfo method = typeof(RoguelikeGame).GetMethod("AddWeaponChoice", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(method);
            method.Invoke(game, new object[] { pool, type });
            Assert.AreEqual(1, pool.Count);
            return pool[0];
        }

        private static void InvokeAddOrUpgradeWeapon(RoguelikeGame game, RoguelikeWeaponType type)
        {
            MethodInfo method = typeof(RoguelikeGame).GetMethod("AddOrUpgradeWeapon", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(method);
            method.Invoke(game, new object[] { type });
        }

        private static List<RoguelikeSurvivalEnemy> GetEnemyList(RoguelikeGame game)
        {
            FieldInfo field = typeof(RoguelikeGame).GetField("_enemies", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            return (List<RoguelikeSurvivalEnemy>)field.GetValue(game);
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

            Assert.Fail($"Projectile {type} not found.");
            return null;
        }

        private static int CountProjectiles(RoguelikeGame game, RoguelikeWeaponType type)
        {
            int count = 0;
            for (int i = 0; i < game.Projectiles.Count; i++)
            {
                if (game.Projectiles[i].WeaponType == type)
                {
                    count++;
                }
            }

            return count;
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
                    Assert.That(game.Weapons[i].Level, Is.EqualTo(level));
                    return;
                }
            }

            Assert.Fail($"Weapon {type} not found.");
        }
    }
}
