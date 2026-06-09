using System;
using System.Collections.Generic;
using GameConfig.roguelike;
using UnityEngine;

namespace GameLogic
{
    public sealed class RoguelikeWeaponDirector
    {
        public delegate int ProjectileIdProvider();

        public delegate RoguelikeSurvivalProjectile ProjectileFactory(int id, RoguelikeWeaponType weaponType, Vector2 position, Vector2 direction, float speed, float distance, RoguelikeDamageRoll damage);

        public delegate RoguelikeDamageRoll DamageRoller(int damage);

        public sealed class Context
        {
            public GameConfig.Tables Tables { get; set; }
            public IReadOnlyList<RoguelikeSurvivalWeapon> Weapons { get; set; }
            public IReadOnlyList<RoguelikeSurvivalEnemy> Enemies { get; set; }
            public Vector2 PlayerPosition { get; set; }
            public float AttackRange { get; set; }
            public float AttackInterval { get; set; }
            public int PlayerAttack { get; set; }
            public System.Random Random { get; set; }
            public ProjectileIdProvider NextProjectileId { get; set; }
            public ProjectileFactory CreateProjectile { get; set; }
            public Action<RoguelikeSurvivalProjectile> AddProjectile { get; set; }
            public DamageRoller RollProjectileDamage { get; set; }
            public Action<Vector2> SetAttackDirection { get; set; }
            public Action<float> SetAttackFlash { get; set; }
            public Action<string> SetLastMessage { get; set; }
        }

        public float UpdateWeapons(Context context, float dt)
        {
            IReadOnlyList<RoguelikeSurvivalWeapon> weapons = context?.Weapons;
            if (weapons == null)
            {
                return context?.AttackInterval ?? 0f;
            }

            for (int i = 0; i < weapons.Count; i++)
            {
                RoguelikeSurvivalWeapon weapon = weapons[i];
                weapon.CooldownRemaining = Mathf.Max(0f, weapon.CooldownRemaining - dt);
                if (weapon.CooldownRemaining > 0f)
                {
                    continue;
                }

                bool fired = FireWeapon(context, weapon);
                if (fired)
                {
                    weapon.CooldownRemaining = GetWeaponInterval(context, weapon);
                }
            }

            return GetPrimaryCooldown(weapons);
        }

        public bool FireWeapon(Context context, RoguelikeSurvivalWeapon weapon)
        {
            if (context == null || weapon == null)
            {
                return false;
            }

            switch (weapon.Type)
            {
                case RoguelikeWeaponType.MagicBolt:
                    return FireMagicBolt(context, weapon);
                case RoguelikeWeaponType.SpinningBlade:
                    return FireSpinningBlade(context, weapon);
                case RoguelikeWeaponType.PiercingDart:
                    return FirePiercingDart(context, weapon);
                case RoguelikeWeaponType.StarRingPulse:
                    return FireStarRingPulse(context, weapon);
                default:
                    return false;
            }
        }

        public bool FireMagicBolt(Context context, RoguelikeSurvivalWeapon weapon)
        {
            RoguelikeSurvivalEnemy target = FindNearestAliveEnemy(context, context.AttackRange * context.AttackRange);
            if (target == null)
            {
                return false;
            }

            Vector2 attackDirection = (target.Position - context.PlayerPosition).normalized;
            context.SetAttackFlash?.Invoke(0.12f);
            context.SetAttackDirection?.Invoke(attackDirection);
            AddProjectile(context, weapon, RoguelikeWeaponType.MagicBolt, attackDirection,
                GetWeaponSpeed(context, weapon, 9f + weapon.Level * 0.4f),
                GetWeaponRange(context, weapon, context.AttackRange + (weapon.Level - 1) * 0.35f),
                GetWeaponDamage(context, weapon, context.PlayerAttack + (weapon.Level - 1) * 2));
            return true;
        }

        public bool FireSpinningBlade(Context context, RoguelikeSurvivalWeapon weapon)
        {
            if (context.Enemies == null || context.Enemies.Count <= 0)
            {
                return false;
            }

            int bladeCount = 4 + weapon.Level;
            int baseDamage = GetWeaponDamage(context, weapon, Mathf.Max(1, Mathf.RoundToInt(context.PlayerAttack * 0.55f) + weapon.Level));
            float range = GetWeaponRange(context, weapon, 2.2f + weapon.Level * 0.12f);
            float angleOffset = NextAngleOffset(context);
            for (int i = 0; i < bladeCount; i++)
            {
                float angle = angleOffset + 360f * i / bladeCount;
                Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                AddProjectile(context, weapon, RoguelikeWeaponType.SpinningBlade, direction, GetWeaponSpeed(context, weapon, 6.5f), range, baseDamage);
            }

            context.SetAttackFlash?.Invoke(0.10f);
            context.SetLastMessage?.Invoke($"旋刃齐射，发射 {bladeCount} 枚刀刃。");
            return true;
        }

        public bool FirePiercingDart(Context context, RoguelikeSurvivalWeapon weapon)
        {
            RoguelikeSurvivalEnemy target = FindNearestAliveEnemy(context, context.AttackRange * context.AttackRange * 1.8f);
            if (target == null)
            {
                return false;
            }

            Vector2 attackDirection = (target.Position - context.PlayerPosition).normalized;
            context.SetAttackDirection?.Invoke(attackDirection);
            AddProjectile(context, weapon, RoguelikeWeaponType.PiercingDart, attackDirection,
                GetWeaponSpeed(context, weapon, 10f),
                GetWeaponRange(context, weapon, context.AttackRange * 1.25f),
                GetWeaponDamage(context, weapon, Mathf.Max(1, context.PlayerAttack - 1 + weapon.Level)));
            context.SetAttackFlash?.Invoke(0.10f);
            context.SetLastMessage?.Invoke("穿透飞镖出手。");
            return true;
        }

        public bool FireStarRingPulse(Context context, RoguelikeSurvivalWeapon weapon)
        {
            if (context.Enemies == null || context.Enemies.Count <= 0)
            {
                return false;
            }

            int pulseCount = 6 + weapon.Level * 2;
            int baseDamage = GetWeaponDamage(context, weapon, Mathf.Max(1, Mathf.RoundToInt(context.PlayerAttack * 0.45f) + weapon.Level));
            float range = GetWeaponRange(context, weapon, 1.8f + weapon.Level * 0.10f);
            float angleOffset = NextAngleOffset(context);
            for (int i = 0; i < pulseCount; i++)
            {
                float angle = angleOffset + 360f * i / pulseCount;
                Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                AddProjectile(context, weapon, RoguelikeWeaponType.StarRingPulse, direction, GetWeaponSpeed(context, weapon, 5.6f), range, baseDamage);
            }

            context.SetAttackFlash?.Invoke(0.14f);
            context.SetLastMessage?.Invoke($"星环脉冲展开，释放 {pulseCount} 道星环。");
            return true;
        }

        public float GetWeaponInterval(Context context, RoguelikeSurvivalWeapon weapon)
        {
            RoguelikeWeapon config = GetWeaponConfig(context, weapon.Type);
            if (config != null)
            {
                return Mathf.Max(0.12f, config.BaseInterval - (weapon.Level - 1) * config.IntervalGrowth);
            }

            switch (weapon.Type)
            {
                case RoguelikeWeaponType.MagicBolt:
                    return Mathf.Max(0.12f, context.AttackInterval * Mathf.Pow(0.94f, weapon.Level - 1));
                case RoguelikeWeaponType.SpinningBlade:
                    return Mathf.Max(0.65f, 2.2f - weapon.Level * 0.16f);
                case RoguelikeWeaponType.PiercingDart:
                    return Mathf.Max(0.35f, 1.7f - weapon.Level * 0.10f);
                case RoguelikeWeaponType.StarRingPulse:
                    return Mathf.Max(0.45f, 1.15f - weapon.Level * 0.08f);
                default:
                    return context.AttackInterval;
            }
        }

        public int GetWeaponDamage(Context context, RoguelikeSurvivalWeapon weapon, int fallback)
        {
            RoguelikeWeapon config = GetWeaponConfig(context, weapon.Type);
            if (config == null)
            {
                return fallback;
            }

            return Mathf.Max(1, config.BaseDamage + (weapon.Level - 1) * config.DamageGrowth);
        }

        public float GetWeaponRange(Context context, RoguelikeSurvivalWeapon weapon, float fallback)
        {
            RoguelikeWeapon config = GetWeaponConfig(context, weapon.Type);
            if (config == null)
            {
                return fallback;
            }

            return Mathf.Max(0.5f, config.BaseRange + (weapon.Level - 1) * 0.25f);
        }

        public float GetWeaponSpeed(Context context, RoguelikeSurvivalWeapon weapon, float fallback)
        {
            RoguelikeWeapon config = GetWeaponConfig(context, weapon.Type);
            return config == null ? fallback : Mathf.Max(0.5f, config.ProjectileSpeed);
        }

        private RoguelikeSurvivalEnemy FindNearestAliveEnemy(Context context, float nearestDistance)
        {
            RoguelikeSurvivalEnemy target = null;
            IReadOnlyList<RoguelikeSurvivalEnemy> enemies = context.Enemies;
            if (enemies == null)
            {
                return null;
            }

            for (int i = 0; i < enemies.Count; i++)
            {
                RoguelikeSurvivalEnemy enemy = enemies[i];
                Vector2 offset = enemy.Position - context.PlayerPosition;
                float distance = offset.sqrMagnitude;
                if (!enemy.IsAlive || distance > nearestDistance)
                {
                    continue;
                }

                target = enemy;
                nearestDistance = distance;
            }

            return target;
        }

        private void AddProjectile(Context context, RoguelikeSurvivalWeapon weapon, RoguelikeWeaponType weaponType, Vector2 direction, float speed, float range, int damage)
        {
            RoguelikeSurvivalProjectile projectile = context.CreateProjectile?.Invoke(
                context.NextProjectileId?.Invoke() ?? 0,
                weaponType,
                context.PlayerPosition,
                direction,
                speed,
                range,
                context.RollProjectileDamage?.Invoke(damage) ?? new RoguelikeDamageRoll(damage, false));
            if (projectile != null)
            {
                context.AddProjectile?.Invoke(projectile);
            }
        }

        private static RoguelikeWeapon GetWeaponConfig(Context context, RoguelikeWeaponType type)
        {
            List<RoguelikeWeapon> weapons = context?.Tables?.TbRoguelikeWeapon?.DataList;
            if (weapons == null || weapons.Count <= 0)
            {
                return null;
            }

            string id = GetWeaponConfigId(type);
            for (int i = 0; i < weapons.Count; i++)
            {
                if (weapons[i].Id == id)
                {
                    return weapons[i];
                }
            }

            return null;
        }

        private static float GetPrimaryCooldown(IReadOnlyList<RoguelikeSurvivalWeapon> weapons)
        {
            if (weapons == null)
            {
                return 0f;
            }

            for (int i = 0; i < weapons.Count; i++)
            {
                if (weapons[i].Type == RoguelikeWeaponType.MagicBolt)
                {
                    return weapons[i].CooldownRemaining;
                }
            }

            return 0f;
        }

        private static string GetWeaponConfigId(RoguelikeWeaponType type)
        {
            switch (type)
            {
                case RoguelikeWeaponType.SpinningBlade:
                    return "spinning_blade";
                case RoguelikeWeaponType.PiercingDart:
                    return "piercing_dart";
                case RoguelikeWeaponType.StarRingPulse:
                    return "star_ring_pulse";
                default:
                    return "magic_bolt";
            }
        }

        private static float NextAngleOffset(Context context)
        {
            return (float)(context.Random?.NextDouble() ?? 0.0) * 360f;
        }
    }
}
