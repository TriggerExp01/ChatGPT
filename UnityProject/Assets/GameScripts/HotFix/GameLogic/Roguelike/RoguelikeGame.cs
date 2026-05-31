using System;
using System.Collections.Generic;
using TEngine;

namespace GameLogic
{
    public sealed class RoguelikeGame : Singleton<RoguelikeGame>
    {
        private RoguelikeRunService _runService;
        private RoguelikeConfigModule _configModule;
        private RoguelikeProgressionModule _progressionModule;
        private RealtimeCombatState _realtimeState;
        private float _moveAxisInput;

        public RoguelikeRunState CurrentRun { get; private set; }
        public bool IsPaused { get; private set; }

        public string LastMessage { get; private set; } = "准备就绪。";
        
        public RoguelikeGamePhase Phase { get; private set; }

        public IReadOnlyList<RoguelikeChoiceOption> RewardOptions => _rewardOptions;
        public float SkillCooldownRemaining => _realtimeState?.SkillCooldownRemaining ?? 0f;
        public float DashCooldownRemaining => _realtimeState?.DashCooldownRemaining ?? 0f;
        public float PlayerLanePosition => _realtimeState?.PlayerPosition ?? 0f;
        public float EnemyLanePosition => _realtimeState?.EnemyPosition ?? 0f;
        public bool InRealtimeCombat => IsRealtimeCombatRoom(CurrentRun?.CurrentRoom) && Phase == RoguelikeGamePhase.Running;
        public string SettlementSummary => BuildSettlementSummary(CurrentRun);

        private readonly List<RoguelikeChoiceOption> _rewardOptions = new List<RoguelikeChoiceOption>(3);

        protected override void OnInit()
        {
            ConfigSystem.Instance.Load();
            var tables = ConfigSystem.Instance.Tables;
            _configModule = new RoguelikeConfigModule(RoguelikeContentCatalog.CreateFromLuban(tables), new RoguelikeChoiceCatalog(tables));
            _progressionModule = new RoguelikeProgressionModule(_configModule);
            _runService = new RoguelikeRunService(_configModule.ContentCatalog);

            if (!_configModule.Validate(out string message))
            {
                Log.Error($"肉鸽配置校验失败：{message}");
            }
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
            _realtimeState = null;
            IsPaused = false;
            _moveAxisInput = 0f;
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
            return UpdateRealtimeCombat(0.2f);
        }

        public RoguelikeCombatResult Tick(float deltaTime, float moveAxisInput)
        {
            SetMoveInput(moveAxisInput);
            return Tick(deltaTime);
        }

        public RoguelikeCombatResult Tick(float deltaTime)
        {
            if (CurrentRun == null)
            {
                StartNewRun();
            }

            if (Phase == RoguelikeGamePhase.Running)
            {
                return UpdateRealtimeCombat(deltaTime);
            }

            return new RoguelikeCombatResult(CurrentRun.Player.IsAlive, CurrentRun.CurrentRoom?.CombatTurnCount ?? 0, LastMessage);
        }

        public void SetMoveInput(float axis)
        {
            if (axis < -1f)
            {
                axis = -1f;
            }
            else if (axis > 1f)
            {
                axis = 1f;
            }

            _moveAxisInput = axis;
            if (_realtimeState != null)
            {
                _realtimeState.MoveAxisInput = axis;
            }
        }

        public void RequestSkill()
        {
            if (_realtimeState != null)
            {
                _realtimeState.SkillRequested = true;
            }
        }

        public void RequestDash()
        {
            if (_realtimeState != null)
            {
                _realtimeState.DashRequested = true;
            }
        }

        public void DebugAddGold(int amount)
        {
            if (CurrentRun == null)
            {
                StartNewRun();
            }

            int delta = Math.Max(1, amount);
            CurrentRun.AddGold(delta);
            LastMessage = $"调试：金币 +{delta}。";
        }

        public void DebugAddRandomRelic()
        {
            if (CurrentRun == null)
            {
                StartNewRun();
            }

            IReadOnlyList<RoguelikeRelicTemplate> relics = _configModule.ContentCatalog.Relics;
            if (relics == null || relics.Count == 0)
            {
                LastMessage = "调试：没有可用遗物。";
                return;
            }

            int seed = CurrentRun.Seed ^ (CurrentRun.CurrentRoomIndex + 1) * 1297 ^ CurrentRun.Relics.Count * 3571;
            Random random = new Random(seed);
            RoguelikeRelicTemplate relic = relics[random.Next(relics.Count)];
            CurrentRun.AddRelic(relic);
            LastMessage = $"调试：获得遗物 {relic.DisplayName}。";
        }

        public void DebugSkipRoom(int count = 1)
        {
            if (CurrentRun == null)
            {
                StartNewRun();
            }

            int steps = Math.Max(1, count);
            for (int i = 0; i < steps; i++)
            {
                RoguelikeRoom room = CurrentRun.CurrentRoom;
                if (room == null)
                {
                    break;
                }

                room.MarkCleared();
                if (!_runService.TryAdvanceAfterCleared(CurrentRun))
                {
                    break;
                }
            }

            _realtimeState = null;
            _rewardOptions.Clear();

            if (CurrentRun.IsCompleted)
            {
                Phase = RoguelikeGamePhase.Victory;
                LastMessage = "调试：已跳至终点并判定胜利。";
                return;
            }

            Phase = RoguelikeGamePhase.Running;
            RoguelikeRoom currentRoom = CurrentRun.CurrentRoom;
            LastMessage = currentRoom == null
                ? "调试：当前无房间。"
                : $"调试：跳转到第 {currentRoom.Index + 1} 个房间（{RoguelikeText.GetRoomName(currentRoom.Type)}）。";
        }

        public void DebugForceVictory()
        {
            if (CurrentRun == null)
            {
                StartNewRun();
            }

            for (int i = CurrentRun.CurrentRoomIndex; i < CurrentRun.Rooms.Count; i++)
            {
                CurrentRun.Rooms[i].MarkCleared();
            }

            while (_runService.TryAdvanceAfterCleared(CurrentRun))
            {
            }

            _realtimeState = null;
            _rewardOptions.Clear();
            Phase = RoguelikeGamePhase.Victory;
            LastMessage = "调试：强制胜利。";
            IsPaused = false;
        }

        public void DebugForceDefeat()
        {
            if (CurrentRun == null)
            {
                StartNewRun();
            }

            CurrentRun.Player.TakeDamage(int.MaxValue);
            _realtimeState = null;
            _rewardOptions.Clear();
            Phase = RoguelikeGamePhase.Defeated;
            LastMessage = "调试：强制失败。";
            IsPaused = false;
        }

        public void TogglePause()
        {
            if (Phase != RoguelikeGamePhase.Running)
            {
                IsPaused = false;
                return;
            }

            IsPaused = !IsPaused;
            LastMessage = IsPaused ? "已暂停。" : "继续战斗。";
        }

        public string RunSmokeSimulation(int runCount, int maxStepsPerRun = 512)
        {
            int total = Math.Max(1, runCount);
            int wins = 0;
            int fails = 0;
            int totalRooms = 0;
            int totalTurns = 0;
            int totalDamageTaken = 0;
            int totalDamageDealt = 0;

            for (int i = 0; i < total; i++)
            {
                StartNewRun(unchecked((int)DateTime.UtcNow.Ticks) + i * 97);
                int guard = 0;
                while (guard < maxStepsPerRun && Phase == RoguelikeGamePhase.Running)
                {
                    UpdateRealtimeCombat(0.2f);
                    if (Phase == RoguelikeGamePhase.RewardChoice)
                    {
                        ChooseFirstAffordableReward();
                    }

                    guard++;
                }

                if (Phase == RoguelikeGamePhase.Victory)
                {
                    wins++;
                }
                else
                {
                    fails++;
                }

                if (CurrentRun != null)
                {
                    totalRooms += CurrentRun.ClearedRoomCount;
                    totalTurns += CurrentRun.TotalTurns;
                    totalDamageTaken += CurrentRun.TotalDamageTaken;
                    totalDamageDealt += CurrentRun.TotalDamageDealt;
                }
            }

            return $"SmokeTest 轮次 {total}，胜利 {wins}，失败 {fails}，均值清房 {(float)totalRooms / total:0.00}，均值回合 {(float)totalTurns / total:0.00}，均值承伤 {(float)totalDamageTaken / total:0.0}，均值输出 {(float)totalDamageDealt / total:0.0}。";
        }

        public RoguelikeCombatResult UpdateRealtimeCombat(float deltaTime)
        {
            if (CurrentRun == null)
            {
                StartNewRun();
            }

            if (IsPaused)
            {
                return new RoguelikeCombatResult(true, 0, "已暂停。");
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
                _realtimeState = null;
                OpenRewardChoice(room);
                return new RoguelikeCombatResult(true, room.CombatTurnCount, LastMessage);
            }

            if (room != null && room.Enemy == null)
            {
                RoguelikeCombatResult eventResult = _runService.ResolveCurrentRoomStep(CurrentRun);
                LastMessage = eventResult.Summary;
                if (CurrentRun.CurrentRoom != null && CurrentRun.CurrentRoom.IsCleared)
                {
                    OpenRewardChoice(CurrentRun.CurrentRoom);
                }

                return eventResult;
            }

            EnsureRealtimeState(room);
            RoguelikeCombatResult result = StepRealtimeCombat(room, deltaTime);
            LastMessage = result.Summary;

            if (!CurrentRun.Player.IsAlive)
            {
                Phase = RoguelikeGamePhase.Defeated;
                LastMessage = "冒险失败。点击重新开始。";
                _realtimeState = null;
            }
            else if (CurrentRun.IsCompleted)
            {
                Phase = RoguelikeGamePhase.Victory;
                LastMessage = "冒险胜利。点击重新开始。";
                _realtimeState = null;
            }
            else if (CurrentRun.CurrentRoom != null && CurrentRun.CurrentRoom.IsCleared)
            {
                _realtimeState = null;
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
            if (!_progressionModule.TryApplyChoice(CurrentRun, option, out string message))
            {
                LastMessage = message;
                return;
            }

            LastMessage = message;
            _rewardOptions.Clear();
            _realtimeState = null;

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
                _realtimeState = null;
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
            _progressionModule.BuildRewardOptions(CurrentRun, clearedRoom, _rewardOptions);

            Phase = RoguelikeGamePhase.RewardChoice;
            LastMessage = RoguelikeText.GetChoicePrompt(clearedRoom.Type);
        }

        private void ChooseFirstAffordableReward()
        {
            if (CurrentRun == null || Phase != RoguelikeGamePhase.RewardChoice)
            {
                return;
            }

            for (int i = 0; i < _rewardOptions.Count; i++)
            {
                if (_rewardOptions[i].CanAfford(CurrentRun))
                {
                    ChooseReward(i);
                    return;
                }
            }

            if (_rewardOptions.Count > 0)
            {
                ChooseReward(0);
            }
        }

        private static string BuildSettlementSummary(RoguelikeRunState run)
        {
            if (run == null)
            {
                return "Run not started.";
            }

            string phase = run.Player.IsAlive
                ? (run.IsCompleted ? "Victory" : "Running")
                : "Defeated";
            return $"Settlement: {phase} | Rooms {run.ClearedRoomCount}/{run.Rooms.Count} | Turns {run.TotalTurns} | DamageDealt {run.TotalDamageDealt} | DamageTaken {run.TotalDamageTaken} | Gold {run.Gold} | Relics {run.Relics.Count}";
        }

        private void EnsureRealtimeState(RoguelikeRoom room)
        {
            if (_realtimeState != null && _realtimeState.RoomIndex == room.Index)
            {
                return;
            }

            _realtimeState = RealtimeCombatState.Create(CurrentRun.Seed, room);
            _realtimeState.MoveAxisInput = _moveAxisInput;
            LastMessage = $"进入实时战斗：{room.Enemy.DisplayName}。";
        }

        private RoguelikeCombatResult StepRealtimeCombat(RoguelikeRoom room, float deltaTime)
        {
            if (_realtimeState == null || room == null || room.Enemy == null)
            {
                return new RoguelikeCombatResult(true, room?.CombatTurnCount ?? 0, LastMessage);
            }

            float dt = deltaTime;
            if (dt < 0f)
            {
                dt = 0f;
            }
            else if (dt > 0.2f)
            {
                dt = 0.2f;
            }

            _realtimeState.TickTimers(dt);

            if (_realtimeState.DashRequested && _realtimeState.DashCooldownRemaining <= 0f)
            {
                _realtimeState.DashRequested = false;
                _realtimeState.DashCooldownRemaining = _realtimeState.DashCooldown;
                _realtimeState.DashRemaining = _realtimeState.DashDuration;
                if (Math.Abs(_realtimeState.MoveAxisInput) > 0.01f)
                {
                    _realtimeState.Facing = _realtimeState.MoveAxisInput > 0f ? 1 : -1;
                }

                _realtimeState.DashDirection = _realtimeState.Facing;
                LastMessage = "触发闪避位移。";
            }
            else
            {
                _realtimeState.DashRequested = false;
            }

            if (_realtimeState.SkillRequested)
            {
                _realtimeState.SkillRequested = false;
                if (_realtimeState.SkillCooldownRemaining <= 0f)
                {
                    float dist = Math.Abs(_realtimeState.EnemyPosition - _realtimeState.PlayerPosition);
                    _realtimeState.SkillCooldownRemaining = _realtimeState.SkillCooldown;
                    if (dist <= _realtimeState.SkillRange)
                    {
                        int skillDamage = Math.Max(1, CurrentRun.Player.Stats.Attack * 2 + 4);
                        int dealt = room.Enemy.TakeDamage(skillDamage);
                        room.RecordDamageDealt(dealt);
                        CurrentRun.RecordDamageDealt(dealt);
                        CurrentRun.RecordTurn();
                        room.AdvanceCombatTurn();
                        LastMessage = $"主动技能命中，造成 {dealt} 点伤害。";
                    }
                    else
                    {
                        LastMessage = "主动技能落空。";
                    }
                }
                else
                {
                    LastMessage = $"主动技能冷却中：{_realtimeState.SkillCooldownRemaining:0.0}s";
                }
            }

            float moveAxis = _realtimeState.MoveAxisInput;
            if (Math.Abs(moveAxis) > 0.01f)
            {
                _realtimeState.Facing = moveAxis > 0f ? 1 : -1;
            }
            else if (_realtimeState.DashRemaining > 0f)
            {
                moveAxis = _realtimeState.DashDirection;
            }

            float moveSpeed = _realtimeState.DashRemaining > 0f ? _realtimeState.DashSpeed : _realtimeState.MoveSpeed;
            _realtimeState.PlayerPosition = Clamp(_realtimeState.PlayerPosition + moveAxis * moveSpeed * dt, -_realtimeState.LaneBound, _realtimeState.LaneBound);

            float distance = Math.Abs(_realtimeState.EnemyPosition - _realtimeState.PlayerPosition);
            if (_realtimeState.PlayerAttackCooldownRemaining <= 0f && distance <= _realtimeState.PlayerAttackRange && room.Enemy.IsAlive)
            {
                int playerDamage = RollDamage(CurrentRun.Player, _realtimeState.Random);
                int dealt = room.Enemy.TakeDamage(playerDamage);
                room.RecordDamageDealt(dealt);
                CurrentRun.RecordDamageDealt(dealt);
                CurrentRun.RecordTurn();
                room.AdvanceCombatTurn();
                _realtimeState.PlayerAttackCooldownRemaining = _realtimeState.PlayerAttackInterval;
                LastMessage = $"自动普攻命中，造成 {dealt} 点伤害。";
            }

            if (room.Enemy.IsAlive)
            {
                distance = Math.Abs(_realtimeState.EnemyPosition - _realtimeState.PlayerPosition);
                int chaseDirection = _realtimeState.EnemyPosition < _realtimeState.PlayerPosition ? 1 : -1;
                if (distance > _realtimeState.EnemyAttackRange)
                {
                    _realtimeState.EnemyPosition = Clamp(
                        _realtimeState.EnemyPosition + chaseDirection * _realtimeState.EnemyMoveSpeed * dt,
                        -_realtimeState.LaneBound,
                        _realtimeState.LaneBound);
                }
                else if (_realtimeState.EnemyAttackCooldownRemaining <= 0f)
                {
                    int enemyDamage = RollDamage(room.Enemy, _realtimeState.Random);
                    int taken = CurrentRun.Player.TakeDamage(enemyDamage);
                    if (_realtimeState.DashRemaining > 0f)
                    {
                        int reduced = (int)Math.Ceiling(taken * 0.5f);
                        CurrentRun.Player.Heal(reduced);
                        taken -= reduced;
                    }

                    room.RecordDamageTaken(taken);
                    CurrentRun.RecordDamageTaken(taken);
                    _realtimeState.EnemyAttackCooldownRemaining = _realtimeState.EnemyAttackInterval;
                    LastMessage = $"{room.Enemy.DisplayName} 命中你，造成 {taken} 点伤害。";
                }
            }

            if (!room.Enemy.IsAlive)
            {
                RoguelikeCombatResult settle = _runService.CompleteRoomAfterRealtimeCombat(CurrentRun, "战斗胜利。");
                return settle;
            }

            return new RoguelikeCombatResult(CurrentRun.Player.IsAlive, room.CombatTurnCount, LastMessage);
        }

        private static int RollDamage(RoguelikeActorState actor, Random random)
        {
            int damage = actor.Stats.Attack;
            bool critical = random.NextDouble() < actor.Stats.CritChance;
            if (critical)
            {
                damage = (int)Math.Ceiling(damage * actor.Stats.CritMultiplier);
            }

            return Math.Max(1, damage);
        }

        private static bool IsRealtimeCombatRoom(RoguelikeRoom room)
        {
            return room != null && room.Enemy != null && !room.IsCleared;
        }

        private static float Clamp(float value, float min, float max)
        {
            if (value < min)
            {
                return min;
            }

            return value > max ? max : value;
        }

        private sealed class RealtimeCombatState
        {
            public int RoomIndex { get; private set; }
            public Random Random { get; private set; }
            public float PlayerPosition;
            public float EnemyPosition;
            public float MoveAxisInput;
            public int Facing;
            public int DashDirection;
            public bool SkillRequested;
            public bool DashRequested;

            public float PlayerAttackCooldownRemaining;
            public float EnemyAttackCooldownRemaining;
            public float SkillCooldownRemaining;
            public float DashCooldownRemaining;
            public float DashRemaining;

            public float MoveSpeed;
            public float DashSpeed;
            public float DashDuration;
            public float DashCooldown;
            public float SkillRange;
            public float SkillCooldown;
            public float PlayerAttackRange;
            public float PlayerAttackInterval;
            public float EnemyAttackRange;
            public float EnemyAttackInterval;
            public float EnemyMoveSpeed;
            public float LaneBound;

            public static RealtimeCombatState Create(int seed, RoguelikeRoom room)
            {
                bool isBoss = room.Type == RoguelikeRoomType.Boss;
                bool isElite = room.Type == RoguelikeRoomType.Elite;
                return new RealtimeCombatState
                {
                    RoomIndex = room.Index,
                    Random = new Random(seed ^ (room.Index + 1) * 7919),
                    PlayerPosition = -2.5f,
                    EnemyPosition = isBoss ? 4f : 3f,
                    Facing = 1,
                    DashDirection = 1,
                    MoveSpeed = 4f,
                    DashSpeed = 11f,
                    DashDuration = 0.2f,
                    DashCooldown = 2.5f,
                    SkillRange = 2.2f,
                    SkillCooldown = 4f,
                    PlayerAttackRange = 1.2f,
                    PlayerAttackInterval = 0.55f,
                    EnemyAttackRange = isBoss ? 1.4f : 1.1f,
                    EnemyAttackInterval = isBoss ? 0.8f : (isElite ? 1.0f : 1.2f),
                    EnemyMoveSpeed = isBoss ? 3.2f : (isElite ? 2.8f : 2.4f),
                    LaneBound = 6f,
                };
            }

            public void TickTimers(float dt)
            {
                PlayerAttackCooldownRemaining = Decrease(PlayerAttackCooldownRemaining, dt);
                EnemyAttackCooldownRemaining = Decrease(EnemyAttackCooldownRemaining, dt);
                SkillCooldownRemaining = Decrease(SkillCooldownRemaining, dt);
                DashCooldownRemaining = Decrease(DashCooldownRemaining, dt);
                DashRemaining = Decrease(DashRemaining, dt);
            }

            private static float Decrease(float value, float dt)
            {
                value -= dt;
                return value > 0f ? value : 0f;
            }
        }
    }
}
