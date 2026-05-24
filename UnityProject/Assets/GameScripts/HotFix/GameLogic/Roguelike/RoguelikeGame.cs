using System;
using System.Collections.Generic;
using TEngine;

namespace GameLogic
{
    public sealed class RoguelikeGame : Singleton<RoguelikeGame>
    {
        private RoguelikeRunService _runService;
        private RoguelikeChoiceCatalog _choiceCatalog;

        public RoguelikeRunState CurrentRun { get; private set; }

        public string LastMessage { get; private set; } = "准备就绪。";
        
        public RoguelikeGamePhase Phase { get; private set; }

        public IReadOnlyList<RoguelikeChoiceOption> RewardOptions => _rewardOptions;

        private readonly List<RoguelikeChoiceOption> _rewardOptions = new List<RoguelikeChoiceOption>(3);

        protected override void OnInit()
        {
            _runService = new RoguelikeRunService(RoguelikeContentCatalog.CreateDefault());
            _choiceCatalog = new RoguelikeChoiceCatalog();
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
            LastMessage = room == null ? LastMessage : $"{LastMessage} 进入第 {room.Index + 1} 个房间：{RoguelikeText.GetRoomName(room.Type)}。";
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
                LastMessage = room == null ? "冒险开始。" : $"进入第 {room.Index + 1} 个房间：{RoguelikeText.GetRoomName(room.Type)}。";
                return;
            }

            _rewardOptions.Clear();
            bool isShop = clearedRoom.Type == RoguelikeRoomType.Shop;
            List<RoguelikeChoiceOption> pool = _choiceCatalog.CreateChoicePool(clearedRoom.Type);
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
                _rewardOptions.Add(_choiceCatalog.CreateLeaveShopOption());
            }

            Phase = RoguelikeGamePhase.RewardChoice;
            LastMessage = RoguelikeText.GetChoicePrompt(clearedRoom.Type);
        }
    }
}
