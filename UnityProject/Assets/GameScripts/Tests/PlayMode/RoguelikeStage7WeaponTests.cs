using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace GameLogic.Tests
{
    public sealed class RoguelikeStage7WeaponTests
    {
        [Test]
        public void WeaponChoicesCanUnlockAndUpgradeWeapons()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(7001);

            RoguelikeChoiceOption unlockBlade = BuildSingleWeaponChoice(game, RoguelikeWeaponType.SpinningBlade);
            Assert.AreEqual("weapon_spinning_blade_unlock", unlockBlade.Id);

            unlockBlade.Apply.Invoke(game.CurrentRun);
            AssertWeaponLevel(game, RoguelikeWeaponType.SpinningBlade, 1);

            RoguelikeChoiceOption upgradeBlade = BuildSingleWeaponChoice(game, RoguelikeWeaponType.SpinningBlade);
            Assert.AreEqual("weapon_spinning_blade_upgrade", upgradeBlade.Id);

            upgradeBlade.Apply.Invoke(game.CurrentRun);
            AssertWeaponLevel(game, RoguelikeWeaponType.SpinningBlade, 2);

            RoguelikeChoiceOption upgradeBolt = BuildSingleWeaponChoice(game, RoguelikeWeaponType.MagicBolt);
            Assert.AreEqual("weapon_magic_bolt_upgrade", upgradeBolt.Id);

            upgradeBolt.Apply.Invoke(game.CurrentRun);
            AssertWeaponLevel(game, RoguelikeWeaponType.MagicBolt, 2);
        }

        [Test]
        public void UnlockedWeaponsFireTogetherDuringRealtimeTick()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(7002);

            InvokeAddOrUpgradeWeapon(game, RoguelikeWeaponType.MagicBolt);
            InvokeAddOrUpgradeWeapon(game, RoguelikeWeaponType.SpinningBlade);
            InvokeAddOrUpgradeWeapon(game, RoguelikeWeaponType.SpinningBlade);

            List<RoguelikeSurvivalEnemy> enemies = GetEnemyList(game);
            enemies.Clear();
            enemies.Add(new RoguelikeSurvivalEnemy(9001, new Vector2(4f, 0f), 999, 1, 0f));

            game.Tick(0.1f);

            int boltCount = 0;
            int bladeCount = 0;
            for (int i = 0; i < game.Projectiles.Count; i++)
            {
                RoguelikeSurvivalProjectile projectile = game.Projectiles[i];
                if (projectile.WeaponType == RoguelikeWeaponType.MagicBolt)
                {
                    boltCount++;
                }
                else if (projectile.WeaponType == RoguelikeWeaponType.SpinningBlade)
                {
                    bladeCount++;
                }
            }

            Assert.GreaterOrEqual(boltCount, 1);
            Assert.GreaterOrEqual(bladeCount, 6);
            Assert.That(game.WeaponSummary, Does.Contain("Lv.2"));
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

        private static void AssertWeaponLevel(RoguelikeGame game, RoguelikeWeaponType type, int level)
        {
            for (int i = 0; i < game.Weapons.Count; i++)
            {
                RoguelikeSurvivalWeapon weapon = game.Weapons[i];
                if (weapon.Type == type)
                {
                    Assert.AreEqual(level, weapon.Level);
                    return;
                }
            }

            Assert.Fail($"Weapon {type} not found.");
        }
    }
}
