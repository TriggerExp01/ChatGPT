using System;
using System.Collections.Generic;
using GameConfig;
using GameConfig.roguelike;

namespace GameLogic
{
    public static class RoguelikeConfigValidator
    {
        public static List<string> Validate(Tables tables, Func<string, bool> resourceExists = null)
        {
            List<string> issues = new List<string>();
            Validate(tables, issues, resourceExists);
            return issues;
        }

        public static void Validate(Tables tables, IList<string> issues, Func<string, bool> resourceExists = null)
        {
            if (issues == null)
            {
                return;
            }

            issues.Clear();
            if (tables == null)
            {
                issues.Add("配置表未加载。");
                return;
            }

            HashSet<string> enemyIds = ValidateEnemies(tables.TbRoguelikeEnemy?.DataList, issues);
            ValidateWeapons(tables.TbRoguelikeWeapon?.DataList, issues);
            ValidateChoices(tables.TbRoguelikeChoice?.DataList, issues);
            ValidateRelics(tables.TbRoguelikeRelic?.DataList, issues);
            ValidateSpawnStages(tables.TbRoguelikeSpawnStage?.DataList, enemyIds, issues);
            ValidateRequiredResources(resourceExists, issues);
        }

        public static IReadOnlyList<string> GetRequiredResourceAddresses()
        {
            return RequiredResourceAddresses;
        }

        private static HashSet<string> ValidateEnemies(IReadOnlyList<RoguelikeEnemy> enemies, IList<string> issues)
        {
            HashSet<string> ids = new HashSet<string>();
            if (enemies == null || enemies.Count <= 0)
            {
                issues.Add("敌人表为空。");
                return ids;
            }

            bool hasCommon = false;
            bool hasBoss = false;
            for (int i = 0; i < enemies.Count; i++)
            {
                RoguelikeEnemy enemy = enemies[i];
                string rowName = enemy != null ? enemy.Id : $"第 {i + 1} 行";
                if (enemy == null)
                {
                    issues.Add("敌人表包含空行。");
                    continue;
                }

                if (!AddRequiredId(ids, enemy.Id, "敌人表", issues))
                {
                    continue;
                }

                if (string.IsNullOrEmpty(enemy.DisplayName))
                {
                    issues.Add($"敌人 `{rowName}` 缺少显示名称。");
                }

                if (enemy.MaxHealth <= 0)
                {
                    issues.Add($"敌人 `{rowName}` 最大生命必须大于 0。");
                }

                if (enemy.Attack < 0)
                {
                    issues.Add($"敌人 `{rowName}` 攻击不能小于 0。");
                }

                if (enemy.Defense < 0)
                {
                    issues.Add($"敌人 `{rowName}` 防御不能小于 0。");
                }

                if (enemy.CritChance < 0f || enemy.CritChance > 1f)
                {
                    issues.Add($"敌人 `{rowName}` 暴击率必须在 0 到 1 之间。");
                }

                if (enemy.CritMultiplier < 0f)
                {
                    issues.Add($"敌人 `{rowName}` 暴击倍率不能小于 0。");
                }

                hasCommon |= enemy.Tier == EEnemyTier.Common;
                hasBoss |= enemy.Tier == EEnemyTier.Boss;
            }

            if (!hasCommon)
            {
                issues.Add("敌人表缺少普通敌人。");
            }

            if (!hasBoss)
            {
                issues.Add("敌人表缺少 Boss 敌人。");
            }

            return ids;
        }

        private static void ValidateWeapons(IReadOnlyList<RoguelikeWeapon> weapons, IList<string> issues)
        {
            HashSet<string> ids = new HashSet<string>();
            HashSet<RoguelikeWeaponType> weaponTypes = new HashSet<RoguelikeWeaponType>();
            if (weapons == null || weapons.Count <= 0)
            {
                issues.Add("武器表为空。");
                return;
            }

            for (int i = 0; i < weapons.Count; i++)
            {
                RoguelikeWeapon weapon = weapons[i];
                string rowName = weapon != null ? weapon.Id : $"第 {i + 1} 行";
                if (weapon == null)
                {
                    issues.Add("武器表包含空行。");
                    continue;
                }

                if (!AddRequiredId(ids, weapon.Id, "武器表", issues))
                {
                    continue;
                }

                if (string.IsNullOrEmpty(weapon.DisplayName))
                {
                    issues.Add($"武器 `{rowName}` 缺少显示名称。");
                }

                if (!Enum.TryParse(weapon.WeaponType, out RoguelikeWeaponType parsedType))
                {
                    issues.Add($"武器 `{rowName}` 的 WeaponType `{weapon.WeaponType}` 无法映射到运行时武器枚举。");
                }
                else if (!weaponTypes.Add(parsedType))
                {
                    issues.Add($"武器表存在重复 WeaponType `{parsedType}`。");
                }

                if (weapon.BaseDamage <= 0)
                {
                    issues.Add($"武器 `{rowName}` 基础伤害必须大于 0。");
                }

                if (weapon.BaseInterval <= 0f)
                {
                    issues.Add($"武器 `{rowName}` 基础间隔必须大于 0。");
                }

                if (weapon.BaseRange <= 0f)
                {
                    issues.Add($"武器 `{rowName}` 基础射程必须大于 0。");
                }

                if (weapon.ProjectileSpeed <= 0f)
                {
                    issues.Add($"武器 `{rowName}` 投射物速度必须大于 0。");
                }
            }

            foreach (RoguelikeWeaponType type in Enum.GetValues(typeof(RoguelikeWeaponType)))
            {
                if (!weaponTypes.Contains(type))
                {
                    issues.Add($"武器表缺少运行时武器 `{type}` 的配置。");
                }
            }
        }

        private static void ValidateChoices(IReadOnlyList<RoguelikeChoice> choices, IList<string> issues)
        {
            HashSet<string> ids = new HashSet<string>();
            if (choices == null || choices.Count <= 0)
            {
                issues.Add("奖励选项表为空。");
                return;
            }

            int rewardCount = 0;
            for (int i = 0; i < choices.Count; i++)
            {
                RoguelikeChoice choice = choices[i];
                string rowName = choice != null ? choice.Id : $"第 {i + 1} 行";
                if (choice == null)
                {
                    issues.Add("奖励选项表包含空行。");
                    continue;
                }

                if (!AddRequiredId(ids, choice.Id, "奖励选项表", issues))
                {
                    continue;
                }

                if (choice.PoolType == EChoicePool.Reward)
                {
                    rewardCount++;
                }

                if (string.IsNullOrEmpty(choice.Title))
                {
                    issues.Add($"奖励选项 `{rowName}` 缺少标题。");
                }

                if (string.IsNullOrEmpty(choice.Desc))
                {
                    issues.Add($"奖励选项 `{rowName}` 缺少描述。");
                }

                if (choice.Cost < 0)
                {
                    issues.Add($"奖励选项 `{rowName}` 费用不能小于 0。");
                }

                ValidateEffects(choice.Effects, $"奖励选项 `{rowName}`", issues);
            }

            if (rewardCount <= 0)
            {
                issues.Add("奖励选项表缺少 Reward 奖励池。");
            }
        }

        private static void ValidateRelics(IReadOnlyList<RoguelikeRelic> relics, IList<string> issues)
        {
            HashSet<string> ids = new HashSet<string>();
            if (relics == null || relics.Count <= 0)
            {
                issues.Add("被动遗物表为空。");
                return;
            }

            for (int i = 0; i < relics.Count; i++)
            {
                RoguelikeRelic relic = relics[i];
                string rowName = relic != null ? relic.Id : $"第 {i + 1} 行";
                if (relic == null)
                {
                    issues.Add("被动遗物表包含空行。");
                    continue;
                }

                if (!AddRequiredId(ids, relic.Id, "被动遗物表", issues))
                {
                    continue;
                }

                if (string.IsNullOrEmpty(relic.DisplayName))
                {
                    issues.Add($"被动遗物 `{rowName}` 缺少显示名称。");
                }

                if (string.IsNullOrEmpty(relic.Desc))
                {
                    issues.Add($"被动遗物 `{rowName}` 缺少描述。");
                }

                ValidateEffects(relic.Effects, $"被动遗物 `{rowName}`", issues);
            }
        }

        private static void ValidateSpawnStages(IReadOnlyList<RoguelikeSpawnStage> stages, HashSet<string> enemyIds, IList<string> issues)
        {
            HashSet<int> ids = new HashSet<int>();
            if (stages == null || stages.Count <= 0)
            {
                issues.Add("刷怪阶段表为空。");
                return;
            }

            float lastStartTime = -1f;
            for (int i = 0; i < stages.Count; i++)
            {
                RoguelikeSpawnStage stage = stages[i];
                string rowName = stage != null ? stage.Id.ToString() : $"第 {i + 1} 行";
                if (stage == null)
                {
                    issues.Add("刷怪阶段表包含空行。");
                    continue;
                }

                if (!ids.Add(stage.Id))
                {
                    issues.Add($"刷怪阶段表存在重复 ID `{stage.Id}`。");
                }

                if (stage.StartTime < 0f)
                {
                    issues.Add($"刷怪阶段 `{rowName}` 开始时间不能小于 0。");
                }

                if (stage.StartTime < lastStartTime)
                {
                    issues.Add($"刷怪阶段 `{rowName}` 开始时间早于前一阶段。");
                }

                lastStartTime = stage.StartTime;
                if (stage.CommonSpawnCount <= 0)
                {
                    issues.Add($"刷怪阶段 `{rowName}` 普通刷怪数量必须大于 0。");
                }

                if (stage.CommonEnemyIds == null || stage.CommonEnemyIds.Count <= 0)
                {
                    issues.Add($"刷怪阶段 `{rowName}` 缺少普通敌人池。");
                }

                if (stage.CommonWeights == null || stage.CommonEnemyIds == null || stage.CommonWeights.Count != stage.CommonEnemyIds.Count)
                {
                    issues.Add($"刷怪阶段 `{rowName}` 普通敌人 ID 与权重数量不一致。");
                }

                if (stage.CommonEnemyIds != null)
                {
                    for (int enemyIndex = 0; enemyIndex < stage.CommonEnemyIds.Count; enemyIndex++)
                    {
                        string enemyId = stage.CommonEnemyIds[enemyIndex];
                        if (string.IsNullOrEmpty(enemyId))
                        {
                            issues.Add($"刷怪阶段 `{rowName}` 包含空普通敌人 ID。");
                        }
                        else if (enemyIds == null || !enemyIds.Contains(enemyId))
                        {
                            issues.Add($"刷怪阶段 `{rowName}` 引用了不存在的普通敌人 `{enemyId}`。");
                        }
                    }
                }

                if (stage.CommonWeights != null)
                {
                    for (int weightIndex = 0; weightIndex < stage.CommonWeights.Count; weightIndex++)
                    {
                        if (stage.CommonWeights[weightIndex] <= 0)
                        {
                            issues.Add($"刷怪阶段 `{rowName}` 普通敌人权重必须大于 0。");
                        }
                    }
                }

                ValidateOptionalEnemyRef(stage.EliteEnemyId, $"刷怪阶段 `{rowName}` 精英敌人", enemyIds, issues);
                ValidateOptionalEnemyRef(stage.BossEnemyId, $"刷怪阶段 `{rowName}` Boss 敌人", enemyIds, issues);
            }
        }

        private static void ValidateEffects(IReadOnlyList<Effect> effects, string owner, IList<string> issues)
        {
            if (effects == null || effects.Count <= 0)
            {
                issues.Add($"{owner} 缺少效果配置。");
                return;
            }

            for (int i = 0; i < effects.Count; i++)
            {
                Effect effect = effects[i];
                if (effect == null)
                {
                    issues.Add($"{owner} 包含空效果。");
                    continue;
                }

                if (effect.Type == EEffectType.None)
                {
                    issues.Add($"{owner} 包含 None 效果类型。");
                }

                if (float.IsNaN(effect.Value) || float.IsInfinity(effect.Value))
                {
                    issues.Add($"{owner} 效果数值非法。");
                }
            }
        }

        private static void ValidateRequiredResources(Func<string, bool> resourceExists, IList<string> issues)
        {
            if (resourceExists == null)
            {
                return;
            }

            for (int i = 0; i < RequiredResourceAddresses.Length; i++)
            {
                string address = RequiredResourceAddresses[i];
                if (!resourceExists(address))
                {
                    issues.Add($"资源地址 `{address}` 不存在或未纳入当前资源校验范围。");
                }
            }
        }

        private static bool AddRequiredId<T>(HashSet<T> ids, T id, string tableName, IList<string> issues)
        {
            if (id == null || string.IsNullOrEmpty(id.ToString()))
            {
                issues.Add($"{tableName} 存在空 ID。");
                return false;
            }

            if (!ids.Add(id))
            {
                issues.Add($"{tableName} 存在重复 ID `{id}`。");
                return false;
            }

            return true;
        }

        private static void ValidateOptionalEnemyRef(string enemyId, string owner, HashSet<string> enemyIds, IList<string> issues)
        {
            if (string.IsNullOrEmpty(enemyId))
            {
                return;
            }

            if (enemyIds == null || !enemyIds.Contains(enemyId))
            {
                issues.Add($"{owner} 引用了不存在的敌人 `{enemyId}`。");
            }
        }

        private static readonly string[] RequiredResourceAddresses =
        {
            RoguelikeGame.HitSoundPath,
            RoguelikeGame.PickupSoundPath,
            RoguelikeGame.LevelUpSoundPath,
            RoguelikeGame.BossSoundPath,
            RoguelikeGame.UiConfirmSoundPath,
            RoguelikeBattleStageView.HitEffectPrefabAddress,
            RoguelikeBattleStageView.KillEffectPrefabAddress,
            RoguelikeBattleStageView.PickupEffectPrefabAddress,
            RoguelikeBattleStageView.CommonEnemyPrefabAddress,
            RoguelikeBattleStageView.BossEnemyPrefabAddress,
            RoguelikeBattleStageView.MagicBoltProjectilePrefabAddress,
            RoguelikeBattleStageView.PiercingDartProjectilePrefabAddress,
        };
    }
}
