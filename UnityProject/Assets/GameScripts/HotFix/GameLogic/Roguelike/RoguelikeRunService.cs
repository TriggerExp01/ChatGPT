using System;
using System.Collections.Generic;
using System.Text;

namespace GameLogic
{
    public sealed class RoguelikeRunService
    {
        private readonly RoguelikeContentCatalog _catalog;

        public RoguelikeRunService(RoguelikeContentCatalog catalog)
        {
            _catalog = catalog;
        }

        public RoguelikeRunState CreateRun(RoguelikeRunConfig config)
        {
            Random random = new Random(config.Seed);
            RoguelikeActorState player = new RoguelikeActorState("player", "冒险者", config.PlayerStats.Clone());
            List<RoguelikeRoom> rooms = CreateRooms(config, random);
            return new RoguelikeRunState(config.Seed, player, rooms);
        }

        public RoguelikeCombatResult ResolveCurrentRoom(RoguelikeRunState run)
        {
            RoguelikeRoom room = run.CurrentRoom;
            if (room == null || room.IsCleared)
            {
                return new RoguelikeCombatResult(true, 0, "当前没有战斗。");
            }

            RoguelikeCombatResult result = null;
            int guard = 0;
            while (run.Player.IsAlive && !room.IsCleared && guard < 100)
            {
                result = ResolveCurrentRoomStep(run);
                guard++;
            }

            return result ?? new RoguelikeCombatResult(run.Player.IsAlive, room.CombatTurnCount, "当前没有战斗。");
        }

        public RoguelikeCombatResult ResolveCurrentRoomStep(RoguelikeRunState run)
        {
            RoguelikeRoom room = run.CurrentRoom;
            if (room == null)
            {
                return new RoguelikeCombatResult(true, 0, "本轮冒险已结束。");
            }

            if (room.IsCleared)
            {
                return new RoguelikeCombatResult(run.Player.IsAlive, room.CombatTurnCount, $"第 {room.Index + 1} 个房间已清理。");
            }

            if (room.Enemy == null)
            {
                ApplyReward(run, room.Reward);
                room.MarkCleared();
                return new RoguelikeCombatResult(true, room.CombatTurnCount, $"第 {room.Index + 1} 个房间（{GetRoomName(room.Type)}）：{DescribeReward(room.Reward)}");
            }

            int turn = room.AdvanceCombatTurn();
            Random random = new Random(run.Seed + room.Index * 7919 + turn * 104729);
            StringBuilder summary = new StringBuilder();

            int playerDamage = RollDamage(run.Player, random);
            int dealt = room.Enemy.TakeDamage(playerDamage);
            summary.Append($"第 {turn} 回合：你对 {room.Enemy.DisplayName} 造成 {dealt} 点伤害。");

            if (!room.Enemy.IsAlive)
            {
                ApplyReward(run, room.Reward);
                room.MarkCleared();
                summary.Append($" 战斗胜利。{DescribeReward(room.Reward)}");
                return new RoguelikeCombatResult(true, turn, summary.ToString());
            }

            int enemyDamage = RollDamage(room.Enemy, random);
            int taken = run.Player.TakeDamage(enemyDamage);
            summary.Append($" {room.Enemy.DisplayName} 对你造成 {taken} 点伤害。");

            if (!run.Player.IsAlive)
            {
                summary.Append(" 你倒下了。");
            }

            return new RoguelikeCombatResult(run.Player.IsAlive, turn, summary.ToString());
        }

        public bool TryAdvanceAfterCleared(RoguelikeRunState run)
        {
            if (run == null || run.IsCompleted)
            {
                return false;
            }

            RoguelikeRoom room = run.CurrentRoom;
            return room != null && room.IsCleared && run.MoveNextRoom();
        }

        public string BuildRunPreview(RoguelikeRunState run)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"种子：{run.Seed}");
            builder.AppendLine($"角色：生命 {run.Player.Health}/{run.Player.Stats.MaxHealth}，攻击 {run.Player.Stats.Attack}，防御 {run.Player.Stats.Defense}");
            for (int i = 0; i < run.Rooms.Count; i++)
            {
                RoguelikeRoom room = run.Rooms[i];
                string enemy = room.Enemy == null ? "-" : room.Enemy.DisplayName;
                builder.AppendLine($"{i + 1:00}. {GetRoomName(room.Type)} / {enemy}");
            }

            return builder.ToString();
        }

        private List<RoguelikeRoom> CreateRooms(RoguelikeRunConfig config, Random random)
        {
            List<RoguelikeRoom> rooms = new List<RoguelikeRoom>(config.MaxRooms);
            for (int i = 0; i < config.MaxRooms; i++)
            {
                RoguelikeRoomType roomType = PickRoomType(i, config, random);
                RoguelikeActorState enemy = CreateEnemy(roomType, i + 1, random);
                RoguelikeReward reward = CreateReward(roomType, i + 1, random);
                rooms.Add(new RoguelikeRoom(i, roomType, enemy, reward));
            }

            return rooms;
        }

        private RoguelikeRoomType PickRoomType(int index, RoguelikeRunConfig config, Random random)
        {
            if (index == 0)
            {
                return RoguelikeRoomType.Start;
            }

            if (index == config.MaxRooms - 1)
            {
                return RoguelikeRoomType.Boss;
            }

            if (config.EliteInterval > 0 && index % config.EliteInterval == 0)
            {
                return RoguelikeRoomType.Elite;
            }

            int roll = random.Next(100);
            if (roll < 60)
            {
                return RoguelikeRoomType.Combat;
            }

            if (roll < 75)
            {
                return RoguelikeRoomType.Treasure;
            }

            if (roll < 90)
            {
                return RoguelikeRoomType.Rest;
            }

            return RoguelikeRoomType.Shop;
        }

        private RoguelikeActorState CreateEnemy(RoguelikeRoomType roomType, int depth, Random random)
        {
            if (roomType == RoguelikeRoomType.Combat)
            {
                return Pick(_catalog.CommonEnemies, random).CreateActor(depth);
            }

            if (roomType == RoguelikeRoomType.Elite)
            {
                return Pick(_catalog.EliteEnemies, random).CreateActor(depth);
            }

            if (roomType == RoguelikeRoomType.Boss)
            {
                return _catalog.BossEnemy.CreateActor(depth);
            }

            return null;
        }

        private RoguelikeReward CreateReward(RoguelikeRoomType roomType, int depth, Random random)
        {
            if (roomType == RoguelikeRoomType.Start)
            {
                return new RoguelikeReward(RoguelikeRewardType.None);
            }

            if (roomType == RoguelikeRoomType.Rest)
            {
                return new RoguelikeReward(RoguelikeRewardType.Heal, 16 + depth);
            }

            if (roomType == RoguelikeRoomType.Treasure || roomType == RoguelikeRoomType.Elite || roomType == RoguelikeRoomType.Boss)
            {
                return new RoguelikeReward(RoguelikeRewardType.Relic, relic: Pick(_catalog.Relics, random));
            }

            if (roomType == RoguelikeRoomType.Shop)
            {
                return new RoguelikeReward(RoguelikeRewardType.None);
            }

            return new RoguelikeReward(RoguelikeRewardType.Gold, 8 + depth * 2);
        }

        private void ApplyReward(RoguelikeRunState run, RoguelikeReward reward)
        {
            if (reward == null)
            {
                return;
            }

            switch (reward.Type)
            {
                case RoguelikeRewardType.Gold:
                    run.AddGold(reward.Amount);
                    break;
                case RoguelikeRewardType.Heal:
                    run.Player.Heal(reward.Amount);
                    break;
                case RoguelikeRewardType.Relic:
                    run.AddRelic(reward.Relic);
                    break;
            }
        }

        private static string DescribeReward(RoguelikeReward reward)
        {
            if (reward == null || reward.Type == RoguelikeRewardType.None)
            {
                return "没有奖励。";
            }

            switch (reward.Type)
            {
                case RoguelikeRewardType.Gold:
                    return $"获得 {reward.Amount} 金币。";
                case RoguelikeRewardType.Heal:
                    return $"恢复 {reward.Amount} 点生命。";
                case RoguelikeRewardType.Relic:
                    return reward.Relic == null ? "没有获得遗物。" : $"获得遗物：{reward.Relic.DisplayName}。";
                default:
                    return "没有奖励。";
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

        private static T Pick<T>(IReadOnlyList<T> items, Random random)
        {
            return items[random.Next(items.Count)];
        }
    }
}
