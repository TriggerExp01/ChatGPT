using System.Collections.Generic;

namespace GameLogic
{
    public sealed class RoguelikeChoiceCatalog
    {
        public List<RoguelikeChoiceOption> CreateChoicePool(RoguelikeRoomType roomType)
        {
            switch (roomType)
            {
                case RoguelikeRoomType.Rest:
                    return BuildRestPool();
                case RoguelikeRoomType.Treasure:
                    return BuildTreasurePool();
                case RoguelikeRoomType.Shop:
                    return BuildShopPool();
                default:
                    return BuildRewardPool();
            }
        }

        public RoguelikeChoiceOption CreateLeaveShopOption()
        {
            return new RoguelikeChoiceOption("shop_leave", "离开商店", "保留金币", run => { });
        }

        private static List<RoguelikeChoiceOption> BuildRewardPool()
        {
            return new List<RoguelikeChoiceOption>
            {
                new RoguelikeChoiceOption("atk_2", "磨利刀刃", "攻击 +2", run => run.Player.Stats.AddAttack(2)),
                new RoguelikeChoiceOption("def_1", "防守架势", "防御 +1", run => run.Player.Stats.AddDefense(1)),
                new RoguelikeChoiceOption("heal_25", "野外口粮", "恢复 25 点生命", run => run.Player.Heal(25)),
                new RoguelikeChoiceOption("max_hp_10", "坚韧皮肤", "生命上限 +10，并恢复 10 点生命", run =>
                {
                    run.Player.Stats.AddMaxHealth(10);
                    run.Player.Heal(10);
                }),
                new RoguelikeChoiceOption("crit_5", "敏锐目光", "暴击 +5%", run => run.Player.Stats.AddCritChance(0.05f)),
                new RoguelikeChoiceOption("gold_25", "金币袋", "金币 +25", run => run.AddGold(25)),
            };
        }

        private static List<RoguelikeChoiceOption> BuildRestPool()
        {
            return new List<RoguelikeChoiceOption>
            {
                new RoguelikeChoiceOption("camp_deep_rest", "深度休整", "恢复 35 点生命", run => run.Player.Heal(35)),
                new RoguelikeChoiceOption("camp_weapon_drill", "武器演练", "攻击 +1，防御 +1", run =>
                {
                    run.Player.Stats.AddAttack(1);
                    run.Player.Stats.AddDefense(1);
                }),
                new RoguelikeChoiceOption("camp_fortify", "加固护具", "生命上限 +8，并恢复 8 点生命", run =>
                {
                    run.Player.Stats.AddMaxHealth(8);
                    run.Player.Heal(8);
                }),
                new RoguelikeChoiceOption("camp_focus", "凝神", "暴击 +4%", run => run.Player.Stats.AddCritChance(0.04f)),
            };
        }

        private static List<RoguelikeChoiceOption> BuildTreasurePool()
        {
            return new List<RoguelikeChoiceOption>
            {
                new RoguelikeChoiceOption("treasure_gold_35", "散落金币", "金币 +35", run => run.AddGold(35)),
                new RoguelikeChoiceOption("treasure_keen_gem", "锋锐宝石", "暴击 +7%", run => run.Player.Stats.AddCritChance(0.07f)),
                new RoguelikeChoiceOption("treasure_guard_plate", "守护甲片", "防御 +1，金币 +10", run =>
                {
                    run.Player.Stats.AddDefense(1);
                    run.AddGold(10);
                }),
                new RoguelikeChoiceOption("treasure_war_charm", "战斗护符", "攻击 +2", run => run.Player.Stats.AddAttack(2)),
            };
        }

        private static List<RoguelikeChoiceOption> BuildShopPool()
        {
            return new List<RoguelikeChoiceOption>
            {
                new RoguelikeChoiceOption("shop_atk_3", "磨刀石", "攻击 +3", run => run.Player.Stats.AddAttack(3), cost: 24),
                new RoguelikeChoiceOption("shop_def_2", "强化护手", "防御 +2", run => run.Player.Stats.AddDefense(2), cost: 22),
                new RoguelikeChoiceOption("shop_heal_35", "营地补给", "恢复 35 点生命", run => run.Player.Heal(35), cost: 16),
                new RoguelikeChoiceOption("shop_max_hp_14", "生命护符", "生命上限 +14，并恢复 14 点生命", run =>
                {
                    run.Player.Stats.AddMaxHealth(14);
                    run.Player.Heal(14);
                }, cost: 30),
                new RoguelikeChoiceOption("shop_crit_8", "刻痕骰子", "暴击 +8%", run => run.Player.Stats.AddCritChance(0.08f), cost: 20),
            };
        }
    }
}
