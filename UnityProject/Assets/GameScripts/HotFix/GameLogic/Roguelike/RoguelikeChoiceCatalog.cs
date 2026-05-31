using System.Collections.Generic;
using GameConfig;
using GameConfig.roguelike;

namespace GameLogic
{
    public sealed class RoguelikeChoiceCatalog
    {
        private static readonly Dictionary<string, string> ChoiceTitles = new Dictionary<string, string>
        {
            { "atk_2", "磨利武器" },
            { "def_1", "加固护甲" },
            { "heal_25", "应急包扎" },
            { "max_hp_10", "生命训练" },
            { "crit_5", "弱点观察" },
            { "gold_25", "搜刮金币" },
            { "camp_deep_rest", "深度休整" },
            { "camp_weapon_drill", "武器演练" },
            { "camp_fortify", "临时加固" },
            { "camp_focus", "专注冥想" },
            { "treasure_gold_35", "金币袋" },
            { "treasure_keen_gem", "锐利宝石" },
            { "treasure_guard_plate", "守卫甲片" },
            { "treasure_war_charm", "战斗护符" },
            { "shop_atk_3", "购买利刃" },
            { "shop_def_2", "购买护甲" },
            { "shop_heal_35", "购买药剂" },
            { "shop_max_hp_14", "购买强身药" },
            { "shop_crit_8", "购买瞄准镜" },
        };

        private static readonly Dictionary<string, string> ChoiceDescriptions = new Dictionary<string, string>
        {
            { "atk_2", "攻击 +2" },
            { "def_1", "防御 +1" },
            { "heal_25", "恢复 25 生命" },
            { "max_hp_10", "生命 +10，回复 10" },
            { "crit_5", "暴击 +5%" },
            { "gold_25", "金币 +25" },
            { "camp_deep_rest", "恢复 35 生命" },
            { "camp_weapon_drill", "攻击 +1，防御 +1" },
            { "camp_fortify", "生命 +8，回复 8" },
            { "camp_focus", "暴击 +4%" },
            { "treasure_gold_35", "金币 +35" },
            { "treasure_keen_gem", "暴击 +7%" },
            { "treasure_guard_plate", "防御 +1，生命 +10" },
            { "treasure_war_charm", "攻击 +2" },
            { "shop_atk_3", "攻击 +3" },
            { "shop_def_2", "防御 +2" },
            { "shop_heal_35", "恢复 35 生命" },
            { "shop_max_hp_14", "生命 +14，回复 14" },
            { "shop_crit_8", "暴击 +8%" },
        };

        private readonly RoguelikeChoiceOption[] _rewardOptions;
        private readonly RoguelikeChoiceOption[] _restOptions;
        private readonly RoguelikeChoiceOption[] _treasureOptions;
        private readonly RoguelikeChoiceOption[] _shopOptions;

        public RoguelikeChoiceCatalog(Tables tables)
        {
            _rewardOptions = BuildChoices(tables.TbRoguelikeChoice.DataList, EChoicePool.Reward);
            _restOptions = BuildChoices(tables.TbRoguelikeChoice.DataList, EChoicePool.Rest);
            _treasureOptions = BuildChoices(tables.TbRoguelikeChoice.DataList, EChoicePool.Treasure);
            _shopOptions = BuildChoices(tables.TbRoguelikeChoice.DataList, EChoicePool.Shop);
        }

        public List<RoguelikeChoiceOption> CreateChoicePool(RoguelikeRoomType roomType)
        {
            switch (roomType)
            {
                case RoguelikeRoomType.Rest:
                    return new List<RoguelikeChoiceOption>(_restOptions);
                case RoguelikeRoomType.Treasure:
                    return new List<RoguelikeChoiceOption>(_treasureOptions);
                case RoguelikeRoomType.Shop:
                    return new List<RoguelikeChoiceOption>(_shopOptions);
                default:
                    return new List<RoguelikeChoiceOption>(_rewardOptions);
            }
        }

        public RoguelikeChoiceOption CreateLeaveShopOption()
        {
            return new RoguelikeChoiceOption("shop_leave", "离开商店", "保留金币", run => { });
        }

        private static RoguelikeChoiceOption[] BuildChoices(IReadOnlyList<RoguelikeChoice> rows, EChoicePool pool)
        {
            List<RoguelikeChoiceOption> result = new List<RoguelikeChoiceOption>();
            for (int i = 0; i < rows.Count; i++)
            {
                RoguelikeChoice row = rows[i];
                if (row.PoolType != pool)
                {
                    continue;
                }

                result.Add(new RoguelikeChoiceOption(
                    row.Id,
                    ResolveText(ChoiceTitles, row.Id, row.Title),
                    ResolveText(ChoiceDescriptions, row.Id, row.Desc),
                    run => ApplyEffects(run, row.Effects),
                    row.Cost));
            }

            return result.ToArray();
        }

        private static string ResolveText(Dictionary<string, string> values, string id, string fallback)
        {
            return !string.IsNullOrEmpty(id) && values.TryGetValue(id, out string value) ? value : fallback;
        }

        private static void ApplyEffects(RoguelikeRunState run, IReadOnlyList<Effect> effects)
        {
            if (effects == null)
            {
                return;
            }

            for (int i = 0; i < effects.Count; i++)
            {
                RoguelikeEffectResolver.Apply(run, effects[i]);
            }
        }
    }
}
