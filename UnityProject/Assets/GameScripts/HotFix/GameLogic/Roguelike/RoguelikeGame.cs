using System;
using System.Collections.Generic;
using TEngine;

namespace GameLogic
{
    public sealed class RoguelikeGame : Singleton<RoguelikeGame>
    {
        private RoguelikeRunService _runService;

        public RoguelikeRunState CurrentRun { get; private set; }

        public string LastMessage { get; private set; } = "准备就绪。";
        
        public RoguelikeGamePhase Phase { get; private set; }

        public IReadOnlyList<RoguelikeChoiceOption> RewardOptions => _rewardOptions;

        private readonly List<RoguelikeChoiceOption> _rewardOptions = new List<RoguelikeChoiceOption>(3);

        protected override void OnInit()
        {
            _runService = new RoguelikeRunService(RoguelikeContentCatalog.CreateDefault());
        }

        public void StartNewRun()
        {
            int seed = unchecked((int)DateTime.UtcNow.Ticks);
            StartNewRun(seed);
        }

        public void StartNewRun(int seed)
        {
            RoguelikeRunConfig config = RoguelikeRunConfig.CreateDefault(seed);
            CurrentRun = _runService.CreateRun(config);
            LastMessage = $"新的冒险开始。种子 {seed}。";
            Phase = RoguelikeGamePhase.Running;
            _rewardOptions.Clear();

            Log.Info("肉鸽冒险已创建。");
            Log.Info(_runService.BuildRunPreview(CurrentRun));
        }

        public RoguelikeCombatResult ResolveCurrentRoom()
        {
            if (CurrentRun == null)
            {
                StartNewRun();
            }

            RoguelikeCombatResult result = _runService.ResolveCurrentRoom(CurrentRun);
            Log.Info(result.Summary);
            return result;
        }

        public RoguelikeCombatResult TickAutoBattle()
        {
            if (CurrentRun == null)
            {
                StartNewRun();
            }

            if (Phase == RoguelikeGamePhase.RewardChoice)
            {
                LastMessage = "请选择一个奖励继续。";
                return new RoguelikeCombatResult(true, 0, LastMessage);
            }

            if (!CurrentRun.Player.IsAlive)
            {
                Phase = RoguelikeGamePhase.Defeated;
                LastMessage = "冒险失败。点击重新开始。";
                return new RoguelikeCombatResult(false, 0, LastMessage);
            }

            if (CurrentRun.IsCompleted)
            {
                Phase = RoguelikeGamePhase.Victory;
                LastMessage = "冒险胜利。点击重新开始。";
                return new RoguelikeCombatResult(true, 0, LastMessage);
            }

            RoguelikeRoom room = CurrentRun.CurrentRoom;
            if (room != null && room.IsCleared)
            {
                OpenRewardChoice(room);
                return new RoguelikeCombatResult(true, room.CombatTurnCount, LastMessage);
            }

            RoguelikeCombatResult result = _runService.ResolveCurrentRoomStep(CurrentRun);
            LastMessage = result.Summary;

            if (!CurrentRun.Player.IsAlive)
            {
                Phase = RoguelikeGamePhase.Defeated;
                LastMessage = "冒险失败。点击重新开始。";
            }
            else if (CurrentRun.IsCompleted)
            {
                Phase = RoguelikeGamePhase.Victory;
                LastMessage = "冒险胜利。点击重新开始。";
            }
            else if (CurrentRun.CurrentRoom != null && CurrentRun.CurrentRoom.IsCleared)
            {
                OpenRewardChoice(CurrentRun.CurrentRoom);
            }

            Log.Info(LastMessage);
            return result;
        }

        public void ChooseReward(int index)
        {
            if (CurrentRun == null || Phase != RoguelikeGamePhase.RewardChoice)
            {
                return;
            }

            if (index < 0 || index >= _rewardOptions.Count)
            {
                return;
            }

            RoguelikeChoiceOption option = _rewardOptions[index];
            if (!option.CanAfford(CurrentRun))
            {
                LastMessage = $"金币不足：{option.Title} 需要 {option.Cost} 金币。";
                return;
            }

            CurrentRun.TrySpendGold(option.Cost);
            option.Apply?.Invoke(CurrentRun);
            LastMessage = option.Cost > 0 ? $"已购买：{option.Title}。" : $"已选择：{option.Title}。";
            _rewardOptions.Clear();

            if (CurrentRun.IsCompleted)
            {
                Phase = RoguelikeGamePhase.Victory;
                LastMessage = "冒险胜利。点击重新开始。";
                return;
            }

            _runService.TryAdvanceAfterCleared(CurrentRun);
            RoguelikeRoom room = CurrentRun.CurrentRoom;
            LastMessage = room == null ? LastMessage : $"{LastMessage} 进入第 {room.Index + 1} 个房间：{GetRoomName(room.Type)}。";
            Phase = RoguelikeGamePhase.Running;
        }

        private void OpenRewardChoice(RoguelikeRoom clearedRoom)
        {
            if (CurrentRun.IsCompleted)
            {
                Phase = RoguelikeGamePhase.Victory;
                LastMessage = "冒险胜利。点击重新开始。";
                return;
            }

            if (clearedRoom.Type == RoguelikeRoomType.Start)
            {
                _runService.TryAdvanceAfterCleared(CurrentRun);
                RoguelikeRoom room = CurrentRun.CurrentRoom;
                Phase = RoguelikeGamePhase.Running;
                LastMessage = room == null ? "冒险开始。" : $"进入第 {room.Index + 1} 个房间：{GetRoomName(room.Type)}。";
                return;
            }

            _rewardOptions.Clear();
            bool isShop = clearedRoom.Type == RoguelikeRoomType.Shop;
            List<RoguelikeChoiceOption> pool = BuildChoicePoolForRoom(clearedRoom);
            Random random = new Random(CurrentRun.Seed + clearedRoom.Index * 3571 + clearedRoom.CombatTurnCount * 997);
            int randomOptionCount = isShop ? 2 : 3;
            while (_rewardOptions.Count < randomOptionCount && pool.Count > 0)
            {
                int index = random.Next(pool.Count);
                _rewardOptions.Add(pool[index]);
                pool.RemoveAt(index);
            }

            if (isShop)
            {
                _rewardOptions.Add(CreateLeaveShopOption());
            }

            Phase = RoguelikeGamePhase.RewardChoice;
            LastMessage = GetChoicePrompt(clearedRoom.Type);
        }

        private static List<RoguelikeChoiceOption> BuildChoicePoolForRoom(RoguelikeRoom clearedRoom)
        {
            switch (clearedRoom.Type)
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

        private static string GetChoicePrompt(RoguelikeRoomType roomType)
        {
            switch (roomType)
            {
                case RoguelikeRoomType.Rest:
                    return "营火：选择恢复方式。";
                case RoguelikeRoomType.Treasure:
                    return "宝箱：选择带走的奖励。";
                case RoguelikeRoomType.Shop:
                    return "商店：购买一件物品，或直接离开。";
                default:
                    return "请选择一个奖励继续。";
            }
        }

        private static string GetRoomName(RoguelikeRoomType type)
        {
            switch (type)
            {
                case RoguelikeRoomType.Start:
                    return "起点";
                case RoguelikeRoomType.Combat:
                    return "战斗";
                case RoguelikeRoomType.Elite:
                    return "精英";
                case RoguelikeRoomType.Treasure:
                    return "宝箱";
                case RoguelikeRoomType.Rest:
                    return "营火";
                case RoguelikeRoomType.Shop:
                    return "商店";
                case RoguelikeRoomType.Boss:
                    return "首领";
                default:
                    return type.ToString();
            }
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

        private static RoguelikeChoiceOption CreateLeaveShopOption()
        {
            return new RoguelikeChoiceOption("shop_leave", "离开商店", "保留金币", run => { });
        }
    }
}
