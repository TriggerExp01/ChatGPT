using System.Collections.Generic;
using System.IO;
using System.Reflection;
using GameConfig;
using Luban;
using NUnit.Framework;
using UnityEngine;

namespace GameLogic.Tests
{
    public sealed class RoguelikeStage66WeaponDirectorTests
    {
        [Test]
        public void Stage66WeaponDirectorFiresMagicBoltWithoutRoguelikeGameState()
        {
            RoguelikeWeaponDirector director = new RoguelikeWeaponDirector();
            List<RoguelikeSurvivalProjectile> projectiles = new List<RoguelikeSurvivalProjectile>();
            Vector2 attackDirection = Vector2.zero;
            float attackFlash = 0f;
            int nextProjectileId = 66001;
            RoguelikeSurvivalWeapon weapon = new RoguelikeSurvivalWeapon(RoguelikeWeaponType.MagicBolt, "追踪魔弹", 2);
            RoguelikeWeaponDirector.Context context = CreateMinimalContext(
                new[] { weapon },
                new[]
                {
                    new RoguelikeSurvivalEnemy(66010, new Vector2(3f, 0f), 30, 1, 0f),
                    new RoguelikeSurvivalEnemy(66011, new Vector2(1.5f, 0f), 30, 1, 0f),
                },
                projectiles,
                () => nextProjectileId++,
                direction => attackDirection = direction,
                value => attackFlash = value);

            bool fired = director.FireMagicBolt(context, weapon);

            Assert.IsTrue(fired);
            Assert.That(projectiles.Count, Is.EqualTo(1));
            Assert.That(projectiles[0].Id, Is.EqualTo(66001));
            Assert.That(projectiles[0].WeaponType, Is.EqualTo(RoguelikeWeaponType.MagicBolt));
            Assert.That(projectiles[0].Damage, Is.GreaterThan(0));
            Assert.That(attackDirection.x, Is.GreaterThan(0.99f));
            Assert.That(attackFlash, Is.EqualTo(0.12f).Within(0.001f));
        }

        [Test]
        public void Stage66WeaponDirectorUpdatesCooldownAndPrimaryTimer()
        {
            RoguelikeWeaponDirector director = new RoguelikeWeaponDirector();
            List<RoguelikeSurvivalProjectile> projectiles = new List<RoguelikeSurvivalProjectile>();
            RoguelikeSurvivalWeapon weapon = new RoguelikeSurvivalWeapon(RoguelikeWeaponType.SpinningBlade, "旋刃", 1);
            weapon.CooldownRemaining = 0f;
            RoguelikeWeaponDirector.Context context = CreateMinimalContext(
                new[] { weapon },
                new[] { new RoguelikeSurvivalEnemy(66020, new Vector2(2f, 0f), 30, 1, 0f) },
                projectiles,
                null,
                null,
                null);

            float primaryCooldown = director.UpdateWeapons(context, 0.25f);

            Assert.That(projectiles.Count, Is.EqualTo(5));
            Assert.That(weapon.CooldownRemaining, Is.GreaterThan(0f));
            Assert.That(primaryCooldown, Is.EqualTo(0f));
        }

        [Test]
        public void Stage66RoguelikeGamePrivateWeaponBoundaryStillDelegatesToWeaponDirector()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(66002);
            SetConfigTables(game, LoadTables());
            GetEnemyList(game).Add(new RoguelikeSurvivalEnemy(66030, new Vector2(2.5f, 0f), 999, 1, 0f));
            RoguelikeSurvivalWeapon weapon = game.Weapons[0];

            bool fired = InvokeFireMagicBolt(game, weapon);

            Assert.IsTrue(fired);
            Assert.That(game.Projectiles.Count, Is.EqualTo(1));
            Assert.That(game.Projectiles[0].WeaponType, Is.EqualTo(RoguelikeWeaponType.MagicBolt));
            Assert.That(game.AttackDirection.x, Is.GreaterThan(0.99f));
            Assert.That(game.AttackFlash, Is.EqualTo(0.12f).Within(0.001f));
        }

        private static RoguelikeWeaponDirector.Context CreateMinimalContext(
            IReadOnlyList<RoguelikeSurvivalWeapon> weapons,
            IReadOnlyList<RoguelikeSurvivalEnemy> enemies,
            List<RoguelikeSurvivalProjectile> projectiles,
            RoguelikeWeaponDirector.ProjectileIdProvider nextProjectileId,
            System.Action<Vector2> setAttackDirection,
            System.Action<float> setAttackFlash)
        {
            int fallbackProjectileId = 1;
            return new RoguelikeWeaponDirector.Context
            {
                Tables = LoadTables(),
                Weapons = weapons,
                Enemies = enemies,
                PlayerPosition = Vector2.zero,
                AttackRange = 5.5f,
                AttackInterval = 0.55f,
                PlayerAttack = 12,
                Random = new System.Random(66003),
                NextProjectileId = nextProjectileId ?? (() => fallbackProjectileId++),
                CreateProjectile = (id, type, position, direction, speed, distance, damage) => new RoguelikeSurvivalProjectile(id, type, position, direction, speed, distance, damage.Amount, damage.IsCritical),
                AddProjectile = projectile => projectiles.Add(projectile),
                RollProjectileDamage = damage => new RoguelikeDamageRoll(damage, false),
                SetAttackDirection = setAttackDirection,
                SetAttackFlash = setAttackFlash,
                SetLastMessage = _ => { },
            };
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

        private static bool InvokeFireMagicBolt(RoguelikeGame game, RoguelikeSurvivalWeapon weapon)
        {
            MethodInfo method = typeof(RoguelikeGame).GetMethod("FireMagicBolt", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(method);
            return (bool)method.Invoke(game, new object[] { weapon });
        }

        private static List<RoguelikeSurvivalEnemy> GetEnemyList(RoguelikeGame game)
        {
            FieldInfo field = typeof(RoguelikeGame).GetField("_enemies", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            return (List<RoguelikeSurvivalEnemy>)field.GetValue(game);
        }

        private static void SetConfigTables(RoguelikeGame game, Tables tables)
        {
            FieldInfo field = typeof(RoguelikeGame).GetField("_configTables", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            field.SetValue(game, tables);
        }
    }
}
