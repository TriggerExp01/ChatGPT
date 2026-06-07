using System.IO;
using System.Reflection;
using GameConfig;
using GameConfig.roguelike;
using Luban;
using NUnit.Framework;
using UnityEngine;

namespace GameLogic.Tests
{
    public sealed class RoguelikeStage23WeaponBalanceTests
    {
        [Test]
        public void Stage23WeaponRowsDefineDistinctRoles()
        {
            Tables tables = LoadTables();

            RoguelikeWeapon magicBolt = tables.TbRoguelikeWeapon.Get("magic_bolt");
            RoguelikeWeapon spinningBlade = tables.TbRoguelikeWeapon.Get("spinning_blade");
            RoguelikeWeapon piercingDart = tables.TbRoguelikeWeapon.Get("piercing_dart");

            Assert.That(magicBolt.BaseDamage, Is.EqualTo(13));
            Assert.That(magicBolt.BaseInterval, Is.EqualTo(0.52f).Within(0.001f));
            Assert.That(spinningBlade.BaseRange, Is.LessThan(magicBolt.BaseRange));
            Assert.That(spinningBlade.BaseInterval, Is.LessThan(piercingDart.BaseInterval));
            Assert.That(piercingDart.BaseRange, Is.GreaterThan(magicBolt.BaseRange));
            Assert.That(piercingDart.DamageGrowth, Is.GreaterThan(spinningBlade.DamageGrowth));
        }

        [Test]
        public void Stage23RuntimeUsesConfiguredWeaponNumbers()
        {
            Tables tables = LoadTables();
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(23001);
            SetConfigTables(game, tables);

            RoguelikeSurvivalWeapon magicBolt = FindWeapon(game, RoguelikeWeaponType.MagicBolt);
            InvokeAddOrUpgradeWeapon(game, RoguelikeWeaponType.SpinningBlade);
            InvokeAddOrUpgradeWeapon(game, RoguelikeWeaponType.PiercingDart);
            RoguelikeSurvivalWeapon spinningBlade = FindWeapon(game, RoguelikeWeaponType.SpinningBlade);
            RoguelikeSurvivalWeapon piercingDart = FindWeapon(game, RoguelikeWeaponType.PiercingDart);

            Assert.That(InvokeGetWeaponDamage(game, magicBolt, 1), Is.EqualTo(13));
            Assert.That(InvokeGetWeaponDamage(game, spinningBlade, 1), Is.EqualTo(8));
            Assert.That(InvokeGetWeaponDamage(game, piercingDart, 1), Is.EqualTo(11));
            Assert.That(InvokeGetWeaponInterval(game, magicBolt), Is.EqualTo(0.52f).Within(0.001f));
            Assert.That(InvokeGetWeaponRange(game, spinningBlade, 1f), Is.EqualTo(3.0f).Within(0.001f));
            Assert.That(InvokeGetWeaponRange(game, piercingDart, 1f), Is.EqualTo(7.8f).Within(0.001f));
            Assert.That(InvokeGetWeaponSpeed(game, piercingDart, 1f), Is.EqualTo(10.5f).Within(0.001f));
        }

        [Test]
        public void Stage23WeaponUpgradesKeepDistinctOutputProfiles()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(23002);
            SetConfigTables(game, LoadTables());

            InvokeAddOrUpgradeWeapon(game, RoguelikeWeaponType.MagicBolt);
            InvokeAddOrUpgradeWeapon(game, RoguelikeWeaponType.MagicBolt);
            InvokeAddOrUpgradeWeapon(game, RoguelikeWeaponType.SpinningBlade);
            InvokeAddOrUpgradeWeapon(game, RoguelikeWeaponType.SpinningBlade);
            InvokeAddOrUpgradeWeapon(game, RoguelikeWeaponType.PiercingDart);
            InvokeAddOrUpgradeWeapon(game, RoguelikeWeaponType.PiercingDart);

            RoguelikeSurvivalWeapon magicBolt = FindWeapon(game, RoguelikeWeaponType.MagicBolt);
            RoguelikeSurvivalWeapon spinningBlade = FindWeapon(game, RoguelikeWeaponType.SpinningBlade);
            RoguelikeSurvivalWeapon piercingDart = FindWeapon(game, RoguelikeWeaponType.PiercingDart);

            Assert.That(magicBolt.Level, Is.EqualTo(3));
            Assert.That(spinningBlade.Level, Is.EqualTo(2));
            Assert.That(piercingDart.Level, Is.EqualTo(2));
            Assert.That(InvokeGetWeaponDamage(game, magicBolt, 1), Is.EqualTo(21));
            Assert.That(InvokeGetWeaponDamage(game, spinningBlade, 1), Is.EqualTo(11));
            Assert.That(InvokeGetWeaponDamage(game, piercingDart, 1), Is.EqualTo(15));
            Assert.That(InvokeGetWeaponInterval(game, magicBolt), Is.LessThan(0.52f));
            Assert.That(InvokeGetWeaponInterval(game, spinningBlade), Is.LessThan(0.78f));
            Assert.That(InvokeGetWeaponInterval(game, piercingDart), Is.LessThan(0.82f));
            Assert.That(InvokeGetWeaponRange(game, piercingDart, 1f), Is.GreaterThan(InvokeGetWeaponRange(game, magicBolt, 1f)));
        }

        [Test]
        public void Stage23AllThreeWeaponsCanFireInOneRun()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(23003);
            SetConfigTables(game, LoadTables());
            InvokeAddOrUpgradeWeapon(game, RoguelikeWeaponType.SpinningBlade);
            InvokeAddOrUpgradeWeapon(game, RoguelikeWeaponType.PiercingDart);

            GetEnemyList(game).Add(new RoguelikeSurvivalEnemy(23003, new Vector2(2.5f, 0f), 999, 1, 0f));
            game.Tick(0.1f);

            Assert.That(game.Projectiles.Count, Is.GreaterThanOrEqualTo(7));
            Assert.That(CountProjectiles(game, RoguelikeWeaponType.MagicBolt), Is.EqualTo(1));
            Assert.That(CountProjectiles(game, RoguelikeWeaponType.SpinningBlade), Is.GreaterThanOrEqualTo(5));
            Assert.That(CountProjectiles(game, RoguelikeWeaponType.PiercingDart), Is.EqualTo(1));
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

        private static void InvokeAddOrUpgradeWeapon(RoguelikeGame game, RoguelikeWeaponType type)
        {
            MethodInfo method = typeof(RoguelikeGame).GetMethod("AddOrUpgradeWeapon", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(method);
            method.Invoke(game, new object[] { type });
        }

        private static float InvokeGetWeaponInterval(RoguelikeGame game, RoguelikeSurvivalWeapon weapon)
        {
            MethodInfo method = typeof(RoguelikeGame).GetMethod("GetWeaponInterval", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(method);
            return (float)method.Invoke(game, new object[] { weapon });
        }

        private static int InvokeGetWeaponDamage(RoguelikeGame game, RoguelikeSurvivalWeapon weapon, int fallback)
        {
            MethodInfo method = typeof(RoguelikeGame).GetMethod("GetWeaponDamage", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(method);
            return (int)method.Invoke(game, new object[] { weapon, fallback });
        }

        private static float InvokeGetWeaponRange(RoguelikeGame game, RoguelikeSurvivalWeapon weapon, float fallback)
        {
            MethodInfo method = typeof(RoguelikeGame).GetMethod("GetWeaponRange", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(method);
            return (float)method.Invoke(game, new object[] { weapon, fallback });
        }

        private static float InvokeGetWeaponSpeed(RoguelikeGame game, RoguelikeSurvivalWeapon weapon, float fallback)
        {
            MethodInfo method = typeof(RoguelikeGame).GetMethod("GetWeaponSpeed", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(method);
            return (float)method.Invoke(game, new object[] { weapon, fallback });
        }

        private static RoguelikeSurvivalWeapon FindWeapon(RoguelikeGame game, RoguelikeWeaponType type)
        {
            for (int i = 0; i < game.Weapons.Count; i++)
            {
                if (game.Weapons[i].Type == type)
                {
                    return game.Weapons[i];
                }
            }

            Assert.Fail($"Weapon {type} not found.");
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

        private static System.Collections.Generic.List<RoguelikeSurvivalEnemy> GetEnemyList(RoguelikeGame game)
        {
            FieldInfo field = typeof(RoguelikeGame).GetField("_enemies", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            return (System.Collections.Generic.List<RoguelikeSurvivalEnemy>)field.GetValue(game);
        }

        private static void SetConfigTables(RoguelikeGame game, Tables tables)
        {
            FieldInfo field = typeof(RoguelikeGame).GetField("_configTables", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            field.SetValue(game, tables);
        }
    }
}
