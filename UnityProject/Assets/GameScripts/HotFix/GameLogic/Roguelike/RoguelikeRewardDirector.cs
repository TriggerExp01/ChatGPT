using System;
using System.Collections.Generic;
using GameConfig.roguelike;
using UnityEngine;

namespace GameLogic
{
    public sealed class RoguelikeRewardDirector
    {
        public delegate void ConfigEffectApplier(RoguelikeRunState run, IReadOnlyList<Effect> effects);

        public delegate void PassiveAdder(RoguelikeRunState run, string id, string displayName, string description, Action<RoguelikeRunState> apply);

        public delegate void WeaponMutator(RoguelikeWeaponType type);

        public sealed class Context
        {
            public GameConfig.Tables Tables { get; set; }
            public IReadOnlyList<RoguelikeSurvivalWeapon> Weapons { get; set; }
            public Action AddMoveSpeedReward { get; set; }
            public Action AddAttackRangeReward { get; set; }
            public Action AddAttackFrequencyReward { get; set; }
            public Action AddFallbackPickupRadiusPassive { get; set; }
            public Action AddFallbackProjectileDamagePassive { get; set; }
            public Action AddFallbackMoveSpeedPassive { get; set; }
            public ConfigEffectApplier ApplyConfigEffects { get; set; }
            public PassiveAdder AddPassive { get; set; }
            public WeaponMutator AddOrUpgradeWeapon { get; set; }
        }

        public void BuildLevelUpOptions(List<RoguelikeChoiceOption> rewardOptions, Context context, System.Random random)
        {
            if (rewardOptions == null)
            {
                return;
            }

            List<RoguelikeChoiceOption> pool = CreateLevelUpChoicePool(context);
            rewardOptions.Clear();
            while (rewardOptions.Count < 3 && pool.Count > 0)
            {
                int index = random?.Next(pool.Count) ?? 0;
                rewardOptions.Add(pool[index]);
                pool.RemoveAt(index);
            }
        }

        public List<RoguelikeChoiceOption> CreateLevelUpChoicePool(Context context)
        {
            List<RoguelikeChoiceOption> pool = new List<RoguelikeChoiceOption>();
            if (!AddConfiguredRewardChoices(pool, context))
            {
                pool.Add(new RoguelikeChoiceOption("attack", "锋利武器", "攻击力 +3", run => run.Player.Stats.AddAttack(3)));
                pool.Add(new RoguelikeChoiceOption("health", "强健体魄", "最大生命 +20，并恢复 20", run =>
                {
                    run.Player.Stats.AddMaxHealth(20);
                    run.Player.Heal(20);
                }));
            }

            pool.Add(new RoguelikeChoiceOption("speed", "轻盈步伐", "移动速度 +10%", run => context?.AddMoveSpeedReward?.Invoke()));
            pool.Add(new RoguelikeChoiceOption("range", "延伸攻击", "攻击范围 +15%", run => context?.AddAttackRangeReward?.Invoke()));
            pool.Add(new RoguelikeChoiceOption("frequency", "快速攻击", "攻击频率 +12%", run => context?.AddAttackFrequencyReward?.Invoke()));
            pool.Add(new RoguelikeChoiceOption("paid_field_ration", "战地补给", "花费金币，立即恢复 35 生命", run => run.Player.Heal(35), 8));

            AddWeaponChoice(pool, context, RoguelikeWeaponType.MagicBolt);
            AddWeaponChoice(pool, context, RoguelikeWeaponType.SpinningBlade);
            AddWeaponChoice(pool, context, RoguelikeWeaponType.PiercingDart);
            AddWeaponChoice(pool, context, RoguelikeWeaponType.StarRingPulse);
            AddPassiveChoices(pool, context);
            return pool;
        }

        public bool AddConfiguredRewardChoices(List<RoguelikeChoiceOption> pool, Context context)
        {
            List<RoguelikeChoice> choices = context?.Tables?.TbRoguelikeChoice?.DataList;
            if (choices == null || choices.Count <= 0)
            {
                return false;
            }

            int added = 0;
            for (int i = 0; i < choices.Count; i++)
            {
                RoguelikeChoice config = choices[i];
                if (config.PoolType != EChoicePool.Reward)
                {
                    continue;
                }

                pool.Add(new RoguelikeChoiceOption(
                    config.Id,
                    config.Title,
                    config.Desc,
                    run => context?.ApplyConfigEffects?.Invoke(run, config.Effects),
                    config.Cost));
                added++;
            }

            return added > 0;
        }

        public void AddWeaponChoice(List<RoguelikeChoiceOption> pool, Context context, RoguelikeWeaponType type)
        {
            RoguelikeSurvivalWeapon weapon = FindWeapon(context?.Weapons, type);
            if (weapon == null)
            {
                if (type != RoguelikeWeaponType.MagicBolt)
                {
                    string displayName = GetWeaponDisplayName(type);
                    pool.Add(new RoguelikeChoiceOption(
                        $"weapon_{GetWeaponConfigId(type)}_unlock",
                        $"解锁{displayName}",
                        $"新增武器：{displayName}",
                        run => context?.AddOrUpgradeWeapon?.Invoke(type)));
                }

                return;
            }

            if (weapon.IsMaxLevel)
            {
                return;
            }

            pool.Add(new RoguelikeChoiceOption(
                $"weapon_{GetWeaponConfigId(type)}_upgrade",
                $"{weapon.DisplayName}升级",
                $"{weapon.DisplayName}升至 {weapon.Level + 1} 级",
                run => context?.AddOrUpgradeWeapon?.Invoke(type)));
        }

        public void AddPassiveChoices(List<RoguelikeChoiceOption> pool, Context context)
        {
            if (AddConfiguredPassiveChoices(pool, context))
            {
                return;
            }

            pool.Add(new RoguelikeChoiceOption(
                "passive_magnet_core",
                "磁力核心",
                "拾取吸附范围 +0.8",
                run => context?.AddPassive?.Invoke(run, "passive_magnet_core", "磁力核心", "拾取吸附范围提升。", _ => context?.AddFallbackPickupRadiusPassive?.Invoke())));
            pool.Add(new RoguelikeChoiceOption(
                "passive_focus_charm",
                "聚能护符",
                "所有投射物伤害 +12%",
                run => context?.AddPassive?.Invoke(run, "passive_focus_charm", "聚能护符", "投射物伤害提升。", _ => context?.AddFallbackProjectileDamagePassive?.Invoke())));
            pool.Add(new RoguelikeChoiceOption(
                "passive_wind_boots",
                "疾风靴",
                "移动速度 +8%",
                run => context?.AddPassive?.Invoke(run, "passive_wind_boots", "疾风靴", "移动速度提升。", _ => context?.AddFallbackMoveSpeedPassive?.Invoke())));
        }

        public bool AddConfiguredPassiveChoices(List<RoguelikeChoiceOption> pool, Context context)
        {
            List<RoguelikeRelic> relics = context?.Tables?.TbRoguelikeRelic?.DataList;
            if (relics == null || relics.Count <= 0)
            {
                return false;
            }

            for (int i = 0; i < relics.Count; i++)
            {
                RoguelikeRelic relic = relics[i];
                pool.Add(new RoguelikeChoiceOption(
                    $"passive_{relic.Id}",
                    relic.DisplayName,
                    relic.Desc,
                    run => AddPassiveFromConfig(run, relic, context)));
            }

            return true;
        }

        public void AddPassiveFromConfig(RoguelikeRunState run, RoguelikeRelic relic, Context context)
        {
            if (relic == null)
            {
                return;
            }

            run?.AddRelic(new RoguelikeRelicTemplate(
                relic.Id,
                relic.DisplayName,
                relic.Desc,
                state => context?.ApplyConfigEffects?.Invoke(state, relic.Effects)));
        }

        private static RoguelikeSurvivalWeapon FindWeapon(IReadOnlyList<RoguelikeSurvivalWeapon> weapons, RoguelikeWeaponType type)
        {
            if (weapons == null)
            {
                return null;
            }

            for (int i = 0; i < weapons.Count; i++)
            {
                if (weapons[i].Type == type)
                {
                    return weapons[i];
                }
            }

            return null;
        }

        private static string GetWeaponDisplayName(RoguelikeWeaponType type)
        {
            switch (type)
            {
                case RoguelikeWeaponType.SpinningBlade:
                    return "旋刃";
                case RoguelikeWeaponType.PiercingDart:
                    return "穿透飞镖";
                case RoguelikeWeaponType.StarRingPulse:
                    return "星环脉冲";
                default:
                    return "追踪魔弹";
            }
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
    }
}
